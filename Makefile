.PHONY: pre-commit

pre-commit:
	cd backend && dotnet format
	cd backend && dotnet build