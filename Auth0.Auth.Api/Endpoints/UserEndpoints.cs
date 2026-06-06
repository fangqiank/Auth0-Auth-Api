using System.Security.Claims;

namespace Auth0.Auth.Api.Endpoints
{
    public static class UserEndpoints
    {
        // 敏感 claims 不应返回给客户端
        private static readonly HashSet<string> SensitiveClaimTypes =
        [
            "at_hash", "nonce", "sid", "aud", "iss", "iat", "exp", "azp", "sub"
        ];

        public static void MapUserEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/users")
                .RequireAuthorization()
                .WithTags("Users");

            group.MapGet("/me", (ClaimsPrincipal user, ILoggerFactory loggerFactory) =>
            {
                var logger = loggerFactory.CreateLogger("UserEndpoints");
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                logger.LogInformation("User {UserId} accessed /me endpoint", userId);

                var scopes = user.FindFirst("scope")?.Value?.Split(' ');
                var permissions = user.FindFirst("permissions")?.Value?.Split(' ');

                return Results.Ok(new
                {
                    UserId = userId,
                    Email = user.FindFirst("email")?.Value,
                    Name = user.FindFirst("name")?.Value,
                    Nickname = user.FindFirst("nickname")?.Value,
                    Picture = user.FindFirst("picture")?.Value,
                    EmailVerified = user.FindFirst("email_verified")?.Value,
                    Permissions = permissions,
                    Scopes = scopes,
                    Timestamp = DateTimeOffset.UtcNow
                });
            })
            .WithName("GetCurrentUser")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "获取当前用户信息";
                operation.Description = "返回当前认证用户的 Claims 信息，包括用户 ID、邮箱、权限等";
                return Task.CompletedTask;
            });

            group.MapGet("/claims", (ClaimsPrincipal user) =>
            {
                return Results.Ok(user.Claims
                    .Where(c => !SensitiveClaimTypes.Contains(c.Type))
                    .GroupBy(c => c.Type)
                    .Select(g => new
                    {
                        Type = g.Key,
                        Values = g.Select(c => c.Value).ToList()
                    }));
            })
            .WithName("GetUserClaims")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "获取用户 Claims";
                operation.Description = "按 Claim 类型分组返回所有用户 Claims（过滤敏感信息）";
                return Task.CompletedTask;
            });
        }
    }
}
