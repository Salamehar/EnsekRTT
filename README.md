# Meter Readings Application

A full-stack application for processing meter readings via CSV uploads. This application is built with .NET 9.0 for the backend API and React for the frontend, using PostgreSQL as the database.

## Project Architecture

```
EnsekRTT/
├── backend/
│   ├── MeterReadings.API/           # API controllers and configuration
│   ├── MeterReadings.Core/          # Business logic, models, interfaces
│   ├── MeterReadings.Data/          # Data access, EF Core, repositories
│   ├── MeterReadings.Infrastructure/ # Service implementations
│   └── MeterReadings.Test/          # Tests
│   └── Dockerfile                   # backend container configuration
│   └── MeterReadings.sln            # .Net solution
├── frontend/                        
│   ├── public/                      # Static assets
│   ├── src/                         # Source code
│   │   ├── components/              # React components
│   │   └── styles/                  # CSS stylesheets
│   ├── Dockerfile                   # Frontend container configuration
│   └── nginx.conf                   # Nginx proxy configuration
└── docker-compose.yml               # Container orchestration
└── Makefile                         # Makefile
└── README.md                        # This file
```

### Architecture Diagram

```
┌──────────────────────────────────────────────────────────────────┐
│                         Client Browser                           │
└───────────────────────────────┬──────────────────────────────────┘
                                │
                                ▼
┌──────────────────────────────────────────────────────────────────┐
│                     Frontend (React + Nginx)                     │
│                                                                  │
│  ┌─────────────────────┐            ┌────────────────────────┐   │
│  │   Database Status   │            │  Meter Reading Upload  │   │
│  │     Component       │            │      Component         │   │
│  └─────────────────────┘            └────────────────────────┘   │
└───────────────────────────┬──────────────────────────────────────┘
                            │
              API Requests  │
                            ▼
┌──────────────────────────────────────────────────────────────────┐
│                        Backend (.NET 9.0)                        │
│                                                                  │
│  ┌─────────────────────┐            ┌────────────────────────┐   │
│  │  API Controllers    │            │     Services Layer     │   │
│  │                     │◄──────────►│                        │   │
│  │  - MeterReading     │            │  - MeterReadingService │   │
│  └─────────────────────┘            │  - CsvParserService    │   │
│            ▲                        └────────────┬───────────┘   │
│            │                                     │               │
│            │           ┌──────────────┐          │               │
│            │           │  Validators  │          │               │
│            └───────────┤              ◄──────────┘               │
│                        │              │                          │
│                        └──────────────┘                          │
│                              ▲                                  │
│  ┌─────────────────────┐     │     ┌────────────────────────┐    │
│  │   Repositories      │     │     │     Data Models        │    │
│  │                     │◄────┴────►│                        │    │
│  │  - AccountRepo      │           │  - Account             │    │
│  │  - MeterReadingRepo │           │  - MeterReading        │    │
│  └──────────┬──────────┘           └────────────────────────┘    │
│             │                                                    │
└─────────────┼────────────────────────────────────────────────────┘
              │
              ▼
┌──────────────────────────────────────────────────────────────────┐
│                     PostgreSQL Database                          │
└──────────────────────────────────────────────────────────────────┘
```

## Running the Application

### Using Docker (Complete Environment)

The simplest way to run the entire application is using Docker Compose via the Makefile:

```bash
# Start all services (PostgreSQL, API, and frontend)
make dev
```

This will start:
- PostgreSQL database on port 5432
- .NET API on port 8000
- React frontend on port 3000

Open your browser and navigate to http://localhost:3000 to access the application.

### Running Locally (With Database in Container)

To run the application locally with only the database in a container:

1. Start PostgreSQL container:
```bash
docker-compose up db -d
```

2. Run the backend API:
```bash
cd backend
dotnet run --project MeterReadings.API/MeterReadings.API.csproj
```

3. Run the frontend:
```bash
cd frontend
npm install
npm start
```

## Makefile Commands

The project includes a Makefile for common operations:

| Command | Description |
|---------|-------------|
| `make dev` | Start all services with Docker Compose |
| `make build` | Build all Docker images from scratch (no cache) |
| `make pre-commit` | Format code and validate build |
| `make migration name=<name>` | Add a new database migration |
| `make db.update` | Apply pending database migrations |
| `make db.reset` | Drop and recreate the database |
| `make db.seed` | Seed the database with test accounts |
| `make test` | Run all tests |

## Backend Architecture

### Overview

