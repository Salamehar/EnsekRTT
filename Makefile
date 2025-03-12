.PHONY: pre-commit dev build migration db.update db.reset db.seed test

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

db.update:
	cd backend && dotnet ef database update -p MeterReadings.Data -s MeterReadings.API

db.reset:
	docker compose up db -d
	cd backend && dotnet ef database drop -p MeterReadings.Data -s MeterReadings.API --force
	cd backend && dotnet ef database update -p MeterReadings.Data -s MeterReadings.API
	docker compose down db

db.seed:
	cd backend && dotnet run --project MeterReadings.API/MeterReadings.API.csproj seed

test:
	cd backend && dotnet test