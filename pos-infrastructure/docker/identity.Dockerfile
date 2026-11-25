FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY pos-shared/ pos-shared/
COPY identity-service/ identity-service/
RUN dotnet publish "identity-service/src/IdentityService.Api/IdentityService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:NuGetAudit=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=8081
USER $APP_UID
EXPOSE 8081
ENTRYPOINT ["sh", "-c", "exec dotnet IdentityService.Api.dll --urls http://0.0.0.0:${PORT:-8081}"]
