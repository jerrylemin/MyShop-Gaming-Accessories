using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.ApplicationModel.DynamicDependency;
using Microsoft.Windows.ApplicationModel.WindowsAppRuntime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WinRT;

namespace ProjectTest;

public static class Program
{
    private const uint WindowsAppSdkMajorMinorVersion = 0x00010008;
    private const int AppModelErrorNoPackage = 15700;

    [STAThread]
    public static void Main(string[] args)
    {
        var bootstrapped = false;

        try
        {
            ComWrappersSupport.InitializeComWrappers();

            if (IsRunningPackaged())
            {
                DeploymentManager.Initialize();
            }
            else
            {
                Bootstrap.Initialize(WindowsAppSdkMajorMinorVersion);
                bootstrapped = true;
            }

            StartWinUIApplication();
        }
        finally
        {
            if (bootstrapped)
            {
                Bootstrap.Shutdown();
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void StartWinUIApplication()
    {
        Application.Start(_ =>
        {
            var context = new DispatcherQueueSynchronizationContext(
                DispatcherQueue.GetForCurrentThread());

            SynchronizationContext.SetSynchronizationContext(context);
            var app = new App();
        });
    }

    private static bool IsRunningPackaged()
    {
        var length = 0;
        var result = GetCurrentPackageFullName(ref length, null);
        return result != AppModelErrorNoPackage;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int GetCurrentPackageFullName(
        ref int packageFullNameLength,
        string? packageFullName);
}
