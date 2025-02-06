# STAGE 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

ARG NUGET_TELERIK_USER
ARG NUGET_TELERIK_PASSWORD
ARG NUGET_ATLASX_USER
ARG NUGET_ATLASX_PASSWORD

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj .
COPY ./nuget.config.template ./nuget.config
RUN dotnet restore

# Copy and publish app and libraries
COPY . .
RUN dotnet publish -c Release -o out --no-restore

# STAGE 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim
ENV TZ='Asia/Bangkok'

WORKDIR /app
COPY --from=build /app/out .

EXPOSE 8080

ENTRYPOINT ["dotnet", "AtlasX.Web.Service.dll", "--urls", "http://0.0.0.0:8080"]
