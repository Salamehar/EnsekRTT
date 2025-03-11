.PHONY: pre-commit dev build

dev:
    docker compose up --build

build:
    docker compose build --no-cache

pre-commit:
    cd backend && dotnet format
    cd backend && dotnet build