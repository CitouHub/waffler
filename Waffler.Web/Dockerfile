FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

ARG build_id=1
ARG build_tag=(none)
ARG commit_id=#

ENV build_id=$build_id
ENV build_tag=$build_tag
ENV commit_id=$commit_id

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

RUN curl -sL https://deb.nodesource.com/setup_10.x |  bash -
RUN apt-get update && apt-get install -y nodejs

COPY ["Waffler.Web/Waffler.Web.csproj", "Waffler.Web/"]
RUN dotnet restore "Waffler.Web/Waffler.Web.csproj"
COPY . .
WORKDIR "/src/Waffler.Web"

RUN echo "Building with build id '$build_id', tag '$build_tag' and commit '$commit_id'"

FROM build AS publish
RUN dotnet publish "Waffler.Web.csproj" -c Release -o /app/publish -p:BuildId=${build_id} -p:BuildTag=${build_tag} -p:CommitId=${commit_id}

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Waffler.Web.dll"]
