#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim-arm64v8 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim-arm64v8 AS build
WORKDIR /src

RUN curl -sL https://deb.nodesource.com/setup_10.x |  bash -
RUN apt-get update && apt-get install -y nodejs

COPY ["Waffler.Web/Waffler.Web.csproj", "Waffler.Web/"]
RUN dotnet restore "Waffler.Web/Waffler.Web.csproj"
COPY . .
WORKDIR "/src/Waffler.Web"
RUN dotnet build "Waffler.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Waffler.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Waffler.Web.dll"]