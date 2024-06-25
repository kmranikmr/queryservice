FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 6004

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
COPY *.sln ./
COPY DataAnalyticsPlatform.QueryService/QueryServiceApi.csproj DataAnalyticsPlatform.QueryService/QueryServiceApi.csproj
COPY DataAccessLayer/DataAccessLayer.csproj DataAccessLayer/DataAccessLayer.csproj
COPY QueryAntlr/QueryAntlr.csproj QueryAntlr/QueryAntlr.csproj
COPY QueryEngine/QueryEngine.csproj QueryEngine/QueryEngine.csproj

RUN dotnet restore
COPY . .
#WORKDIR /DataAccessLayer
#RUN dotnet publish -c Release -o /app

#WORKDIR /QueryAntlr
#RUN dotnet publish -c Release -o /app

#WORKDIR /QueryEngine
#RUN dotnet publish -c Release -o /app

WORKDIR /DataAnalyticsPlatform.QueryService
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "QueryServiceApi.dll"]
