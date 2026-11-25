FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY pos-shared/ pos-shared/
COPY catalog-service/ catalog-service/
RUN dotnet publish "catalog-service/src/CatalogService.Api/CatalogService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=8082
USER $APP_UID
EXPOSE 8082
ENTRYPOINT ["sh", "-c", "exec dotnet CatalogService.Api.dll --urls http://0.0.0.0:${PORT:-8082}"]
