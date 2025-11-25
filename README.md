fs-2025-assessment-1-74154
Student Name: Gabriel Blauth de Araujo
Student Number: 74154

A.NET 8 Web API with full CRUD functions, caching, and real-time simulation for DublinBike station data.

FEATURES:
- Real-time Data Simulation: Background service updates station availability every 15 seconds
- In-Memory Caching: 5-minute cache for improved performance
- API Versioning: V1 (File-based JSON) and V2 (Future CosmosDB)
- Full CRUD Operations: Create, read, update, delete stations
- Advanced Filtering: Search by status, minimum bikes, name, and address
- Sorting & Pagination: Sort by name, available bikes, occupancy with pagination
- Computed Properties: Occupancy rate and local Dublin time conversion

ASSIGNMENT REQUIREMENTS:
- Data Loading & Modeling: JSON file loaded at startup with proper models  
- API Endpoints: All required GET, POST, PUT endpoints implemented  
- Background Service: Random station updates every 15 seconds  
- Memory Caching: 5-minute cache for all queries  
- API Versioning: V1 and V2 with same endpoints  
- Filtering/Searching/Sorting/Paging: Full implementation  
- Date/Time Handling: Epoch to Dublin time conversion  
- Unit Tests: Filtering logic and happy path tests  
- Documentation: This README with setup instructions

SETUP AND INSTALATION
PREREQUISITES:
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code

INSTALLATION:
1. Clone the repository:
git clone https://github.com/GabrielBlauth/fs-2025-assessment-1-74154.git

