# BitsBlog

ASP.NET Core 9 WebAPI + Entity Framework + React + Typescript example. A simple Blog implementation based on clean architecture.

## Project Structure

- `src/BitsBlog.Domain` - Domain Entities
- `src/BitsBlog.Application` - Services and Interfaces
- `src/BitsBlog.Infrastructure` - EF Core DbContext and Repositories
- `src/BitsBlog.WebApi` - REST API
- `src/BitsBlog.Web` - ASP.NET Core MVC Client
- `client-react` - React + Typescript Client Sample

## How to Run

```bash
# Web API
cd src/BitsBlog.WebApi
# dotnet run

# MVC Client
cd src/BitsBlog.Web
# dotnet run

# React Client (e.g., build with Vite)
cd client-react
# cp ../.env.example .env # Configure environment variables
# npm install && npm run dev
```

The `VITE_API_URL` value in the `.env` file should point to the base address of the backend Web API.

The `dotnet` command may not run in the current environment because the .NET SDK is not installed.
