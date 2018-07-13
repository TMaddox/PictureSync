using System.Configuration.Install;
using System.Reflection;

namespace PictureSync
{
    public static class SelfInstaller
    {
        private static readonly string ExePath = Assembly.GetExecutingAssembly().Location;
        public static bool InstallMe()
        {
            try
            {
                ManagedInstallerClass.InstallHelper(
                    new string[] { ExePath });
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static bool UninstallMe()
        {
            try
            {
                ManagedInstallerClass.InstallHelper(
                    new string[] { "/u", ExePath });
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}