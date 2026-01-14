FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/VeniceOrders.API/VeniceOrders.API.csproj", "src/VeniceOrders.API/"]
COPY ["src/VeniceOrders.Application/VeniceOrders.Application.csproj", "src/VeniceOrders.Application/"]
COPY ["src/VeniceOrders.Domain/VeniceOrders.Domain.csproj", "src/VeniceOrders.Domain/"]
COPY ["src/VeniceOrders.Infrastructure/VeniceOrders.Infrastructure.csproj", "src/VeniceOrders.Infrastructure/"]
RUN dotnet restore "src/VeniceOrders.API/VeniceOrders.API.csproj"
COPY . .
WORKDIR "/src/src/VeniceOrders.API"
RUN dotnet build "VeniceOrders.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VeniceOrders.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VeniceOrders.API.dll"]
