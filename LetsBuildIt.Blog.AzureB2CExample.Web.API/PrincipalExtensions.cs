using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace LetsBuildIt.Web.API
{
    public static class PrincipalExtensions
    {
        public static IEnumerable<string> Roles(this IPrincipal user)
        {
            return ClaimsOfType(user, ClaimTypes.Role);
        }

        public static IEnumerable<string> ClaimsOfType(this IPrincipal user, string claimType)
        {
            if (!(user.Identity is ClaimsIdentity)) return new string[0];
            return ((ClaimsIdentity)user.Identity).Claims
                                                   .Where(c => c.Type.Equals(claimType))
                                                   .Select(c => c.Value);
        }

        public static string ClaimOfType(this IPrincipal user, string claimType)
        {
            return ClaimsOfType(user, claimType).FirstOrDefault();
        }

        public static string Name(this IPrincipal user)
        {
            return user.Identity.Name;
        }

        public static string DisplayName(this IPrincipal user)
        {
            string surname = ClaimOfType(user, ClaimTypes.Surname);
            string givenName = ClaimOfType(user, ClaimTypes.GivenName);
            if (string.IsNullOrWhiteSpace(surname) && string.IsNullOrWhiteSpace(givenName)) return Name(user);//.TrimDomain();
            if (string.IsNullOrWhiteSpace(surname) || string.IsNullOrWhiteSpace(givenName)) return string.Format("{0}{1}", givenName ?? string.Empty, surname ?? string.Empty);
            return string.Format("{0} {1}", givenName, surname);
        }
    }
}
