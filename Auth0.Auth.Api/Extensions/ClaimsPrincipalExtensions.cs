using System.Security.Claims;

namespace Auth0.Auth.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// 检查用户是否拥有指定的 scope（Auth0 scope claim，空格分隔）。
        /// </summary>
        public static bool HasScope(this ClaimsPrincipal user, string scope)
        {
            return user.Claims.Any(c =>
                c.Type == "scope" &&
                c.Value.Split(' ').Contains(scope));
        }

        /// <summary>
        /// 检查用户是否拥有指定的 permission（Auth0 permissions claim，空格分隔）。
        /// </summary>
        public static bool HasPermission(this ClaimsPrincipal user, string permission)
        {
            return user.Claims.Any(c =>
                c.Type == "permissions" &&
                c.Value.Split(' ').Contains(permission));
        }
    }
}
