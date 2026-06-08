FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
WORKDIR /source

COPY ./Directory.Build.props ./Directory.Packages.props .

WORKDIR /source/src
COPY src/. ./
RUN dotnet restore ./BookStorage.Api/BookStorage.Api.csproj -a $TARGETARCH
RUN dotnet publish ./BookStorage.Api/BookStorage.Api.csproj -a $TARGETARCH -c Release -o /api-app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS debug
RUN apt-get update && apt-get install -y unzip curl && rm -rf /var/lib/apt/lists/*
RUN curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg
WORKDIR /source
ENTRYPOINT ["dotnet", "run", "--project", "./src/BookStorage.Api/BookStorage.Api.csproj", "--no-launch-profile"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0
RUN apt-get update && apt-get install -y curl --no-install-recommends && rm -rf /var/lib/apt/lists/*
EXPOSE 8080
WORKDIR /api-app
COPY --link --from=build /api-app .
ENTRYPOINT ["dotnet", "BookStorage.Api.dll"]
