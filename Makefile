.PHONY: pre-commit dev build migration

dev:
	docker compose up --build

build:
	docker compose build --no-cache

pre-commit:
	cd backend && dotnet format
	cd backend && dotnet build

migration:
	@if [ -z "$(name)" ]; then echo "Error: Migration name required. Usage: make migration name=<name>"; exit 1; fi
	cd backend && dotnet ef migrations add "$(name)" -p MeterReadings.Data -s MeterReadings.API -o Migrations