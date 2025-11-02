# Grafana Monitoring Configuration

This directory contains Grafana provisioning configuration files for dashboards, alerts, and data sources.

## Directory Structure

```
grafana/
├── provisioning/
│   ├── alerting/
│   │   ├── rules.yml          # Alert rule definitions
│   │   ├── notifiers.yml      # Notification channels
│   │   └── README.md          # Alerting setup guide
│   ├── dashboards/
│   │   └── (dashboard JSON files)
│   └── datasources/
│       └── prometheus.yml      # Prometheus data source configuration
└── README.md                   # This file
```

## Quick Start

1. **Configure Grafana**: Set the provisioning path in your `grafana.ini`:
   ```ini
   [paths]
   provisioning = /path/to/grafana/provisioning
   ```

2. **Set up Data Source**: Configure Prometheus data source (see `provisioning/datasources/`)

3. **Configure Alerts**: Update notification channels in `provisioning/alerting/notifiers.yml`

4. **Restart Grafana**: Reload Grafana to apply provisioning changes

## Components

### Alerting

See [provisioning/alerting/README.md](provisioning/alerting/README.md) for detailed alerting setup instructions.

Alert rules include:
- High Failed Logins
- Database Slow Queries
- HTTP 401 Unauthorized Spike

### Dashboards

Grafana dashboards can be provisioned by placing JSON files in `provisioning/dashboards/`.

### Data Sources

Prometheus data source configuration is located in `provisioning/datasources/`.

## Environment Variables

You can also configure Grafana via environment variables:

```bash
GF_PATHS_PROVISIONING=/path/to/grafana/provisioning
GF_ALERTING_ENABLED=true
GF_DATABASE_TYPE=sqlite3
GF_DATABASE_PATH=/var/lib/grafana/grafana.db
```

## Docker Usage

If running Grafana in Docker, mount this directory:

```bash
docker run -d \
  -p 3000:3000 \
  -v $(pwd)/grafana/provisioning:/etc/grafana/provisioning \
  grafana/grafana:latest
```

## References

- [Grafana Provisioning Documentation](https://grafana.com/docs/grafana/latest/administration/provisioning/)
- [Grafana Alerting Guide](https://grafana.com/docs/grafana/latest/alerting/)
- [Prometheus Query Language](https://prometheus.io/docs/prometheus/latest/querying/basics/)

