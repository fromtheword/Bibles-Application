using System.Linq;
using System.Reflection;
using GeneralExtensions;

namespace Bibles.Downloads
{
    internal static class LogonCredentials
    {
        internal static string Token()
        {
            return typeof(Properties.Resources)
              .GetProperties(BindingFlags.Static | BindingFlags.NonPublic |
                             BindingFlags.Public)
              .Where(p => p.PropertyType == typeof(string) && p.Name == "Token")
              .Select(x => x.GetValue(null, null))
              .FirstOrDefault()
              .ParseToString();
        }
    }
}