The backend follows a clean, layered architecture pattern:

1. **API Layer (MeterReadings.API)**
   - Controllers for handling HTTP requests
   - Dependency injection configuration
   - Middleware setup

2. **Core Layer (MeterReadings.Core)**
   - Domain models and DTOs
   - Business logic interfaces
   - Validators

3. **Data Layer (MeterReadings.Data)**
   - Entity Framework Core DbContext
   - Repositories
   - Database migrations
   - Data seeding

4. **Infrastructure Layer (MeterReadings.Infrastructure)**
   - Service implementations
   - External integrations (CSV parsing)

### Key Features

#### Validation Pipeline

The application uses FluentValidation for meter reading validation. Key validations include:

- **Account Existence**: Verifies the account ID exists in the database
- **Meter Reading Format**: Ensures readings follow the NNNNN (5 digits) format
- **Duplicate Prevention**: Checks that readings aren't duplicated for the same account/date
- **Chronological Validation**: Ensures readings aren't older than existing ones

```csharp
public class MeterReadingDtoValidator : AbstractValidator<MeterReadingDto>
{
    public MeterReadingDtoValidator(
        IAccountRepository accountRepository,
        IMeterReadingRepository meterReadingRepository)
    {
        RuleFor(x => x.AccountId)
            .NotEmpty()
            .MustAsync(AccountExists)
            .WithMessage("Account ID does not exist.");

        RuleFor(x => x.MeterReadValue)
            .NotEmpty()
            .Matches(@"^\d{5}$")
            .WithMessage("Meter reading value must be in the format NNNNN (5 digits).");

        RuleFor(x => x)
            .MustAsync(ReadingNotDuplicate)
            .WithMessage("A reading with this account ID and date already exists.")
            .MustAsync(ReadingNotOlderThanExisting)
            .WithMessage("A newer reading already exists for this account.");
    }
    
    // Implementation methods...
}
```

#### Repository Pattern

The application implements the Repository pattern to abstract data access:

- **IAccountRepository**: Operations for account data
- **IMeterReadingRepository**: Operations for meter reading data

This pattern:
- Decouples business logic from data access
- Makes testing easier with mock repositories
- Centralises data access logic

#### CSV Processing & File Uploads

The meter reading upload workflow follows these steps:

1. **File Upload**: 
   - CSV file is submitted via `POST /api/meter-reading-uploads`
   - Controller validates it's a non-empty CSV file
   - File stream is passed to the meter reading service

2. **CSV Parsing**:
   - **ICsvParserService** defines the interface for parsing CSV files
   - **CsvParserService** implements parsing using CsvHelper library
   - Converts CSV rows to `MeterReadingDto` objects

3. **Validation**:
   - Each `MeterReadingDto` is validated using FluentValidation rules
   - Readings must have valid account IDs, proper format, and follow temporal rules
   - Validation results determine which readings are processed or rejected

4. **Data Processing**:
   - Valid readings are converted to domain models
   - Saved to database via repository
   - Statistics are calculated (successful vs. failed readings)

5. **Response**:
   - Returns `MeterReadingUploadResultDto` with counts of successful and failed readings
   - Frontend displays these results as a pie chart



### Best Practices

The backend implements several best practices:

- **Dependency Injection**: All services registered in Program.cs
- **Validation Pipeline**: Using FluentValidation for robust input validation
- **Repository Pattern**: Clean separation between data access and business logic
- **Clean Architecture**: Separation of concerns with clear layers
- **Explicit Error Handling**: Consistent error responses and logging
- **Environment-Based Configuration**: Different settings for development/production

## Frontend Architecture

### Overview

The frontend is a React application that provides a clean interface for:

1. Viewing database connection status and migrations
2. Uploading meter reading CSV files
3. Visualising upload results with a pie chart

### Components

The frontend consists of two main components that interact with the API endpoints:

#### DatabaseStatus Component

Fetches and displays database connection status and migrations information:

```javascript
function DatabaseStatus() {
  const [status, setStatus] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchStatus = async () => {
      try {
        const response = await axios.get('/api/db-status');
        setStatus(response.data);
        // ...
      } catch (err) {
        setError('Failed to fetch database status');
      }
    };

    fetchStatus();
    // Poll every 30 seconds
    const interval = setInterval(fetchStatus, 30000);
    return () => clearInterval(interval);
  }, []);
  
  // Render component...
}
```

Features:
- Polls the `/api/db-status` endpoint every 30 seconds
- Shows connection status (connected/disconnected)
- Lists pending and applied migrations
- Error handling for API failures
- Loading state while fetching data

