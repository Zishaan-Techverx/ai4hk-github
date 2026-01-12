using System.Security.Principal;
using System.Security.Claims;

public static class AuthorizationExtensions
{
    public static bool HasPermission(this IPrincipal user, string permissionName)
    {
        var principal = user as ClaimsPrincipal;

        if (principal == null)
            return false;

        return principal.HasClaim("Permission", permissionName);
    }
}