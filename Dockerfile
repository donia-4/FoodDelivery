# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Note: the inner folder is named "Restaurant" not "Restaurant.API"
COPY Restaurant/*.csproj Restaurant/
COPY Restaurant.Application/*.csproj Restaurant.Application/
COPY Restaurant.Domain/*.csproj Restaurant.Domain/
COPY Restaurant.Infrastructure/*.csproj Restaurant.Infrastructure/

RUN dotnet restore Restaurant/Restaurant.API.csproj

COPY . .

WORKDIR /source/Restaurant
RUN dotnet publish -c Release -o /app --no-restore

# Final Stage: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Restaurant.API.dll"]