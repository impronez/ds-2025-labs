FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /source

COPY ../*.sln .  
COPY ./Services/*.csproj ./Services/
COPY ./Valuator/*.csproj ./Valuator/

RUN dotnet restore Valuator/Valuator.csproj

COPY ./Services/ ./Services/
COPY Valuator/. ./Valuator/
COPY ./Valuator/appsettings*.json ./Valuator/

RUN dotnet publish Valuator/Valuator.csproj -c release -o /app

RUN ls -l /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app ./
COPY --from=build /source/Valuator/appsettings*.json .

RUN ls -l /app

ENTRYPOINT ["dotnet", "Valuator.dll"]
