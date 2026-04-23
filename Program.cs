using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.ApplicationModel.DynamicDependency;
using System.Runtime.InteropServices;
using WinRT;

namespace ProjectTest;

public static class Program
{
    private const uint WindowsAppSdkMajorMinorVersion = 0x00010008;

    [STAThread]
    public static void Main(string[] args)
    {
        ComWrappersSupport.InitializeComWrappers();

        var bootstrapped = false;

        try
        {
            if (!IsRunningPackaged())
            {
                Bootstrap.Initialize(WindowsAppSdkMajorMinorVersion);
                bootstrapped = true;
            }

            Application.Start(_ =>
            {
                var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);
                var app = new App();
            });
        }
        finally
        {
            if (bootstrapped)
            {
                Bootstrap.Shutdown();
            }
        }
    }

    private static bool IsRunningPackaged()
    {
        var length = 0;
        var result = GetCurrentPackageFullName(ref length, null);
        return result != AppModelErrorNoPackage;
    }

    private const int AppModelErrorNoPackage = 15700;

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int GetCurrentPackageFullName(ref int packageFullNameLength, string? packageFullName);
}
