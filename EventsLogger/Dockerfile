FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /source

COPY *.sln ./
COPY ./Services/*.csproj ./Services/
COPY ./EventsLogger/*.csproj ./EventsLogger/

RUN dotnet restore EventsLogger/EventsLogger.csproj

COPY ./Services/ ./Services/
COPY ./EventsLogger/ ./EventsLogger/
COPY ./EventsLogger/EventsLogger*.json ./EventsLogger/

RUN dotnet publish EventsLogger/EventsLogger.csproj -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app .
COPY --from=build /source/EventsLogger/appsettings*.json .

ENTRYPOINT ["dotnet", "EventsLogger.dll"]