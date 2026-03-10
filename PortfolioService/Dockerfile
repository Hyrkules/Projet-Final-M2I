FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5003

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["PortfolioService/PortfolioService.csproj", "PortfolioService/"]
RUN dotnet restore "PortfolioService/PortfolioService.csproj"
COPY PortfolioService/ PortfolioService/
WORKDIR "/src/PortfolioService"
RUN dotnet build "PortfolioService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PortfolioService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PortfolioService.dll"]
