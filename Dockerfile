# Stage 1: Build & Restore
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy project files for caching
COPY ["src/NeoWallet.Domain/NeoWallet.Domain.csproj", "src/NeoWallet.Domain/"]
COPY ["src/NeoWallet.Application/NeoWallet.Application.csproj", "src/NeoWallet.Application/"]
COPY ["src/NeoWallet.Infrastructure/NeoWallet.Infrastructure.csproj", "src/NeoWallet.Infrastructure/"]
COPY ["src/NeoWallet.Api/NeoWallet.Api.csproj", "src/NeoWallet.Api/"]

RUN dotnet restore "src/NeoWallet.Api/NeoWallet.Api.csproj"

# Copy full source and build
COPY . .
WORKDIR "/src/src/NeoWallet.Api"
RUN dotnet build "NeoWallet.Api.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "NeoWallet.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Security: run as non-root user
USER $APP_UID

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NeoWallet.Api.dll"]
