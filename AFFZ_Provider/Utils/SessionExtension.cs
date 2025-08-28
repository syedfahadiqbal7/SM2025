using Microsoft.AspNetCore.DataProtection;
using System.Text;

namespace AFFZ_Provider.Utils
{
    public static class SessionExtension
    {

        public static void SetEncryptedString(this ISession session, string key, string value, IDataProtector protector)
        {
            var protectedValue = protector.Protect(Encoding.UTF8.GetBytes(value));
            session.Set(key, protectedValue);
        }

        public static string GetEncryptedString(this ISession session, string key, IDataProtector protector)
        {
            session.TryGetValue(key, out var protectedValue);
            if (protectedValue == null)
                return null;

            var unprotectedValue = protector.Unprotect(protectedValue);
            return Encoding.UTF8.GetString(unprotectedValue);
        }

    }
}
