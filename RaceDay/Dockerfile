# Multi-stage Dockerfile. Docker Compose builds the API and MVC as separate images.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG APP_PROJECT=RaceDay.Api
WORKDIR /src
COPY . .
RUN dotnet restore "${APP_PROJECT}/${APP_PROJECT}.csproj"
RUN dotnet publish "${APP_PROJECT}/${APP_PROJECT}.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
ARG APP_PROJECT=RaceDay.Api
ENV APP_DLL=${APP_PROJECT}
ENV ASPNETCORE_URLS=http://+:8080
WORKDIR /app
COPY --from=build /app/publish .
# Prepare the local image fallback directory and run as the non-root .NET app user.
RUN mkdir -p /app/wwwroot/uploads && chown -R $APP_UID:$APP_UID /app
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "dotnet ${APP_DLL}.dll"]
