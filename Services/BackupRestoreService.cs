using Npgsql;
using ProjectTest.Models;

namespace ProjectTest.Services;

public class BackupRestoreService
{
    private readonly DatabaseOptions _databaseOptions;

    public BackupRestoreService(DatabaseOptions databaseOptions)
    {
        _databaseOptions = databaseOptions;
    }

    public async Task<OperationResult<string>> BackupAsync(string targetFile)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(targetFile) ?? AppContext.BaseDirectory);
            var builder = new NpgsqlConnectionStringBuilder(_databaseOptions.ConnectionString);
            var dumpTool = FindTool("pg_dump");
            if (dumpTool is null)
            {
                return OperationResult<string>.Fail("pg_dump was not found in PATH. Install PostgreSQL client tools to use backup.");
            }

            var result = await RunProcessAsync(dumpTool, $"-Fc --file \"{targetFile}\" \"{builder.ConnectionString}\"");
            return result.ExitCode == 0
                ? OperationResult<string>.Ok(targetFile, "Backup completed.")
                : OperationResult<string>.Fail(result.Error);
        }
        catch (Exception ex)
        {
            return OperationResult<string>.Fail(ex.Message);
        }
    }

    public async Task<OperationResult> RestoreAsync(string sourceFile)
    {
        if (!File.Exists(sourceFile))
        {
            return OperationResult.Fail("Backup file was not found.");
        }

        var restoreTool = FindTool("pg_restore");
        if (restoreTool is null)
        {
            return OperationResult.Fail("pg_restore was not found in PATH. Install PostgreSQL client tools to use restore.");
        }

        var safetyBackup = Path.Combine(Path.GetTempPath(), $"myshop-before-restore-{DateTime.UtcNow:yyyyMMddHHmmss}.dump");
        var backup = await BackupAsync(safetyBackup);
        if (!backup.Success)
        {
            return OperationResult.Fail($"Restore cancelled because safety backup failed: {backup.Message}");
        }

        var builder = new NpgsqlConnectionStringBuilder(_databaseOptions.ConnectionString);
        var result = await RunProcessAsync(restoreTool, $"--clean --if-exists --no-owner --dbname \"{builder.ConnectionString}\" \"{sourceFile}\"");
        return result.ExitCode == 0
            ? OperationResult.Ok("Restore completed.")
            : OperationResult.Fail($"Restore failed. Existing data was preserved in {safetyBackup}. {result.Error}");
    }

    private static string? FindTool(string name)
    {
        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var directory in path.Split(Path.PathSeparator))
        {
            var candidate = Path.Combine(directory.Trim(), $"{name}.exe");
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static async Task<(int ExitCode, string Error)> RunProcessAsync(string fileName, string arguments)
    {
        using var process = new System.Diagnostics.Process();
        process.StartInfo = new System.Diagnostics.ProcessStartInfo(fileName, arguments)
        {
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };
        process.Start();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return (process.ExitCode, error);
    }
}
