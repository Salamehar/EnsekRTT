FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["MeterReadings.API/*.csproj", "MeterReadings.API/"]
COPY ["MeterReadings.Core/*.csproj", "MeterReadings.Core/"]
COPY ["MeterReadings.Infrastructure/*.csproj", "MeterReadings.Infrastructure/"]
COPY ["MeterReadings.Data/*.csproj", "MeterReadings.Data/"]
RUN dotnet restore "MeterReadings.API/MeterReadings.API.csproj"

# Copy the rest of the code
COPY . .
WORKDIR "/src/MeterReadings.API"

# Build and publish
RUN dotnet build "MeterReadings.API.csproj" -c Release -o /app/build
RUN dotnet publish "MeterReadings.API.csproj" -c Release -o /app/publish

# Create runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Copy the CSV files for seeding
COPY ["MeterReadings.API/Data/*.csv", "/app/Data/"]

EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "MeterReadings.API.dll"]