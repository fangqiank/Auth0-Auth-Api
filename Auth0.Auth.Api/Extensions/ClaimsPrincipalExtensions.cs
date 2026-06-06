using System.Security.Claims;

namespace Auth0.Auth.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// 检查用户是否拥有指定的 scope。
        /// 兼容 Auth0 的 scope 格式：单字符串 "read:data write:data" 或多个独立 claim。
        /// </summary>
        public static bool HasScope(this ClaimsPrincipal user, string scope)
        {
            return user.Claims.Any(c =>
                c.Type == "scope" &&
                c.Value.Split(' ').Contains(scope));
        }
    }
}
