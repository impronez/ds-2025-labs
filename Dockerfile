FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /source

COPY *.sln . 
COPY Valuator/*.csproj ./Valuator/

RUN dotnet restore

COPY Valuator/. ./Valuator/

RUN dotnet publish -c release -o /app

RUN ls -l /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app ./

RUN ls -l /app

ENTRYPOINT ["dotnet", "Valuator.dll"]
