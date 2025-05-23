@echo off

cd /d %~dp0\..

docker-compose down

docker-compose -f docker-compose.selenium.yaml down

docker network rm app-network
