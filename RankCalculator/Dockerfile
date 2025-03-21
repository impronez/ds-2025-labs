FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /source

COPY *.sln ./
COPY ./Services/*.csproj ./Services/
COPY ./RankCalculator/*.csproj ./RankCalculator/

RUN dotnet restore RankCalculator/RankCalculator.csproj

COPY ./Services/ ./Services/
COPY ./RankCalculator/ ./RankCalculator/

RUN dotnet publish RankCalculator/RankCalculator.csproj -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app .

ENTRYPOINT ["dotnet", "RankCalculator.dll"]