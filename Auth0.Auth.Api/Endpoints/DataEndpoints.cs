using Auth0.Auth.Api.Extensions;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace Auth0.Auth.Api.Endpoints
{
    public static class DataEndpoints
    {
        private static readonly ConcurrentDictionary<int, DataItem> DataStore = new(Enumerable.Range(1, 3).ToDictionary(
            i => i, i => new DataItem(i, $"Item {i}", $"Value {i}")));

        private static int _nextId = 4;

        public static void MapDataEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/data")
                .RequireAuthorization()
                .WithTags("Data");

            // 需要 read:data scope
            group.MapGet("/", () =>
            {
                return Results.Ok(new
                {
                    Message = "数据读取成功",
                    Data = DataStore.Values.OrderBy(d => d.Id),
                    Count = DataStore.Count,
                    Timestamp = DateTimeOffset.UtcNow
                });
            })
            .RequireAuthorization(policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(ctx =>
                    ctx.User.HasScope("read:data"));
            })
            .WithName("ReadAllData")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "读取所有数据";
                operation.Description = "需要 read:data scope 权限";
                return Task.CompletedTask;
            });

            // 需要 write:data scope
            group.MapPost("/", (DataItem payload, ClaimsPrincipal user) =>
            {
                var id = Interlocked.Increment(ref _nextId) - 1;
                var item = payload with { Id = id };
                DataStore[id] = item;

                return Results.Created($"/api/data/{id}", new
                {
                    Message = "数据写入成功",
                    CreatedBy = user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    NewItem = item,
                    TotalCount = DataStore.Count,
                    Timestamp = DateTimeOffset.UtcNow
                });
            })
            .RequireAuthorization(policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(ctx =>
                    ctx.User.HasScope("write:data"));
            })
            .WithName("WriteData")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "写入新数据";
                operation.Description = "需要 write:data scope 权限";
                return Task.CompletedTask;
            });

            // 需要 admin permission
            group.MapDelete("/purge", (ClaimsPrincipal user) =>
            {
                var count = DataStore.Count;
                DataStore.Clear();

                return Results.Ok(new
                {
                    Message = "所有数据已清除",
                    PurgedBy = user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    PurgedCount = count,
                    Timestamp = DateTimeOffset.UtcNow
                });
            })
            .RequireAuthorization(policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(ctx =>
                    ctx.User.HasPermission("admin"));
            })
            .WithName("PurgeData")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "清除所有数据";
                operation.Description = "需要 admin 权限，危险操作";
                return Task.CompletedTask;
            });
        }
    }

    public record DataItem(int Id, string Name, string Value);
}
