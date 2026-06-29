# ----- Build aşaması (context = repo kökü) -----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props ./
COPY src/ ./src/

RUN dotnet restore src/Ols.ControlCenter.Worker/Ols.ControlCenter.Worker.csproj
RUN dotnet publish src/Ols.ControlCenter.Worker/Ols.ControlCenter.Worker.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# ----- Runtime aşaması -----
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV DOTNET_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Ols.ControlCenter.Worker.dll"]
