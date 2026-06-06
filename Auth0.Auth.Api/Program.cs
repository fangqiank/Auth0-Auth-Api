using Auth0.Auth.Api.Endpoints;
using Auth0.Auth.Api.Extensions;
using Auth0.Auth.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

var auth0Domain = builder.Configuration["Authentication:Domain"];
if (string.IsNullOrEmpty(auth0Domain))
    throw new InvalidOperationException(
        "Auth0 Domain is not configured. Please set the 'Authentication:Domain' configuration value.");

// 验证必需配置
var requiredConfigs = new Dictionary<string, string>
{
    ["Authentication:Audience"] = "Auth0 API Audience",
    ["Authentication:ClientId"] = "Auth0 Client ID",
    ["Authentication:ClientSecret"] = "Auth0 Client Secret",
    ["Authentication:MetadataAddress"] = "Auth0 OIDC Metadata Address",
    ["Authentication:ValidIssuer"] = "Auth0 Valid Issuer"
};
foreach (var (key, label) in requiredConfigs)
{
    if (string.IsNullOrEmpty(builder.Configuration[key]))
        throw new InvalidOperationException($"{label} is not configured. Please set the '{key}' configuration value.");
}

// 原生 OpenAPI + Scalar
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info = new()
        {
            Title = "Auth0 Auth API",
            Version = "v1",
            Description = """
            .NET 10 Minimal API with Auth0 JWT Authentication

            ## 认证方式
            使用 Bearer JWT — 在 Authorize 面板粘贴通过 client_credentials 获取的 Access Token。
            """
        };

        var authorizationUrl = builder.Configuration["Authentication:AuthorizationUrl"];
        var tokenUrl = builder.Configuration["Authentication:TokenUrl"];

        document.Components ??= new();

        // Bearer JWT 认证方案
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "输入 JWT Bearer token（通过 client_credentials 获取）"
        };

        // Auth0 OAuth2 认证方案（仅配置完整时启用）
        if (!string.IsNullOrEmpty(authorizationUrl) && !string.IsNullOrEmpty(tokenUrl))
        {
            document.Components.SecuritySchemes["Auth0"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(authorizationUrl),
                        TokenUrl = new Uri(tokenUrl),
                        Scopes = new Dictionary<string, string>
                        {
                            { "openid", "OpenID Connect" },
                            { "profile", "用户基本信息" },
                            { "email", "用户邮箱" },
                            { "read:data", "读取数据" },
                            { "write:data", "写入数据" }
                        }
                    }
                }
            };
        }

        return Task.CompletedTask;
    });
});

// JWT Bearer 认证
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Audience = builder.Configuration["Authentication:Audience"];
        options.MetadataAddress = builder.Configuration["Authentication:MetadataAddress"]!;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Authentication:ValidIssuer"],
            ValidateAudience = true,
            ValidAudience = options.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5),
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = "permissions"
        };
        options.MapInboundClaims = false;
    });

// 授权
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("read:data", policy =>
        policy.RequireAuthenticatedUser()
              .RequireAssertion(ctx => ctx.User.HasScope("read:data")))
    .AddPolicy("write:data", policy =>
        policy.RequireAuthenticatedUser()
              .RequireAssertion(ctx => ctx.User.HasScope("write:data")))
    .AddPolicy("admin", policy =>
        policy.RequireAuthenticatedUser()
              .RequireAssertion(ctx => ctx.User.HasPermission("admin")));

// OpenTelemetry 可观测性
builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("Auth0.Auth.Api"))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter();
    });

var app = builder.Build();

app.UseExceptionHandling();

// 开发环境：OpenAPI 文档 + Scalar UI
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthEndpoints();
app.MapUserEndpoints();
app.MapDataEndpoints();

app.MapGet("/", () => Results.Redirect("/scalar/v1"))
    .ExcludeFromDescription();

app.Run();
