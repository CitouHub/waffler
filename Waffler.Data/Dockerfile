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

RUN apt-get --purge remove -y dos2unix