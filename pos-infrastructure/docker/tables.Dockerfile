FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY pos-shared/ pos-shared/
COPY tables-service/ tables-service/
RUN dotnet publish "tables-service/src/TablesService.Api/TablesService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:WarningsNotAsErrors="NU1901;NU1902;NU1903;NU1904"

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=8083
USER $APP_UID
EXPOSE 8083
ENTRYPOINT ["sh", "-c", "exec dotnet TablesService.Api.dll --urls http://0.0.0.0:${PORT:-8083}"]
