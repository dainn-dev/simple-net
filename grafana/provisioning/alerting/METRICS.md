# Expected Metrics for Alerts

This document describes the Prometheus metrics that the alert rules expect to be available.

## Alert Metrics

### 1. High Failed Logins

**Alert Query:**
```promql
rate(auth_login_total{result="failed"}[5m]) > 5
```

**Expected Metric:**
- **Name:** `auth_login_total`
- **Type:** Counter
- **Labels:**
  - `result`: Should include `"failed"` value
- **Description:** Counter tracking total login attempts, with a label indicating success or failure

**Alternative Metric Names:**
- `auth.login.total` (if using dot notation)
- Ensure the metric name matches your application's instrumentation

### 2. Database Slow Queries

**Alert Query:**
```promql
avg(efcore_command_duration_seconds)[5m] > 1
```

**Expected Metric:**
- **Name:** `efcore_command_duration_seconds`
- **Type:** Histogram or Summary
- **Labels:** Optional (can include `command`, `operation`, etc.)
- **Description:** Duration of Entity Framework Core database commands in seconds

**Alternative Metric Names:**
- `efcore.command.duration` (if using dot notation)
- The metric should be provided by OpenTelemetry EntityFrameworkCore instrumentation

### 3. HTTP 401 Unauthorized Spike

**Alert Query:**
```promql
rate(http_server_duration_seconds_bucket{status_code="401"}[5m]) > 10
```

**Expected Metric:**
- **Name:** `http_server_duration_seconds_bucket` (for histogram) or `http_server_request_total` (for counter)
- **Type:** Histogram (buckets) or Counter
- **Labels:**
  - `status_code`: Should include `"401"` value
- **Description:** HTTP server request metrics, including status code labels

**Alternative Queries:**
```promql
# If using counter instead of histogram:
rate(http_server_request_total{status_code="401"}[5m]) > 10

# If using different label name:
rate(http_server_duration_seconds_bucket{status="401"}[5m]) > 10
```

## Metric Names

The exact metric names depend on your instrumentation:

- **OpenTelemetry** typically uses `_` (underscore) notation: `auth_login_total`, `efcore_command_duration_seconds`
- **Custom instrumentation** might use `.` (dot) notation: `auth.login.total`, `efcore.command.duration`

If your metrics use different names, update the queries in `rules.yml` accordingly.

## Verification

To verify your metrics are available:

1. Query Prometheus directly:
   ```bash
   curl 'http://localhost:9090/api/v1/query?query=auth_login_total'
   ```

2. Use Grafana Explore:
   - Navigate to Grafana > Explore
   - Select Prometheus data source
   - Enter the metric name
   - Verify the metric appears and has the expected labels

3. Check metric names in Prometheus:
   ```bash
   curl 'http://localhost:9090/api/v1/label/__name__/values'
   ```

## Custom Metrics

If you're emitting custom metrics, ensure they follow Prometheus naming conventions:

- Use `_` (underscore) for metric names
- Use `_total` suffix for counters
- Use `_seconds` suffix for durations
- Include appropriate labels for filtering

