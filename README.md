# Göztepe Volleyball Scraper

A comprehensive web application that scrapes and displays Göztepe Sports Club volleyball team fixtures, standings, and match results from the İzmir Voleybol İl Temsilciliği (İzmir Provincial Volleyball Association) website.

## 🏐 Features

### Backend API

- **Real-time Data Scraping**: Automatically fetches volleyball data from the provincial federation website
- **Smart Caching System**: In-memory caching with configurable durations to reduce federation site load
- **RESTful API**: Clean, documented endpoints with OpenAPI/Swagger support
- **Flexible Filtering**: Filter fixtures and standings by season, league, category, division, group, and more
- **Competition Management**: Handles multiple competitions (group stage, playoffs, finals) per season
- **Göztepe-Focused**: Automatically identifies and highlights Göztepe games across all leagues

### Frontend UI

- **Responsive Design**: Works seamlessly on desktop, tablet, and mobile devices
- **Turkish Localization**: Full Turkish date formatting and language support
- **Real-time Updates**: Live fixture and standings data with manual refresh capability
- **Göztepe Branding**: Custom red color scheme matching club identity
- **Interactive Filtering**: Filter games by league, division, and competition
- **Cache Management**: View and clear cached data with detailed statistics
- **Unified Game Cards**: Consistent display for both fixtures and match results

## 🚀 Quick Start

### Prerequisites

- .NET 10.0 SDK
- Node.js 18+
- npm or yarn

### Backend Setup

```bash
cd backend
dotnet restore
dotnet run --project VolleyballScraper.Api
```

The API will be available at `http://localhost:5281`

### Frontend Setup

```bash
cd frontend
npm install
npm run dev
```

The web app will be available at `http://localhost:5173`

## ⚡ Quick Build & Publish

### Automated Build Script

The project includes automated build scripts that test, build, and publish both backend and frontend:

#### For macOS/Linux:

```bash
# Make script executable (one time only)
chmod +x publish.sh

# Run the full build process
./publish.sh

# Or use npm from project root
npm run publish
```

#### For Windows:

```powershell
# Run the PowerShell script
.\publish.ps1

# Or use npm from project root
npm run publish:win
```

### What the Build Script Does:

1. **🧪 Runs Backend Tests** - Ensures code quality
2. **🔨 Builds Backend** - Creates optimized API build
3. **🎨 Builds Frontend** - Creates optimized Vue.js build
4. **📦 Publishes Both** - Copies everything to `./publish/` directory

### Build Output Structure:

```
publish/
├── api/           # Backend API (.NET build)
└── frontend/      # Frontend files (Vue.js build)
```

## 📡 API Documentation

### Base URL

```
http://localhost:5281/api
```

### Fixture Endpoints

#### Get Available Leagues

```http
GET /api/fixture/leagues
```

**Response:**

```json
[
  {
    "code": "GKSL",
    "displayName": "Genç Kızlar Süper Ligi",
    "category": "GK"
  },
  {
    "code": "YKSL",
    "displayName": "Yıldızlar Kızlar Süper Ligi",
    "category": "YK"
  }
]
```

#### Get Fixture Games

```http
POST /api/fixture/games?forceRefresh=false
Content-Type: application/json

{
  "seasonId": "2025-2026",
  "leagues": ["GKSL", "YKSL"],
  "division": "MDKSL",
  "category": "MdK",
  "matchType": "KL",
  "group": "A",
  "round": "1D",
  "week": "1"
}
```

**Parameters:**

- `seasonId` (string): Season identifier (e.g., "2025-2026")
- `leagues` (array, optional): League codes to filter by
- `division` (string, optional): Division filter (e.g., "MDKSL", "KK1L")
- `category` (string, optional): Category filter (e.g., "GK", "KK", "MdK", "YK")
- `matchType` (string, optional): Match type filter (e.g., "KL", "CF", "YF", "FI")
- `group` (string, optional): Group filter (e.g., "A", "B", "C")
- `round` (string, optional): Round filter (e.g., "1D", "2D")
- `week` (string, optional): Week number filter (e.g., "1", "5")
- `forceRefresh` (query param): Bypass cache and fetch fresh data

**Response:**

```json
{
  "total": 45,
  "season": "2025-2026",
  "cachedLeagues": 3,
  "filters": {
    "division": "MDKSL",
    "category": "MdK"
  },
  "leagues": [
    {
      "league": "GKSL",
      "count": 12,
      "games": [
        {
          "date": "28.01.2026",
          "time": "18:30",
          "venue": "Göztepe Spor Salonu",
          "homeTeam": "Göztepe Spor Kulübü",
          "awayTeam": "Karşıyaka GSK",
          "score": "",
          "division": "GKSL",
          "group": "A",
          "league": "GKSL"
        }
      ]
    }
  ]
}
```

### Standings Endpoints

#### Get Available Competitions

