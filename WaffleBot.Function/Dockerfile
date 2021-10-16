#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/azure-functions/dotnet:3.0 AS base
WORKDIR /home/site/wwwroot
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["WaffleBot.Function/WaffleBot.Function.csproj", "WaffleBot.Function/"]
COPY ["WaffleBot.Data/WaffleBot.Data.csproj", "WaffleBot.Data/"]
COPY ["WaffleBot.Domain/WaffleBot.Domain.csproj", "WaffleBot.Domain/"]
COPY ["WaffleBot.Service/WaffleBot.Service.csproj", "WaffleBot.Service/"]
RUN dotnet restore "WaffleBot.Function/WaffleBot.Function.csproj"
COPY . .
WORKDIR "/src/WaffleBot.Function"
RUN dotnet build "WaffleBot.Function.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WaffleBot.Function.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true