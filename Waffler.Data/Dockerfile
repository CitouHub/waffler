# Inspired by sample in https://github.com/twright-msft/mssql-node-docker-demo-app
FROM mcr.microsoft.com/azure-sql-edge:latest
EXPOSE 1433

ARG RS_DATABASE_NAME=Waffler
ENV RS_DATABASE_NAME $RS_DATABASE_NAME

# Switch to root user for access to apt-get install
USER root

RUN apt-get update && apt-get install -y dos2unix

# Create app directory
RUN mkdir -p /usr/src/db
WORKDIR /usr/src/db

# Bundle app source
COPY ./Waffler.Data/Db /usr/src/db
COPY ./Waffler.Data/Master /usr/src/db

RUN dos2unix *
RUN apt-get --purge remove -y dos2unix

RUN chmod +x /usr/src/db/setup.sh
RUN chmod +x /usr/src/db/entypoint.sh

# Switch back to mssql user and run the entrypoint script
USER mssql
ENTRYPOINT /bin/bash ./entypoint.sh