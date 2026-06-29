# ----- Build aşaması (context = repo kökü) -----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Central Package Management dosyaları (restore için gerekli)
COPY Directory.Build.props Directory.Packages.props ./
COPY src/ ./src/

RUN dotnet restore src/Ols.ControlCenter.Api/Ols.ControlCenter.Api.csproj
RUN dotnet publish src/Ols.ControlCenter.Api/Ols.ControlCenter.Api.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# ----- Runtime aşaması -----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "Ols.ControlCenter.Api.dll"]
