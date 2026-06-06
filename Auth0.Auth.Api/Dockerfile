# 构建阶段
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 复制项目文件并还原依赖
COPY ["Auth0.Auth.Api/Auth0.Auth.Api.csproj", "Auth0.Auth.Api/"]
RUN dotnet restore "Auth0.Auth.Api/Auth0.Auth.Api.csproj"

# 复制所有源代码
COPY . .

# 构建并发布
WORKDIR "/src/Auth0.Auth.Api"
RUN dotnet build "Auth0.Auth.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build
RUN dotnet publish "Auth0.Auth.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 运行阶段
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# 创建非 root 用户
RUN useradd -m -s /bin/false appuser && chown -R appuser /app
USER appuser

# 暴露端口
EXPOSE 8080

# 健康检查
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget -qO- http://localhost:8080/api/health || exit 1

# 环境变量
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# 复制发布文件
COPY --from=build /app/publish .

# 启动应用
ENTRYPOINT ["dotnet", "Auth0.Auth.Api.dll"]