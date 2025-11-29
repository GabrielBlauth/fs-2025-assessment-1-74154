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
