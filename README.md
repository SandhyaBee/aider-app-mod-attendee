# AI-Powered Developer Escape Room: App Modernization

StyleVerse is an e-commerce application built with a .NET 8 backend and React frontend. This repository is part of the AI-Powered Developer Escape Room for App Modernization challenges.

## Prerequisites

Before running this application, ensure you have the following installed:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (v18 or higher)
- [SQL Server LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) (included with Visual Studio or SQL Server Express)

## Project Structure

```
├── backend/          # ASP.NET Core Web API
├── frontend/         # React + Vite application
└── database.sql      # SQL schema and seed data
```

## Running the Application

**1. Start the Backend:**

```bash
cd backend
dotnet restore
dotnet run
```

The backend API will start at `https://localhost:7200` (or similar - check console output).

**2. Start the Frontend (in a new terminal):**

```bash
cd frontend
npm install
npm run dev
```

The frontend will start at `http://localhost:5173` (or similar - check console output).


1. The API should be running at:
   - HTTP: `http://localhost:5000` (default)
   - Swagger UI: `http://localhost:5000/swagger`


## Accessing the Application

- **Frontend**: Open your browser to `http://localhost:5173`
- **Backend API**: `https://localhost:5000`
- **Swagger Documentation**: `https://localhost:5000/swagger`

## Database

The application uses SQL Server LocalDB with the following configuration:
- **Database Name**: StyleVerseDb
- **Connection String**: Configured in `backend/appsettings.json`

The database schema is automatically created on first run using Entity Framework Core migrations. If you need to manually set up the database, you can use the `database.sql` script.

## Troubleshooting

**Database Connection Issues:**
- Ensure SQL Server LocalDB is installed
- Check the connection string in `backend/appsettings.json`
- Try deleting the database and letting it recreate: `sqllocaldb stop mssqllocaldb && sqllocaldb delete mssqllocaldb`

**Port Conflicts:**
- If ports are already in use, you can modify:
  - Backend: `backend/Properties/launchSettings.json`
  - Frontend: `frontend/vite.config.js`

**CORS Errors:**
- The backend is configured to allow all origins for development
- Ensure both frontend and backend are running

## Technologies Used

- **Backend**: ASP.NET Core 8, Entity Framework Core, SQL Server
- **Frontend**: React 18, Vite, JavaScript
- **API Documentation**: Swagger/OpenAPI
