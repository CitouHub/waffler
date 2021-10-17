#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/azure-functions/dotnet:3.0 AS base
WORKDIR /home/site/wwwroot
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["Waffler.Function/Waffler.Function.csproj", "Waffler.Function/"]
COPY ["Waffler.Data/Waffler.Data.csproj", "Waffler.Data/"]
COPY ["Waffler.Domain/Waffler.Domain.csproj", "Waffler.Domain/"]
COPY ["Waffler.Service/Waffler.Service.csproj", "Waffler.Service/"]
RUN dotnet restore "Waffler.Function/Waffler.Function.csproj"
COPY . .
WORKDIR "/src/Waffler.Function"
RUN dotnet build "Waffler.Function.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Waffler.Function.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true