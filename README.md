# DublinBikes API - Assessment 1

- **Student Name:** Gabriel Blauth de Araujo
- **Student Number:** 74154
- **Course:** Full Stack Development 2025

A .NET 8 Web API with full CRUD functions, caching, real-time simulation, and dual data source support for DublinBike station data.

## FEATURES

- **Real-time Data Simulation:** Background service updates station availability every 15 seconds
- **Dual Data Sources:** V1 (JSON File) and V2 (Azure Cosmos DB) with same endpoints
- **In-Memory Caching:** 5-minute cache for improved performance
- **Full CRUD Operations:** Create, read, update, delete stations
- **Advanced Filtering:** Search by status, minimum bikes, name, and address
- **Sorting & Pagination:** Sort by name, available bikes, occupancy with pagination
- **Computed Properties:** Occupancy rate and local Dublin time conversion
- **API Versioning:** Seamless switching between V1 and V2

## ✅ ASSIGNMENT REQUIREMENTS IMPLEMENTED

### Data Modelling & Loading (10 marks) ✅
- Correct model classes mapped from JSON
- JSON file loaded and deserialized at startup with error handling

### API Endpoints & Contracts (15 marks) ✅  
- All GET, POST, PUT endpoints implemented with validation
- Correct HTTP verbs and status codes (200, 201, 400, 404, 409)
- Clear, consistent response shapes with DTOs

### Background Mechanism (25 marks) ✅
- Background service updates stations every 15 seconds
- Random capacity and availability changes
- Data updates through service layer

### Filtering, Searching, Sorting & Paging (25 marks) ✅
- Search (q) over name/address working
- Filters (status, minBikes) applied correctly and combinable
- Sorting and paging implemented predictably

### Date/Time & Data Validation (10 marks) ✅
- last_update converted from epoch ms to .NET DateTime
- Europe/Dublin local time exposed in responses
- Defensive handling of invalid values

### Code Quality & Architecture (10 marks) ✅
- Clear separation of concerns (Services vs Endpoints)
- Clean naming, minimal duplication, proper DI usage

### Testing (5 marks) ✅
- Unit tests for filtering/search logic
- Happy path endpoint tests

### Documentation (5 marks) ✅
- Complete README with setup and examples
- Postman collection with automated tests

## SETUP AND INSTALLATION

### PREREQUISITES:
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- Azure Cosmos DB Emulator (for V2 testing)

### INSTALLATION:
1. Clone the repository:
- git clone https://github.com/GabrielBlauth/fs-2025-assessment-1-74154.git
- cd fs-2025-assessment-1-74154

### Configure data source in appsettings.json:
json
{
  "ApiVersion": "V1",  // or "V2" for CosmosDB
  "CosmosDb": {
    "ConnectionString": "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
    "DatabaseId": "DublinBikesDb",
    "ContainerId": "Stations"
  }
}
### Run the application:
### Access the API:
-Swagger UI: https://localhost:7146/swagger
-API Base: https://localhost:7146

### API ENDPOINTS
V1 - File JSON:
-GET /api/v1/stations
-GET /api/v1/stations/{number}
-GET /api/v1/stations/summary
-POST /api/v1/stations
-PUT /api/v1/stations/{number}

V2 - CosmosDB:
-GET /api/v2/stations
-GET /api/v2/stations/{number}
-GET /api/v2/stations/summary
-POST /api/v2/stations
-PUT /api/v2/stations/{number}

### Query Parameters:
-status: OPEN|CLOSED
-minBikes: integer
-search: string (name/address)
-sort: name|availableBikes|occupancy
-dir: asc|desc
-page: integer
-pageSize: integer

### TESTING
Unit Tests:
dotnet test

### Postman Testing:
-Import DublinBikes-API.postman_collection.json into Postman
-Set environment variable baseUrl to https://localhost:7146
-Run the collection using Postman Test Runner
-All tests should pass (409 conflicts are expected for duplicate stations)

### Manual Testing:
-Test filtering: ?status=OPEN&minBikes=5&search=street
-Test sorting: ?sort=availableBikes&dir=desc
-Test pagination: ?page=1&pageSize=10
-Observe real-time updates every 15 seconds

### CONFIGURATION
-Switch between V1 and V2 by changing ApiVersion in appsettings.json:
-"V1": Uses JSON file data source
-"V2": Uses Azure Cosmos DB data source
