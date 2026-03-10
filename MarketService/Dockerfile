FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5002

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["MarketService/MarketService.csproj", "MarketService/"]
RUN dotnet restore "MarketService/MarketService.csproj"
COPY MarketService/ MarketService/
WORKDIR "/src/MarketService"
RUN dotnet build "MarketService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MarketService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MarketService.dll"]
