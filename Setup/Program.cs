using System;
using System.Windows.Forms;

namespace SMCopySetup
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Set DPI mode before any UI is created
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SetupForm());
        }
    }
}
