#!/bin/sh
# Headless installer for nopCommerce.
#
# nopCommerce has no built-in unattended install, so this drives the regular
# /install endpoint: it fetches a fresh antiforgery token and POSTs the same
# form the wizard submits. Idempotent — exits 0 if the store is already
# installed. Intended to run as the one-shot nopcommerce_installer sidecar.
#
# Configuration (env vars):
#   NOP_AUTO_INSTALL       must be "true" or this script is a no-op
#   NOP_URL                base URL of the web app (default http://nopcommerce_web:80)
#   NOP_ADMIN_EMAIL        admin account email
#   NOP_ADMIN_PASSWORD     admin account password (required)
#   NOP_SAMPLE_DATA        "true" to install demo/sample data (default true)
#   NOP_CONNECTION_STRING  raw PostgreSQL connection string

set -eu

if [ "${NOP_AUTO_INSTALL:-false}" != "true" ]; then
    echo "[installer] NOP_AUTO_INSTALL != true -> skipping automated install."
    exit 0
fi

if [ -z "${NOP_ADMIN_PASSWORD:-}" ]; then
    echo "[installer] ERROR: NOP_ADMIN_PASSWORD is required when NOP_AUTO_INSTALL=true." >&2
    exit 1
fi

BASE="${NOP_URL:-http://nopcommerce_web:80}"
BASE="${BASE%/}"
ADMIN_EMAIL="${NOP_ADMIN_EMAIL:-admin@yourstore.com}"
SAMPLE_DATA="${NOP_SAMPLE_DATA:-true}"
CONN="${NOP_CONNECTION_STRING:-Host=nopcommerce_database;Port=5432;Database=nopcommerce;Username=nop;Password=${POSTGRES_PASSWORD:-};}"

JAR="$(mktemp)"
RESP="$(mktemp)"

echo "[installer] Waiting for web app at ${BASE} ..."
i=0
until curl -s -o /dev/null "${BASE}/install"; do
    i=$((i + 1))
    if [ "$i" -gt 120 ]; then
        echo "[installer] ERROR: web app did not respond in time." >&2
        exit 1
    fi
    sleep 3
done

CODE="$(curl -s -o /dev/null -w '%{http_code}' "${BASE}/install")"
if [ "$CODE" != "200" ]; then
    echo "[installer] /install returned HTTP ${CODE} -> already installed. Nothing to do."
    exit 0
fi

echo "[installer] Fetching antiforgery token ..."
HTML="$(curl -s -c "${JAR}" "${BASE}/install")"
TOKEN="$(printf '%s' "$HTML" | tr '>' '\n' \
    | grep '__RequestVerificationToken' \
    | sed -n 's/.*value="\([^"]*\)".*/\1/p' | head -n1)"
if [ -z "${TOKEN}" ]; then
    echo "[installer] ERROR: could not extract antiforgery token." >&2
    exit 1
fi

echo "[installer] Submitting installation (sample data=${SAMPLE_DATA}) ..."
STATUS="$(curl -s -b "${JAR}" -c "${JAR}" -o "${RESP}" -w '%{http_code}' -m 1800 \
    --data-urlencode "__RequestVerificationToken=${TOKEN}" \
    --data-urlencode "AdminEmail=${ADMIN_EMAIL}" \
    --data-urlencode "AdminPassword=${NOP_ADMIN_PASSWORD}" \
    --data-urlencode "ConfirmPassword=${NOP_ADMIN_PASSWORD}" \
    --data-urlencode "DataProvider=PostgreSQL" \
    --data-urlencode "ConnectionStringRaw=true" \
    --data-urlencode "ConnectionString=${CONN}" \
    --data-urlencode "CreateDatabaseIfNotExists=true" \
    --data-urlencode "InstallSampleData=${SAMPLE_DATA}" \
    --data-urlencode "SubscribeNewsletters=false" \
    "${BASE}/install")"

# On success the install view renders a JS redirect (window.location.replace).
# On failure it re-renders the form with a validation-summary error.
if grep -q "window.location.replace" "${RESP}"; then
    echo "[installer] Installation succeeded. Restarting app ..."
    curl -s -o /dev/null -m 30 "${BASE}/install/restartapplication" || true
    echo "[installer] Done."
    exit 0
fi

echo "[installer] ERROR: installation failed (HTTP ${STATUS}). Response excerpt:" >&2
sed -n '1,60p' "${RESP}" >&2
exit 1