```http
POST /api/standings/competitions
Content-Type: application/json

{
  "seasonId": "2025-2026",
  "category": "GK",
  "leagueCode": "GKSL"
}
```

**Response:**

```json
[
  {
    "name": "2025-2026 GKSL Grup A",
    "displayName": "Genç Kızlar Süper Ligi - Grup A",
    "league": "Genç Kızlar Süper Ligi",
    "leagueCode": "GKSL",
    "category": "GK",
    "division": "GKSL",
    "group": "A",
    "hasGoztepe": true
  }
]
```

#### Get Standings and Match Results

```http
POST /api/standings
Content-Type: application/json

{
  "seasonId": "2025-2026",
  "category": "GK",
  "leagueCode": "GKSL",
  "competitionName": "2025-2026 GKSL Grup A"
}
```

**Response:**

```json
{
  "competition": {
    "name": "2025-2026 GKSL Grup A",
    "displayName": "Genç Kızlar Süper Ligi - Grup A",
    "seasonId": "2025-2026"
  },
  "standings": [
    {
      "rank": 1,
      "teamName": "Göztepe Spor Kulübü",
      "logoUrl": "https://example.com/goztepe-logo.png",
      "played": 8,
      "won": 6,
      "lost": 2,
      "setsWon": 19,
      "setsLost": 10,
      "pointsFor": 425,
      "pointsAgainst": 380,
      "points": 18,
      "isGoztepe": true
    }
  ],
  "games": [
    {
      "date": "15.01.2026",
      "time": "19:00",
      "homeTeam": "Göztepe Spor Kulübü",
      "awayTeam": "Karşıyaka GSK",
      "homeScore": 3,
      "awayScore": 1,
      "setResults": "(25-20, 23-25, 25-18, 25-22)",
      "venue": "Göztepe Spor Salonu",
      "isPlayed": true,
      "isGoztepe": true
    }
  ],
  "goztepeGames": [
    {
      "date": "15.01.2026",
      "time": "19:00",
      "homeTeam": "Göztepe Spor Kulübü",
      "awayTeam": "Karşıyaka GSK",
      "homeScore": 3,
      "awayScore": 1,
      "setResults": "(25-20, 23-25, 25-18, 25-22)",
      "venue": "Göztepe Spor Salonu",
      "isPlayed": true,
      "isGoztepe": true
    }
  ]
}
```

## 🎯 Cache System

The application implements a sophisticated caching system to minimize load on the federation website and improve performance:

### Cache Configuration

- **Fixture Cache Duration**: 24 hours (configurable via `AppConstants.FixtureCacheDuration`)
- **Standings Cache Duration**: 48 hours (configurable via `AppConstants.StandingsCacheDuration`)
- **Cache Type**: In-memory caching using `Microsoft.Extensions.Caching.Memory`

### Cache Keys Structure

- **Fixtures**: `fixture:{seasonId}:{leagues}`
- **Competitions**: `competitions:{seasonId}:{category}:{leagueCode}`
- **Standings**: `standings:{seasonId}:{competitionName}`

### Cache Management

- Automatic expiration based on configured durations
- Manual cache clearing via frontend interface
- Force refresh capability with `forceRefresh=true` parameter
- Cache statistics display (hit/miss rates, key counts)

### Cache Benefits

- **Reduced Load**: Prevents excessive requests to federation website
- **Better Performance**: Faster response times for frequently accessed data
- **Reliability**: Continues working even if federation site is temporarily unavailable
- **Bandwidth Saving**: Reduces data transfer costs and server load

## 🎨 Frontend Technology Stack

### Core Framework

- **Vue.js 3**: Modern composition API with `<script setup>` syntax
- **TypeScript**: Full type safety across all components and stores
- **Vite**: Fast development server and build tool
- **Vue Router 4**: Client-side routing between fixture and standings views

### State Management

- **Pinia**: Reactive state management with separate stores for fixtures and standings
- **Computed Properties**: Intelligent filtering and data transformation
- **Reactive Updates**: Real-time UI updates when data changes

### UI Framework

- **Tailwind CSS 4**: Utility-first CSS framework with custom Göztepe color scheme
- **Custom Colors**: Brand-specific red (`goztepe-red`) and dark (`goztepe-dark`) colors
- **Responsive Design**: Mobile-first approach with responsive breakpoints
- **Typography**: Optimized font sizes and spacing for volleyball data display

### Components Architecture

```
src/
├── components/
│   ├── GameCard.vue     # Shared game display component
│   ├── LeagueFilter.vue        # League selection dropdown
│   ├── GameList.vue           # List container for game cards
│   └── StandingsTable.vue     # Standings table with team logos
├── stores/
│   ├── fixture.ts             # Fixture data and filtering logic
│   └── standings.ts          # Standings and competition management
├── views/
│   ├── FixtureView.vue       # Main fixtures page
│   └── StandingsView.vue     # Standings and results page
└── types/
    └── index.ts              # TypeScript type definitions
```

### Key Features

