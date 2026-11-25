FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY pos-shared/ pos-shared/
COPY orders-service/ orders-service/
RUN dotnet publish "orders-service/src/OrdersService.Api/OrdersService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:WarningsNotAsErrors="NU1901;NU1902;NU1903;NU1904"

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=8084
USER $APP_UID
EXPOSE 8084
ENTRYPOINT ["sh", "-c", "exec dotnet OrdersService.Api.dll --urls http://0.0.0.0:${PORT:-8084}"]
