@echo off

cd /d %~dp0\..

docker-compose down

docker network create app-network

docker-compose -f docker-compose.selenium.yaml up -d

docker-compose up -d webapp1 webapp2 rank-calculator events-logger1 events-logger2 redis rabbitmq nginx

docker-compose up --abort-on-container-exit --exit-code-from e2e-tests e2e-tests

docker-compose -f docker-compose.selenium.yaml stop
