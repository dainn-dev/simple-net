# Docker Observability Stack

This directory contains the Docker Compose configuration for running the DainnUserManagement application with a complete observability stack.

## Services

The stack includes:

- **app**: DainnUserManagement application
- **otel-collector**: OpenTelemetry Collector for traces, metrics, and logs
- **prometheus**: Metrics collection and storage
- **grafana**: Visualization and dashboards
- **loki**: Log aggregation
- **promtail**: Log shipper
- **jaeger**: Distributed tracing UI
- **postgres**: PostgreSQL database (optional)

## Quick Start

### 1. Prerequisites

- Docker and Docker Compose installed
- Ports available: 3000, 5000, 5001, 5432, 9090, 3100, 4317, 4318, 8888, 16686

### 2. Start the Stack

```bash
docker-compose up -d
```

This will:
- Build the application Docker image
- Start all observability services
- Configure automatic provisioning

### 3. Verify Services

#### Application
- **API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger
- **Metrics Endpoint**: http://localhost:5000/metrics
- **Health Checks**: 
  - http://localhost:5000/health/live
  - http://localhost:5000/health/ready

#### Prometheus
- **UI**: http://localhost:9090
- **Metrics**: http://localhost:9090/metrics
- **Targets**: http://localhost:9090/targets

#### Grafana
- **UI**: http://localhost:3000
- **Default Credentials**: 
  - Username: `admin`
  - Password: `admin`

#### Jaeger
- **UI**: http://localhost:16686

#### Loki
- **API**: http://localhost:3100

### 4. Verify Observability

#### Traces in Jaeger
1. Open http://localhost:16686
2. Select service: `DainnUserManagement`
3. Click "Find Traces"
4. You should see traces from the application (after making some API calls)

#### Metrics in Prometheus
1. Open http://localhost:9090
2. Navigate to "Status" > "Targets"
3. Verify `dainn-user-management` target is UP
4. Query metrics:
   - `rate(auth_login_total[5m])`
   - `http_server_request_duration_seconds`
   - `efcore_command_duration_seconds`

#### Logs in Grafana Loki
1. Open Grafana at http://localhost:3000
2. Go to "Explore"
3. Select "Loki" data source
4. Query: `{job="varlogs"}` or `{service_name="DainnUserManagement"}`

#### Dashboard Auto-Import
1. Open Grafana at http://localhost:3000
2. Navigate to "Dashboards"
3. The "DainnUserManagement - System Metrics" dashboard should be automatically available

## Configuration Files

### docker-compose.yml
Main orchestration file for all services.

### otel-config.yaml
OpenTelemetry Collector configuration:
- Receives OTLP data from the application (gRPC on 4317, HTTP on 4318)
- Exports traces to Jaeger
- Exports metrics to Prometheus
- Exports logs to Loki

### prometheus.yml
Prometheus scrape configuration:
- Scrapes application metrics from `/metrics` endpoint
- Scrapes OTEL Collector metrics

### promtail-config.yml
Log shipper configuration for sending logs to Loki.

## Data Persistence

All data is persisted in Docker volumes:
- `prometheus-data`: Prometheus metrics storage (30 day retention)
- `grafana-data`: Grafana dashboards and settings
- `loki-data`: Loki log storage
- `postgres-data`: PostgreSQL database (if using)
- `app-data`: Application data (SQLite database)

## Environment Variables

Key environment variables for the application (can be overridden in docker-compose.yml):
- `UserManagement__Provider`: Database provider (sqlite, postgresql, etc.)
- `UserManagement__ConnectionString`: Database connection string
- `OpenTelemetry__OtlpTracesEndpoint`: OTEL Collector endpoint for traces
- `Serilog__OtlpLogsEndpoint`: OTEL Collector endpoint for logs

## Troubleshooting

### Services Not Starting
```bash
# Check logs
docker-compose logs app
docker-compose logs otel-collector
docker-compose logs prometheus

# Restart a specific service
docker-compose restart app
```

### No Metrics in Prometheus
1. Verify application is running: `curl http://localhost:5000/health/live`
2. Check metrics endpoint: `curl http://localhost:5000/metrics`
3. Verify Prometheus targets: http://localhost:9090/targets
4. Check OTEL Collector logs: `docker-compose logs otel-collector`

### No Traces in Jaeger
1. Verify OTEL Collector is receiving data: `docker-compose logs otel-collector`
2. Check application OTLP configuration
3. Verify Jaeger is accessible: http://localhost:16686
4. Make some API calls to generate traces

### Dashboard Not Appearing
1. Check Grafana logs: `docker-compose logs grafana`
2. Verify dashboard file exists: `grafana/provisioning/dashboards/dainn-user-management.json`
3. Check Grafana provisioning path in environment variables
4. Refresh Grafana dashboards page

### Application Not Building
1. Verify Dockerfile path is correct
2. Check build context in docker-compose.yml
3. Review Dockerfile for correct project references

## Stopping the Stack

```bash
# Stop all services
docker-compose down

# Stop and remove volumes (deletes all data)
docker-compose down -v
```

## Updating Configuration

After modifying configuration files:
```bash
# Restart affected services
docker-compose restart otel-collector
docker-compose restart prometheus
docker-compose restart grafana
```

## Building Custom Application Image

```bash
# Build only
docker-compose build app

# Build and start
docker-compose up -d --build app
```

## Testing the Stack

1. **Generate some traffic:**
   ```bash
   # Register a user
   curl -X POST http://localhost:5000/api/v1/auth/register \
     -H "Content-Type: application/json" \
     -d '{"email":"test@example.com","password":"Test@123!","fullName":"Test User"}'
   
   # Login
   curl -X POST http://localhost:5000/api/v1/auth/login \
     -H "Content-Type: application/json" \
     -d '{"email":"test@example.com","password":"Test@123!"}'
   ```

2. **Check traces in Jaeger:** http://localhost:16686
3. **Check metrics in Prometheus:** http://localhost:9090
4. **Check dashboard in Grafana:** http://localhost:3000
