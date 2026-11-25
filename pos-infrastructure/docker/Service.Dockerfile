# Multi-stage Dockerfile for all 7 RestaurantPOS .NET 10 microservices.
# Build context MUST be the monorepo root (~/Desktop/restaurant-pos).
# Example:
#   docker build -f pos-infrastructure/docker/Service.Dockerfile \
#     --build-arg SERVICE_DIR=orders-service \
#     --build-arg PROJECT_NAME=OrdersService.Api \
#     -t pos-orders .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG SERVICE_DIR
ARG PROJECT_NAME

WORKDIR /src

# Copy shared building blocks and target service
COPY pos-shared/ pos-shared/
COPY ${SERVICE_DIR}/ ${SERVICE_DIR}/

# Restore and publish Release build without SDK artifacts
RUN dotnet publish "${SERVICE_DIR}/src/${PROJECT_NAME}/${PROJECT_NAME}.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
ARG PROJECT_NAME

WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV APP_DLL="${PROJECT_NAME}.dll"
ENV PORT=8080

USER $APP_UID
EXPOSE 8080

# Bind to $PORT dynamically (Render injects PORT=10000; docker-compose sets 8081..8087)
ENTRYPOINT ["sh", "-c", "exec dotnet ${APP_DLL} --urls http://0.0.0.0:${PORT:-8080}"]
