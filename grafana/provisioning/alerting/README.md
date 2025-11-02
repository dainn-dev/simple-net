# Grafana Alerting Configuration

This directory contains the provisioning configuration for Grafana alerts and notification channels.

## Files

- **rules.yml** - Alert rule definitions for the DainnUserManagement application
- **notifiers.yml** - Notification channel configurations (Email, Slack, PagerDuty)

## Setup Instructions

### 1. Alert Rules (rules.yml)

The alert rules are automatically provisioned when Grafana starts. The rules include:

- **High Failed Logins**: Triggers when failed login attempts exceed 5 per second over 5 minutes
- **Database Slow Queries**: Triggers when average database command duration exceeds 1 second over 5 minutes
- **HTTP 401 Spike**: Triggers when HTTP 401 responses exceed 10 per second over 5 minutes

### 2. Notification Channels (notifiers.yml)

Before using the notification channels, update the configuration:

#### Email
- Update the `addresses` field with your email addresses
- No additional configuration needed if SMTP is configured in Grafana

#### Slack
1. Create a Slack webhook URL:
   - Go to https://api.slack.com/apps
   - Create a new app or select an existing one
   - Navigate to "Incoming Webhooks" and create a webhook
   - Copy the webhook URL
2. Update the `url` field in `notifiers.yml` with your Slack webhook URL
3. Update the `channel` field with your desired Slack channel (e.g., `#alerts`)

#### PagerDuty
1. Create a PagerDuty integration:
   - Log in to PagerDuty
   - Navigate to Configuration > Services
   - Create or select a service
   - Add a "Grafana" integration
   - Copy the Integration Key
2. Update the `integrationKey` field in `notifiers.yml` with your PagerDuty Integration Key

### 3. Grafana Configuration

Add the following to your `grafana.ini` or environment variables:

```ini
[paths]
provisioning = /path/to/grafana/provisioning

[alerting]
enabled = true
```

Or via environment variables:
```bash
GF_PATHS_PROVISIONING=/path/to/grafana/provisioning
GF_ALERTING_ENABLED=true
```

### 4. Prometheus Data Source

Ensure Prometheus is configured as a data source in Grafana with UID `prometheus`. If using a different UID, update the `datasourceUid` field in `rules.yml`.

## Alert Metrics

The alerts expect the following metrics to be available in Prometheus:

- `auth_login_total{result="failed"}` - Counter for failed login attempts
- `efcore_command_duration_seconds` - Histogram of Entity Framework Core command durations
- `http_server_duration_seconds_bucket{status_code="401"}` - HTTP server duration buckets for 401 status codes

## Customization

You can customize the alert thresholds by modifying the expressions in `rules.yml`:

- **High Failed Logins**: Change `> 5` to adjust the threshold
- **DB Slow**: Change `> 1` to adjust the duration threshold (in seconds)
- **HTTP 401 Spike**: Change `> 10` to adjust the rate threshold

## Testing Alerts

To test alerts manually:
1. Navigate to Grafana Alerting UI
2. Select the alert rule
3. Click "Test rule" to verify the query
4. Use the "Test" button in notification channels to verify delivery

## Troubleshooting

- **Alerts not firing**: Check that Prometheus data source is correctly configured and metrics are available
- **Notifications not sending**: Verify notification channel credentials and Grafana SMTP/API configuration
- **Rules not loading**: Ensure the provisioning path is correct and Grafana has read permissions