#### MeterReadingUpload Component

Handles CSV file uploads and visualises results:

```javascript
function MeterReadingUpload() {
  const [file, setFile] = useState(null);
  const [uploading, setUploading] = useState(false);
  const [result, setResult] = useState(null);
  
  const handleSubmit = async (e) => {
    e.preventDefault();
    
    // Upload validation and processing
    const formData = new FormData();
    formData.append('file', file);
    
    const response = await axios.post('/api/meter-reading-uploads', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
    
    setResult(response.data);
    // ...
  };
  
  // Chart configuration and rendering...
}
```

Features:
- File selection with drag-and-drop styling
- Validation of file type (CSV only)
- Upload progress indicator
- Displays results as a color-coded pie chart
  - Green section for successful readings
  - Maroon section for failed readings
- Shows detailed statistics in a summary table:
  - Successful readings count
  - Failed readings count
  - Total processed count
- Error handling for API failures and validation issues

### Design

The frontend follows a clean, modern design:

- Responsive layout with two main columns
- Card-based UI components with subtle shadows
- Blue gradient header
- Clear visual hierarchy
- Interactive elements (file upload, buttons)
- Color coding for success/failure states

## Database

The application uses PostgreSQL with Entity Framework Core:

- **Migrations**: Database changes are tracked with EF Core migrations
- **Seeding**: Initial test accounts are loaded from `Test_Accounts.csv`
- **Connection**: Configured via connection strings in appsettings.json or environment variables

### Database Schema

#### Accounts Table
```
+------------+--------------+-------------------------------------------+
| Column     | Type         | Description                               |
+------------+--------------+-------------------------------------------+
| AccountId  | INT          | Primary Key, not auto-incremented         |
| FirstName  | VARCHAR(100) | Account holder's first name               |
| LastName   | VARCHAR(100) | Account holder's last name                |
+------------+--------------+-------------------------------------------+
```

#### MeterReadings Table
```
+--------------------+--------------+-------------------------------------------+
| Column             | Type         | Description                               |
+--------------------+--------------+-------------------------------------------+
| Id                 | INT          | Primary Key, auto-incremented             |
| AccountId          | INT          | Foreign Key to Accounts table             |
| MeterReadingDateTime | TIMESTAMP  | Date and time of the meter reading        |
| MeterReadValue     | INT          | The 5-digit meter reading value           |
+--------------------+--------------+-------------------------------------------+
```

**Constraints**:
- Unique index on (AccountId, MeterReadingDateTime) to prevent duplicates
- Foreign key relationship from MeterReadings to Accounts
- Cascade delete (if an account is deleted, all its readings are also removed)

The database schema is defined in `MeterReadingDbContext.cs` using Entity Framework Core's fluent API.

## API Endpoints

The API exposes two main endpoints:

### 1. GET /api/db-status

Checks and returns database connection status and migration information.

**Response Example:**
```json
{
  "canConnect": true,
  "pendingMigrations": [],
  "appliedMigrations": [
    "20230101120000_InitialCreate",
    "20230115083000_AddMeterReadingsTable"
  ]
}
```

### 2. POST /api/meter-reading-uploads

Processes CSV file uploads containing meter readings.

**Request:**
- Content-Type: multipart/form-data
- Body: CSV file with meter readings

**CSV Format:**
```
AccountId,MeterReadingDateTime,MeterReadValue
2344,22/04/2019 09:24,1002
2233,22/04/2019 12:25,323
...
```

**Response:**
```json
{
  "successfulReadings": 15,
  "failedReadings": 3
}
```

**Validation Rules:**
- Account ID must exist in the database
- Meter reading value must be a 5-digit number
- Reading datetime must be in format "dd/MM/yyyy HH:mm"
- No duplicate readings (same account ID and datetime)
- No readings with datetime older than existing readings for the same account

## Deployment

The application is containerised for easy deployment:

- **Backend**: .NET 9.0 API in a container
- **Frontend**: React app served via Nginx
- **Database**: PostgreSQL container
- **Networking**: Internal Docker network for service communication

## Getting Started

1. Clone the repository
2. Install the prerequisites:
   - .NET 9.0 SDK
   - Node.js and npm
   - Docker and Docker Compose
3. Run `make dev` to start the development environment

---
**Contact**  
Feel free to reach out to me at <a href="mailto:salameh@sysfiction.com">salameh@sysfiction.com</a>  
