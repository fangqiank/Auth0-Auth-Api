param(
    [string]$EnvFile = "../../.env"
)

$ProjectPath = Join-Path $PSScriptRoot "..\Auth0.Auth.Api.csproj"

Write-Host "🔐 正在配置 User Secrets for Auth0.Auth.Api..." -ForegroundColor Cyan

# 读取 .env 文件
if (Test-Path $EnvFile) {
    Get-Content $EnvFile | ForEach-Object {
        if ($_ -match '^([^=]+)=(.*)$' -and $matches[1] -notlike '#*') {
            [Environment]::SetEnvironmentVariable($matches[1], $matches[2], 'Process')
        }
    }
}

# 验证必要的环境变量
$requiredVars = @('AUTH0_DOMAIN', 'AUTH0_AUDIENCE', 'AUTH0_CLIENT_ID', 'AUTH0_CLIENT_SECRET')
foreach ($var in $requiredVars) {
    if ([string]::IsNullOrEmpty([Environment]::GetEnvironmentVariable($var, 'Process'))) {
        Write-Error "❌ 缺少必要的环境变量: $var"
        exit 1
    }
}

# 设置 User Secrets
try {
    dotnet user-secrets set "Authentication:Domain" $env:AUTH0_DOMAIN --project $ProjectPath
    dotnet user-secrets set "Authentication:Audience" $env:AUTH0_AUDIENCE --project $ProjectPath
    dotnet user-secrets set "Authentication:ClientId" $env:AUTH0_CLIENT_ID --project $ProjectPath
    dotnet user-secrets set "Authentication:ClientSecret" $env:AUTH0_CLIENT_SECRET --project $ProjectPath
    dotnet user-secrets set "Authentication:AuthorizationUrl" "https://$env:AUTH0_DOMAIN/authorize" --project $ProjectPath
    dotnet user-secrets set "Authentication:TokenUrl" "https://$env:AUTH0_DOMAIN/oauth/token" --project $ProjectPath
    dotnet user-secrets set "Authentication:MetadataAddress" "https://$env:AUTH0_DOMAIN/.well-known/openid-configuration" --project $ProjectPath
    dotnet user-secrets set "Authentication:ValidIssuer" "https://$env:AUTH0_DOMAIN/" --project $ProjectPath

    Write-Host ""
    Write-Host "✅ User Secrets 配置完成！" -ForegroundColor Green
    Write-Host ""
    Write-Host "配置摘要:" -ForegroundColor Yellow
    Write-Host "  Domain: $env:AUTH0_DOMAIN"
    Write-Host "  Audience: $env:AUTH0_AUDIENCE"
    Write-Host "  Client ID: $env:AUTH0_CLIENT_ID"
    Write-Host ""
    Write-Host "现在可以运行: dotnet run --project $ProjectPath" -ForegroundColor Cyan
}
catch {
    Write-Error "❌ 配置失败: $_"
    exit 1
}