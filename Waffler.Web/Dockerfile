FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
ARG build_id=1
ARG build_tag=(none)
ARG commit_id=#

RUN echo "$build_id" >> var_build_id
RUN echo "$build_tag" >> var_build_tag
RUN echo "$commit_id" >> var_commit_id

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY --from=base var_build_id .
COPY --from=base var_build_tag .
COPY --from=base var_commit_id .

ENV BUILD_ID=1
ENV BUILD_TAG=(none)
ENV COMMIT_ID=#

RUN BUILD_ID=$(cat var_build_id); \
    BUILD_TAG=$(cat var_build_tag); \
    COMMIT_ID=$(cat var_commit_id); \
    echo "Building with build id '$BUILD_ID', tag '$BUILD_TAG' and commit '$COMMIT_ID'"

RUN echo "Building with build id '$BUILD_ID', tag '$BUILD_TAG' and commit '$COMMIT_ID'"

RUN curl -sL https://deb.nodesource.com/setup_10.x |  bash -
RUN apt-get update && apt-get install -y nodejs

RUN echo "Building with build id '$env_build_id', tag '$env_build_tag' and commit '$env_commit_id'"

COPY ["Waffler.Web/Waffler.Web.csproj", "Waffler.Web/"]
RUN dotnet restore "Waffler.Web/Waffler.Web.csproj"
COPY . .
WORKDIR "/src/Waffler.Web"

RUN echo "Building with build id '$env_build_id', tag '$env_build_tag' and commit '$env_commit_id'"

FROM build AS publish
RUN dotnet publish "Waffler.Web.csproj" -c Release -o /app/publish -p:BuildId=${env_build_id} -p:BuildTag=${env_build_tag} -p:CommitId=${env_commit_id}

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Waffler.Web.dll"]
