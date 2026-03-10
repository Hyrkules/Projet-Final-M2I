FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["CryptoSim/CryptoSim.Blazor.csproj", "CryptoSim/"]
RUN dotnet restore "CryptoSim/CryptoSim.Blazor.csproj"
COPY CryptoSim/ CryptoSim/
WORKDIR "/src/CryptoSim"
RUN dotnet build "CryptoSim.Blazor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CryptoSim.Blazor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CryptoSim.Blazor.dll"]
