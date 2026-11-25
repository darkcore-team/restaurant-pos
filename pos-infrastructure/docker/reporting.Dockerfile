FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY pos-shared/ pos-shared/
COPY reporting-service/ reporting-service/
RUN dotnet publish "reporting-service/src/ReportingService.Api/ReportingService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:WarningsNotAsErrors="NU1901;NU1902;NU1903;NU1904"

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=8086
USER $APP_UID
EXPOSE 8086
ENTRYPOINT ["sh", "-c", "exec dotnet ReportingService.Api.dll --urls http://0.0.0.0:${PORT:-8086}"]
