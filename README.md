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
│   ├── wwwroot/      # Compiled frontend files (production mode)
│   ├── Controllers/  # API controllers
│   ├── Data/         # Database context
│   └── Models/       # Data models
├── frontend/         # React + Vite application
│   └── dist/         # Build output (created by npm run build)
└── database.sql      # SQL schema and seed data
```

## Running the Application

### Option 1: Development Mode (Separate Frontend & Backend)

Run the frontend and backend separately for development with hot-reload:

**1. Start the Backend:**

```bash
cd backend
dotnet restore
dotnet run
```

The backend API will start at `http://localhost:5000` (check console output for exact port).

**2. Start the Frontend (in a new terminal):**

```bash
cd frontend
npm install
npm run dev
```

The frontend will start at `http://localhost:5173` (or similar - check console output).

### Option 2: Production Mode (Backend Serves Frontend)

The backend is configured to serve the compiled frontend from the `wwwroot` folder. This is the deployment model used in production where a single server hosts both the API and the static frontend files.

**1. Build the Frontend:**

```bash
cd frontend
npm install
npm run build
```

This creates a production build in `backend/wwwroot`.

**3. Run the Backend:**

```bash
cd backend
dotnet run
```

The backend will now serve both the API and the frontend at:
   - Application: `http://localhost:5000`
   - Swagger UI: `http://localhost:5000/swagger`

> **Note**: The backend uses `UseStaticFiles()` and `MapFallbackToFile("index.html")` to serve the React SPA. All non-API routes are handled by the React router.


## Accessing the Application

### Development Mode:
- **Frontend**: `http://localhost:5173`
- **Backend API**: `http://localhost:5000`
- **Swagger Documentation**: `http://localhost:5000/swagger`

### Production Mode (Backend serving frontend):
- **Application & API**: `http://localhost:5000`
- **Swagger Documentation**: `http://localhost:5000/swagger`

## Database

The application uses SQL Server LocalDB with the following configuration:
- **Database Name**: StyleVerseDb
- **Connection String**: Configured in `backend/appsettings.json`

The database schema is automatically created on first run using Entity Framework Core migrations. If you need to manually set up the database, you can use the `database.sql` script.

## Available Scripts

### Backend

- `dotnet run` - Run the application
- `dotnet build` - Build the project
- `dotnet restore` - Restore NuGet packages

### Frontend

- `npm run dev` - Start development server with hot-reload
- `npm run build` - Build for production (output to `dist/` folder)
- `npm run preview` - Preview production build locally
- `npm run lint` - Run ESLint
- `npm install` - Install dependencies

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
