FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

RUN apt-get update && apt-get install -y netcat-openbsd

COPY System.Specs/*.csproj ./System.Specs/
RUN dotnet restore ./System.Specs/System.Specs.csproj

COPY . .

RUN dotnet build ./System.Specs/System.Specs.csproj -c Release

COPY System.Specs/wait-for-it.sh /wait-for-it.sh
RUN chmod +x /wait-for-it.sh

CMD ["/wait-for-it.sh", "nginx:8080", "--", "dotnet", "test", "./System.Specs/System.Specs.csproj", "--logger:trx"]