- **Turkish Localization**: Date formatting with Turkish month names (e.g., "28 Ocak 2026")
- **Göztepe Highlighting**: Red border and bold text for Göztepe team games
- **Unified Game Cards**: Same component handles both fixtures and match results
- **Smart Filtering**: League, division, and competition filters with persistent state
- **Loading States**: Proper loading indicators during data fetching
- **Error Handling**: User-friendly error messages for failed requests

## 🏗️ Project Structure

```
goztepe-volleyball-scraper/
├── backend/
│   └── VolleyballScraper.Api/
│       ├── Controllers/          # API endpoint controllers
│       │   ├── FixtureController.cs
│       │   └── StandingsController.cs
│       ├── Models/              # Request/response data models
│       │   ├── Fixture/
│       │   └── Standings/
│       ├── Services/            # Business logic and scraping services
│       │   ├── FixtureScraperService.cs
│       │   ├── StandingsScraperService.cs
│       │   └── *CacheService.cs
│       ├── Constants/           # Application configuration
│       │   └── AppConstants.cs
│       └── Program.cs          # Application startup and DI configuration
└── frontend/
    ├── src/
    │   ├── components/         # Reusable Vue components
    │   ├── views/             # Page-level components
    │   ├── stores/            # Pinia state management
    │   ├── types/             # TypeScript type definitions
    │   └── router/            # Vue Router configuration
    ├── package.json           # Frontend dependencies
    └── vite.config.ts         # Vite build configuration
```

## 🔧 Configuration

### Backend Configuration (`appsettings.json`)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "CorsOrigins": ["http://localhost:5173", "http://localhost:3000"]
}
```

### Application Constants

Located in `AppConstants.cs`:

- `OrganisationId`: Göztepe's ID on federation site ("662")
- `Gender`: "B" for women's volleyball (Bayan)
- `ProvinceId`: "35" for İzmir province
- `ClubName`: "Göztepe" for team name filtering
- `SeasonId`: Default season "2025-2026"
- Cache durations for fixtures (24h) and standings (48h)

## 🎯 Supported Leagues

The application supports all İzmir provincial volleyball leagues:

### Women's Categories

- **GKSL**: Genç Kızlar Süper Ligi (Young Girls Super League)
- **YKSL**: Yıldızlar Kızlar Süper Ligi (Stars Girls Super League)
- **KKSL**: Küçükler Kızlar Süper Ligi (Juniors Girls Super League)
- **MDKSL**: Minikler-Değirmencik Kızlar Süper Ligi (Mini-Değirmencik Girls Super League)
- **MDK1L**: Minikler-Değirmencik Kızlar 1. Ligi (Mini-Değirmencik Girls 1st League)
- **MDK2L**: Minikler-Değirmencik Kızlar 2. Ligi (Mini-Değirmencik Girls 2nd League)
- **KK1L**: Küçükler Kızlar 1. Ligi (Juniors Girls 1st League)
- **KK2L**: Küçükler Kızlar 2. Ligi (Juniors Girls 2nd League)
- **YK2L**: Yıldızlar Kızlar 2. Ligi (Stars Girls 2nd League)

### Match Types

- **KL**: Klasman Ligi (Classification League)
- **CF**: Çeyrek Final (Quarter Final)
- **YF**: Yarı Final (Semi Final)
- **FI**: Final (Final)

## 🚀 Deployment

### Backend Deployment

```bash
cd backend
dotnet publish -c Release -o ./publish
# Deploy to your hosting provider (Azure, AWS, etc.)
```

### Frontend Deployment

```bash
cd frontend
npm run build
# Serve the dist/ folder with your web server
```

### Environment Variables

Set the following environment variables for production:

- `ASPNETCORE_ENVIRONMENT=Production`
- `ASPNETCORE_URLS=http://localhost:5000`
- Configure CORS origins for your domain

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines

- Follow TypeScript best practices in frontend code
- Use proper C# conventions in backend code
- Add appropriate error handling for scraping operations
- Test both cached and fresh data scenarios
- Ensure responsive design across all screen sizes

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🏐 About Göztepe Sports Club

[Göztepe Spor Kulübü](https://www.goztepe.org/) is a multi-sport club based in İzmir, Turkey, founded in 1925. The club competes in various sports including football, basketball, and volleyball. This application specifically tracks the women's volleyball teams across all age categories and league levels.

## 🛠️ Maintenance

### Regular Tasks

- Monitor cache performance and adjust durations as needed
- Update league codes if federation changes structure
- Test scraping functionality when federation site updates
- Update TypeScript and Vue.js dependencies regularly
- Backup cached data for important competitions

### Troubleshooting

- Check federation website accessibility if scraping fails
- Verify cache keys match expected format
- Test API endpoints with different parameter combinations
- Validate frontend-backend integration after updates
- Monitor application logs for scraping errors

---

**Built with ❤️ for Göztepe Volleyball** 🏐
