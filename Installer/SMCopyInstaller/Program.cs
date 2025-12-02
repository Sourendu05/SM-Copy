using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;

namespace SMCopyInstaller
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Check if running as administrator
            if (!IsAdministrator())
            {
                // Restart as administrator
                var exeName = Process.GetCurrentProcess().MainModule?.FileName;
                if (exeName != null)
                {
                    try
                    {
                        var processInfo = new ProcessStartInfo
                        {
                            FileName = exeName,
                            UseShellExecute = true,
                            Verb = "runas" // Request admin elevation
                        };
                        Process.Start(processInfo);
                    }
                    catch
                    {
                        MessageBox.Show(
                            "Administrator privileges are required to install SM Copy.\n\nPlease run the installer as administrator.",
                            "Administrator Required",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                    }
                }
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new InstallerForm());
        }

        private static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
