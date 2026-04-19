# ambev-developer-evaluation

## Database migrations

Migrations live in `src/Ambev.DeveloperEvaluation.ORM/Migrations`. The WebApi is the startup project.

Add a migration:

```bash
dotnet ef migrations add <MigrationName> \
  --project src/Ambev.DeveloperEvaluation.ORM \
  --startup-project src/Ambev.DeveloperEvaluation.WebApi
```

Apply pending migrations to the configured `DefaultConnection`:

```bash
dotnet ef database update \
  --project src/Ambev.DeveloperEvaluation.ORM \
  --startup-project src/Ambev.DeveloperEvaluation.WebApi
```

Bring the database container up first:

```bash
docker-compose up -d ambev.developerevaluation.database
```

The PostgreSQL container from `docker-compose.yml` publishes on host port `55432` (the standard `5432` was already taken on the dev machine). Credentials:

| Key | Value |
|---|---|
| Host | `localhost` |
| Port | `55432` |
| Database | `developer_evaluation` |
| User | `developer` |
| Password | `ev@luAt10n` |

`src/Ambev.DeveloperEvaluation.WebApi/appsettings.json` already points to this connection:

```json
"DefaultConnection": "Host=localhost;Port=55432;Database=developer_evaluation;Username=developer;Password=ev@luAt10n"
```

If your host has port 55432 taken, change the host-side port in [docker-compose.yml](docker-compose.yml) and the matching `Port=` in `appsettings.json`.
