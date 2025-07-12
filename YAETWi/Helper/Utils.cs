using System.Security.Principal;

namespace YAETWi.Helper
{
    public class Utils
    {
        public static bool isPrivileged()
        {
            using (WindowsIdentity identiy = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identiy);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
        }
    }
}
