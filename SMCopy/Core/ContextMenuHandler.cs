using System;
using System.IO;
using System.Reflection;
using Microsoft.Win32;

namespace SMCopy.Core
{
    public static class ContextMenuHandler
    {
        private static string GetExecutablePath()
        {
            return Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe");
        }

        public static void Register()
        {
            string exePath = GetExecutablePath();

            // Register for files
            RegisterForFiles(exePath);

            // Register for folders
            RegisterForFolders(exePath);

            // Register for folder background (for paste)
            RegisterForFolderBackground(exePath);
        }

        private static void RegisterForFiles(string exePath)
        {
            // HKEY_CLASSES_ROOT\*\shell\SMCopy
            using var key = Registry.ClassesRoot.CreateSubKey(@"*\shell\SMCopy");
            key.SetValue("", "SM Copy");
            key.SetValue("Icon", exePath);

            using var commandKey = key.CreateSubKey("command");
            commandKey.SetValue("", $"\"{exePath}\" --copy \"%1\"");
        }

        private static void RegisterForFolders(string exePath)
        {
            // HKEY_CLASSES_ROOT\Directory\shell\SMCopy
            using var key = Registry.ClassesRoot.CreateSubKey(@"Directory\shell\SMCopy");
            key.SetValue("", "SM Copy");
            key.SetValue("Icon", exePath);

            using var commandKey = key.CreateSubKey("command");
            commandKey.SetValue("", $"\"{exePath}\" --copy \"%1\"");
        }

        private static void RegisterForFolderBackground(string exePath)
        {
            // HKEY_CLASSES_ROOT\Directory\Background\shell\SMPaste
            using var key = Registry.ClassesRoot.CreateSubKey(@"Directory\Background\shell\SMPaste");
            key.SetValue("", "SM Paste");
            key.SetValue("Icon", exePath);

            using var commandKey = key.CreateSubKey("command");
            commandKey.SetValue("", $"\"{exePath}\" --paste \"%V\"");
        }

        public static void Unregister()
        {
            // Remove file context menu
            try
            {
                Registry.ClassesRoot.DeleteSubKeyTree(@"*\shell\SMCopy", false);
            }
            catch { }

            // Remove folder context menu
            try
            {
                Registry.ClassesRoot.DeleteSubKeyTree(@"Directory\shell\SMCopy", false);
            }
            catch { }

            // Remove folder background context menu
            try
            {
                Registry.ClassesRoot.DeleteSubKeyTree(@"Directory\Background\shell\SMPaste", false);
            }
            catch { }
        }

        public static bool IsRegistered()
        {
            try
            {
                using var key = Registry.ClassesRoot.OpenSubKey(@"*\shell\SMCopy");
                return key != null;
            }
            catch
            {
                return false;
            }
        }
    }
}

