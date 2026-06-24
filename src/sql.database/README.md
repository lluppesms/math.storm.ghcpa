# MathStorm SQL Database

This folder contains the SQL Server DACPAC project for the MathStorm application.  
All objects are created in the **`mathstorm`** schema within a shared Azure SQL database.

## Schema: `mathstorm`

### Tables

| Table | Description |
|-------|-------------|
| `mathstorm.GameUser` | Player profiles with cumulative stats |
| `mathstorm.Game` | Completed game records (questions stored as JSON) |
| `mathstorm.LeaderboardEntry` | Top scores per difficulty (max 10 per difficulty, max 3 per user per difficulty) |

## Project Structure

```
src/sql.database/
├── MathStorm.Database.sqlproj   # SDK-style SQL project (Microsoft.Build.Sql)
├── MathStorm.Database.sln       # Solution file
├── Schemas/
│   └── mathstorm.sql            # Schema definition
└── MathStorm/
    ├── Pre.Deployment.sql        # Ensures schema exists before objects
    ├── Post.Deployment.sql       # Post-deployment confirmation message
    └── Tables/
        ├── GameUser.sql          # Player profile table
        ├── Game.sql              # Game record table (with JSON questions column)
        └── LeaderboardEntry.sql  # Leaderboard scores table
```

## Building the DACPAC

```bash
cd src/sql.database
dotnet build MathStorm.Database.sqlproj --configuration Release
```

The output DACPAC will be at:  
`src/sql.database/bin/Release/MathStorm.Database.dacpac`

## Deploying the DACPAC

### Using SqlPackage CLI

```bash
sqlpackage /Action:Publish \
  /SourceFile:MathStorm.Database.dacpac \
  /TargetConnectionString:"Server=tcp:<server>.database.windows.net,1433;Initial Catalog=<db>;Authentication=Active Directory Default;Encrypt=True;"
```

### Using Azure SQL Action (GitHub Actions)

The workflow `template-database-deploy.yml` handles deployment automatically.  
It requires the `SQL_CONNECTION_STRING` secret to be set in the GitHub environment.

## Local Development

When no `ConnectionStrings:SqlDb` is set in `appsettings.json`, the application
automatically falls back to an **in-memory mock** (`MockDataService`), so no database
is required for local development or running tests.

## Connection String Configuration

Set the `ConnectionStrings:SqlDb` key in `appsettings.json` (or as an App Service  
environment variable using `ConnectionStrings__SqlDb`):

```json
{
  "ConnectionStrings": {
    "SqlDb": "Server=tcp:<server>.database.windows.net,1433;Initial Catalog=MathStormDB-dev;Encrypt=True;Authentication=Active Directory Default;"
  }
}
```

For Managed Identity authentication (production), leave out `User Id`/`Password` and  
use `Authentication=Active Directory Default`.
