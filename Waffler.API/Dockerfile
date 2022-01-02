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
COPY ["Waffler.API/Waffler.API.csproj", "Waffler.API/"]
COPY ["Waffler.Common/Waffler.Common.csproj", "Waffler.Common/"]
COPY ["Waffler.Service/Waffler.Service.csproj", "Waffler.Service/"]
COPY ["Waffler.Data/Waffler.Data.csproj", "Waffler.Data/"]
COPY ["Waffler.Domain/Waffler.Domain.csproj", "Waffler.Domain/"]

RUN mkdir -p /Master
RUN mkdir -p /Migration
COPY ./Waffler.Data/Master /Master
COPY ./Waffler.Data/Migration /Migration

RUN dotnet restore "Waffler.API/Waffler.API.csproj"
COPY . .
WORKDIR "/src/Waffler.API"

FROM build AS publish

COPY --from=base var_build_id .
COPY --from=base var_build_tag .
COPY --from=base var_commit_id .

RUN BUILD_ID=$(cat var_build_id); \
    BUILD_TAG=$(cat var_build_tag); \
    COMMIT_ID=$(cat var_commit_id); \
    echo "Publish with build id '$BUILD_ID', tag '$BUILD_TAG' and commit '$COMMIT_ID'"; \
    dotnet publish "Waffler.API.csproj" -c Release -o /app/publish -p:BuildId=${BUILD_ID} -p:BuildTag=${BUILD_TAG} -p:CommitId=${COMMIT_ID}

FROM base AS final
LABEL org.opencontainers.image.source="https://github.com/CitouHub/waffler"
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Waffler.API.dll"]
