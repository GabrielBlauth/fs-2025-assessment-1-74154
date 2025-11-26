fs-2025-assessment-1-74154
Student Name: Gabriel Blauth de Araujo
Student Number: 74154

A .NET 8 Web API with full CRUD functions, caching, and real-time simulation for DublinBike station data.

## FEATURES
- Real-time Data Simulation: Background service updates station availability every 30 seconds
- In-Memory Caching: 5-minute cache for improved performance
- API Versioning: V1 (File-based JSON) and V2 (Future CosmosDB)
- Full CRUD Operations: Create, read, update stations
- Advanced Filtering: Search by status, minimum bikes, name, and address
- Sorting & Pagination: Sort by name, available bikes, occupancy with pagination
- Computed Properties: Occupancy rate and local Dublin time conversion

## ASSIGNMENT REQUIREMENTS
- Data Loading & Modeling: JSON file loaded at startup with proper models  
- API Endpoints: All required GET, POST, PUT endpoints implemented  
- Background Service: Random station updates every 30 seconds  
- Memory Caching: 5-minute cache for all queries  
- API Versioning: V1 and V2 with same endpoints  
- Filtering/Searching/Sorting/Paging: Full implementation  
- Date/Time Handling: Epoch to Dublin time conversion  
- Unit Tests: Filtering logic and happy path tests  
- Documentation: This README with setup instructions

## SETUP AND INSTALLATION

**PREREQUISITES:**
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code

**INSTALLATION:**
1. Clone the repository:
git clone https://github.com/GabrielBlauth/fs-2025-assessment-1-74154.git

Run the application:
cd fs-2025-assessment-1-74154 (dotnet run)

Access the API:
Swagger UI: https://localhost:7146/swagger
API Base: https://localhost:7146

API ENDPOINTS
V1 - File JSON:
- GET /api/v1/stations
- GET /api/v1/stations/{number}
- GET /api/v1/stations/summary
- POST /api/v1/stations
- PUT /api/v1/stations/{number}

V2 - CosmosDB:
- GET /api/v2/stations
- GET /api/v2/stations/{number}
- GET /api/v2/stations/summary
- POST /api/v2/stations
- PUT /api/v2/stations/{number}

## TESTING
Unit Tests:
dotnet test

## POSTMAN TESTING
1. Import `DublinBikes-API.postman_collection.json` into Postman
2. Set environment variable `baseUrl` to `https://localhost:7146`
3. Run the collection using Postman Test Runner
4. All tests should pass (409 conflicts are expected for duplicate stations)

