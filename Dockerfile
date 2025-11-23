# Stage 1: Base Runtime Image
# Uses the .NET 9 ASP.NET Core runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Stage 2: Build Image
# Uses the .NET 9 SDK
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["RenderLibraryBackend/RenderLibraryBackend.csproj", "RenderLibraryBackend/"]
RUN dotnet restore "./RenderLibraryBackend/RenderLibraryBackend.csproj"

# Copy the rest of the source code and build
COPY . .
WORKDIR "/src/RenderLibraryBackend"
RUN dotnet build "./RenderLibraryBackend.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RenderLibraryBackend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RenderLibraryBackend.dll"]