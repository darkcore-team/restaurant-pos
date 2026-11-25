FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY pos-shared/ pos-shared/
COPY payments-service/ payments-service/
RUN dotnet publish "payments-service/src/PaymentsService.Api/PaymentsService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=8085
USER $APP_UID
EXPOSE 8085
ENTRYPOINT ["sh", "-c", "exec dotnet PaymentsService.Api.dll --urls http://0.0.0.0:${PORT:-8085}"]
