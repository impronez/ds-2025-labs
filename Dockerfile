# Используем образ SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /source

# Копируем файлы проекта
COPY *.sln . 
COPY Valuator/*.csproj ./Valuator/

# Восстанавливаем зависимости
RUN dotnet restore

# Копируем исходный код
COPY Valuator/. ./Valuator/

# Публикуем приложение
RUN dotnet publish -c release -o /app

# Проверка содержимого /app после публикации
RUN ls -l /app

# Финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app ./

# Проверяем, что файл .dll присутствует в финальном образе
RUN ls -l /app

# Запуск приложения с абсолютным путем
ENTRYPOINT ["dotnet", "Valuator.dll"]
