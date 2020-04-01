using System.Linq;
using System.Reflection;
using GeneralExtensions;

namespace Bibles.Downloads
{
    internal static class LogonCredentials
    {
        internal static string UserName()
        {
            return typeof(Properties.Resources)
              .GetProperties(BindingFlags.Static | BindingFlags.NonPublic |
                             BindingFlags.Public)
              .Where(p => p.PropertyType == typeof(string) && p.Name == "UserName")
              .Select(x => x.GetValue(null, null))
              .FirstOrDefault()
              .ParseToString();
        }

        internal static string Password()
        {
            return typeof(Properties.Resources)
              .GetProperties(BindingFlags.Static | BindingFlags.NonPublic |
                             BindingFlags.Public)
              .Where(p => p.PropertyType == typeof(string) && p.Name == "Password")
              .Select(x => x.GetValue(null, null))
              .FirstOrDefault()
              .ParseToString();
        }
    }
}
