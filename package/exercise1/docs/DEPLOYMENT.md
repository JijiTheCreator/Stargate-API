# Deployment Runbook

> **Target**: Docker Compose on a single host. Database is SQLite (file-based, no external DB server).

---

## 1. Production Build

### API

```bash
cd src/api
dotnet publish -c Release -o ./publish
```

Output: `./publish/` contains the self-contained .NET 8 application.

### UI

```bash
cd src/ui
npm ci
npm run build -- --configuration production
```

Output: `./dist/stargate-ui/browser/` contains the optimized Angular bundle.

---

## 2. Docker Image Build

### Build both images

```bash
cd package/exercise1
docker-compose build
```

### Build individually

```bash
# API
docker build -t stargate-api:latest ./src/api

# UI
docker build -t stargate-ui:latest ./src/ui
```

### Push to registry (if applicable)

```bash
docker tag stargate-api:latest <registry>/stargate-api:<version>
docker push <registry>/stargate-api:<version>

docker tag stargate-ui:latest <registry>/stargate-ui:<version>
docker push <registry>/stargate-ui:<version>
```

---

## 3. Environment Variables

| Variable | Default | Description |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` | .NET environment (`Development` enables detailed errors) |
| `ASPNETCORE_URLS` | `http://+:5204` | API listen address (set in Dockerfile) |
| `ConnectionStrings__StarbaseApiDatabase` | `Data Source=/data/starbase.db` | SQLite connection string |

Override via `.env` file in the project root or inline with `docker-compose`:

```bash
ASPNETCORE_ENVIRONMENT=Production docker-compose up --build
```

---

## 4. Deploy

### Start services

```bash
docker-compose up -d --build
```

The `-d` flag runs containers in detached mode.

### What happens on startup

1. **API container**: `entrypoint.sh` runs `dotnet StargateAPI.dll`
2. **Program.cs**: Applies EF Core migrations (`Database.Migrate()`) — creates SQLite DB if missing
3. **API**: Starts listening on port 5204
4. **UI container**: Nginx starts, serves Angular app on port 80 (mapped to 4200)
5. **Docker healthcheck**: Polls `http://localhost:5204/health` every 10s
6. **UI depends_on**: UI container waits until API healthcheck passes

---

## 5. Health Check Verification

### API health endpoint

```bash
curl -f http://localhost:5204/health
```

**Expected response**:

```json
{ "status": "healthy" }
```

### Docker healthcheck status

```bash
docker inspect --format='{{.State.Health.Status}}' stargate-api
```

**Expected**: `healthy`

### Verify all services

```bash
docker-compose ps
```

**Expected**:

```
NAME            STATUS                  PORTS
stargate-api    Up X minutes (healthy)  0.0.0.0:5204->5204/tcp
stargate-ui     Up X minutes            0.0.0.0:4200->80/tcp
```

---

## 6. Logs

### View live logs

```bash
docker-compose logs -f
```

### View API logs only

```bash
docker-compose logs -f api
```

### Query application logs (stored in DB)

```
GET http://localhost:5204/Logs
```

---

## 7. Rollback Procedure

### Quick rollback (revert to previous image)

```bash
docker-compose down
docker tag stargate-api:<previous-version> stargate-api:latest
docker tag stargate-ui:<previous-version> stargate-ui:latest
docker-compose up -d
```

### Full clean restart

```bash
docker-compose down -v          # Removes Docker volumes (networks etc)
# Note: Bind mounts like ./data are NOT removed by -v
rm -rf ./data                   # Manually delete to full reset DB
docker-compose up --build -d    # Rebuilds from source
```

> [!CAUTION]
> Deleting the `./data` folder manually or running `docker-compose down -v` followed by an `rm` will lose all astronaut career history.

### 7.3 Database Persistence & Backup

The system uses a **bind mount** to persist the SQLite database on your host machine.

- **Storage Location**: `./package/exercise1/data/starbase.db`
- **Persistence**: Even if you stop the containers or run `docker-compose down`, the data remains in this folder.

**To Backup**:
Simply copy the `./data/starbase.db` file from your host machine to a safe location.

---

## 8. Database Access (Direct)

Since the database is a physical file mapped to your host, you can access it directly without entering the container.

### VS Code (Recommended)
1. Install the **"SQLite Viewer"** extension.
2. Open `package/exercise1/data/starbase.db` in VS Code.

### External Tools (DBeaver, DB Browser)
1. Open your SQLite-compatible tool.
2. "Open Database" and select the file at:
   `C:\...\tech_exercise\package\exercise1\data\starbase.db`

---

## 8. Troubleshooting

| Symptom | Cause | Fix |
|---|---|---|
| API exits with SQLite Error 14 | `/data` directory permissions | Rebuild image — Dockerfile creates `/data` with correct ownership |
| UI shows CORS errors | API CORS policy missing origin | Check `Program.cs` CORS configuration |
| Health check fails | API didn't start | Check `docker-compose logs api` for startup errors |
| Nginx permission denied | Non-root user can't write nginx dirs | Rebuild UI image — Dockerfile chowns nginx directories |
| Port already in use | Another service on 5204 or 4200 | Stop conflicting service or change ports in `docker-compose.yml` |
