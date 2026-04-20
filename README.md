# Great Region Concerts

A web app that aggregates upcoming concert listings from venues in Luxembourg's Greater Region. Built as an Azure Static Web App with a React frontend and .NET Azure Functions API backend.

## Architecture

- **Frontend**: React SPA (served as static files)
- **API**: .NET Azure Functions (isolated worker model) that scrapes concert data from venue websites
- **Hosting**: Azure Static Web Apps
- **Venues covered**: Rockhal, Atelier, Kulturfabrik (with more possible in the future)

## How it works

The API uses AngleSharp to scrape concert listings from each venue's website, aggregates them, groups by date, and returns them as JSON. The React frontend fetches and displays this data.

## Getting started

### Prerequisites

- Node.js 18+
- .NET 9 SDK
- Azure Static Web Apps CLI (optional, for local full-stack dev with `swa start`)

### Frontend

```sh
npm install
npm start
```

### API

```sh
cd api
dotnet run
```

### Full-stack local dev

```sh
swa start http://localhost:3000 --api-location ./api
```

## Project structure

```
├── src/              # React frontend
├── api/              # .NET Azure Functions API
│   ├── Scrapers/     # Web scrapers for each venue
│   ├── Models/       # Data models
│   └── GetConcerts.cs # Main API endpoint
├── public/           # Static assets
└── staticwebapp.config.json
```

## Deployment

Deployed automatically via GitHub Actions to Azure Static Web Apps on push to main.
