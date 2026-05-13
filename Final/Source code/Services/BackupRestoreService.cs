using Npgsql;
using ProjectTest.Models;

namespace ProjectTest.Services;

public class BackupRestoreService
{
    private readonly DatabaseOptions _databaseOptions;
    private string _postgresToolsDirectory = string.Empty;

    public BackupRestoreService(DatabaseOptions databaseOptions)
    {
        _databaseOptions = databaseOptions;
    }

    public string PostgreSqlToolsDirectory
    {
        get => _postgresToolsDirectory;
        set => _postgresToolsDirectory = value.Trim();
    }

    public async Task<OperationResult<string>> BackupAsync(string targetFile)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(targetFile) ?? AppContext.BaseDirectory);
            var builder = new NpgsqlConnectionStringBuilder(_databaseOptions.ConnectionString);
            var dumpTool = FindTool("pg_dump", PostgreSqlToolsDirectory);
            if (dumpTool is null)
            {
                return OperationResult<string>.Fail("pg_dump was not found. Browse to the PostgreSQL bin folder or install PostgreSQL client tools.");
            }

            var result = await RunProcessAsync(dumpTool, $"-Fc --file \"{targetFile}\" \"{builder.ConnectionString}\"", Path.GetDirectoryName(dumpTool));
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

        var restoreTool = FindTool("pg_restore", PostgreSqlToolsDirectory);
        if (restoreTool is null)
        {
            return OperationResult.Fail("pg_restore was not found. Browse to the PostgreSQL bin folder or install PostgreSQL client tools.");
        }

        var safetyBackup = Path.Combine(Path.GetTempPath(), $"myshop-before-restore-{DateTime.UtcNow:yyyyMMddHHmmss}.dump");
        var backup = await BackupAsync(safetyBackup);
        if (!backup.Success)
        {
            return OperationResult.Fail($"Restore cancelled because safety backup failed: {backup.Message}");
        }

        var builder = new NpgsqlConnectionStringBuilder(_databaseOptions.ConnectionString);
        var result = await RunProcessAsync(restoreTool, $"--clean --if-exists --no-owner --dbname \"{builder.ConnectionString}\" \"{sourceFile}\"", Path.GetDirectoryName(restoreTool));
        return result.ExitCode == 0
            ? OperationResult.Ok("Restore completed.")
            : OperationResult.Fail($"Restore failed. Existing data was preserved in {safetyBackup}. {result.Error}");
    }

    public static string? FindTool(string name, string? preferredDirectory = null)
    {
        foreach (var directory in EnumerateToolDirectories(preferredDirectory))
        {
            var candidate = Path.Combine(directory, $"{name}.exe");
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    public static string GetToolStatus(string? preferredDirectory = null)
    {
        var dumpTool = FindTool("pg_dump", preferredDirectory);
        var restoreTool = FindTool("pg_restore", preferredDirectory);
        if (dumpTool is not null && restoreTool is not null)
        {
            return $"PostgreSQL tools found: {Path.GetDirectoryName(dumpTool)}";
        }

        return "PostgreSQL tools not found. Browse to a PostgreSQL bin folder or install PostgreSQL client tools.";
    }

    private static IEnumerable<string> EnumerateToolDirectories(string? preferredDirectory)
    {
        if (!string.IsNullOrWhiteSpace(preferredDirectory))
        {
            yield return preferredDirectory.Trim();
        }

        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var directory in path.Split(Path.PathSeparator))
        {
            if (!string.IsNullOrWhiteSpace(directory))
            {
                yield return directory.Trim();
            }
        }

        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var candidates = new[]
        {
            Path.Combine(programFiles, "PostgreSQL"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "PostgreSQL")
        };

        foreach (var root in candidates.Where(Directory.Exists))
        {
            foreach (var bin in Directory.EnumerateDirectories(root, "bin", SearchOption.AllDirectories))
            {
                yield return bin;
            }
        }
    }

    private static async Task<(int ExitCode, string Error)> RunProcessAsync(string fileName, string arguments, string? toolDirectory)
    {
        using var process = new System.Diagnostics.Process();
        process.StartInfo = new System.Diagnostics.ProcessStartInfo(fileName, arguments)
        {
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };
        if (!string.IsNullOrWhiteSpace(toolDirectory))
        {
            var currentPath = process.StartInfo.EnvironmentVariables["PATH"];
            process.StartInfo.EnvironmentVariables["PATH"] = $"{toolDirectory}{Path.PathSeparator}{currentPath}";
        }

        process.Start();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return (process.ExitCode, error);
    }
}
