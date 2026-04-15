#!/usr/bin/env bash
set -euo pipefail

# ═══════════════════════════════════════════════════════════════════════
#  Opportunity+ — One-command local dev environment
#  Usage: ./start-local.sh
#  Stop:  Ctrl+C  (or ./stop-local.sh from another terminal)
# ═══════════════════════════════════════════════════════════════════════

# ─── Configuration ────────────────────────────────────────────────────
GCP_PROJECT="unops-opportunityplus-dev"
BASTION_INSTANCE="unopsgc567901-sql-proxy"
BASTION_ZONE="europe-west4-b"
SQL_PRIVATE_IP="10.129.0.16"
SQL_PORT=5432
LOCAL_TUNNEL_PORT=6364
BACKEND_PORT=7123
FRONTEND_PORT=44426
BACKEND_DIR="UNOPS.PAO.Server"
FRONTEND_DIR="UNOPS.PAO.ClientApp"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# ─── Colors / helpers ─────────────────────────────────────────────────
RED='\033[0;31m'; GREEN='\033[0;32m'; YELLOW='\033[1;33m'; BLUE='\033[0;34m'; NC='\033[0m'
info()  { echo -e "${BLUE}[INFO]${NC}  $*"; }
ok()    { echo -e "${GREEN}[ OK ]${NC}  $*"; }
warn()  { echo -e "${YELLOW}[WARN]${NC}  $*"; }
die()   { echo -e "${RED}[FAIL]${NC}  $*"; exit 1; }

port_in_use() { lsof -iTCP:"$1" -sTCP:LISTEN >/dev/null 2>&1; }

kill_port() {
  local port=$1
  local pids
  pids=$(lsof -iTCP:"$port" -sTCP:LISTEN -t 2>/dev/null || true)
  if [ -n "$pids" ]; then
    echo "$pids" | xargs kill -9 2>/dev/null || true
    sleep 1
  fi
}

wait_for_port() {
  local port=$1 label=$2 timeout=${3:-90} elapsed=0
  while ! port_in_use "$port"; do
    sleep 2; elapsed=$((elapsed + 2))
    [ "$elapsed" -ge "$timeout" ] && return 1
  done
  return 0
}

wait_for_https() {
  local url=$1 label=$2 timeout=${3:-90} elapsed=0
  while true; do
    local code
    code=$(curl -sk -o /dev/null -w "%{http_code}" "$url" 2>/dev/null || echo "000")
    if [ "$code" != "000" ]; then
      return 0
    fi
    sleep 2; elapsed=$((elapsed + 2))
    [ "$elapsed" -ge "$timeout" ] && return 1
  done
}

# ─── Cleanup on exit ─────────────────────────────────────────────────
PIDS_TO_KILL=()
cleanup() {
  echo ""
  warn "Shutting down..."
  for pid in "${PIDS_TO_KILL[@]}"; do
    kill "$pid" 2>/dev/null || true
  done
  # Also kill any child processes we spawned
  pkill -P $$ 2>/dev/null || true
  exit 0
}
trap cleanup INT TERM EXIT

# ═══════════════════════════════════════════════════════════════════════
echo ""
echo -e "${GREEN}  Opportunity+ Local Dev${NC}"
echo ""

# ─── Pre-flight ──────────────────────────────────────────────────────
command -v gcloud &>/dev/null || die "gcloud CLI not found"
command -v dotnet &>/dev/null || die "dotnet CLI not found"
command -v node   &>/dev/null || die "node not found"

info "Refreshing gcloud credentials..."
if ! gcloud auth print-access-token >/dev/null 2>&1; then
  warn "Token expired — running gcloud auth login..."
  gcloud auth login --update-adc
fi
ok "gcloud auth valid"

# ─── 1. SSH Tunnel ───────────────────────────────────────────────────
echo ""
info "Step 1/3 — SSH tunnel to Cloud SQL"

# Kill stale tunnel if the port is occupied but the tunnel is dead
if port_in_use "$LOCAL_TUNNEL_PORT"; then
  # Verify the tunnel actually works by trying a TCP connection to PostgreSQL
  if nc -z -w 2 127.0.0.1 "$LOCAL_TUNNEL_PORT" 2>/dev/null; then
    ok "Tunnel alive on port $LOCAL_TUNNEL_PORT"
  else
    warn "Stale tunnel — restarting..."
    kill_port "$LOCAL_TUNNEL_PORT"
  fi
fi

if ! port_in_use "$LOCAL_TUNNEL_PORT"; then
  gcloud compute ssh "$BASTION_INSTANCE" \
    --project="$GCP_PROJECT" \
    --zone="$BASTION_ZONE" \
    --tunnel-through-iap \
    -- -N -L "${LOCAL_TUNNEL_PORT}:${SQL_PRIVATE_IP}:${SQL_PORT}" &
  PIDS_TO_KILL+=($!)

  wait_for_port "$LOCAL_TUNNEL_PORT" "tunnel" 45 || die "SSH tunnel failed to start"
  ok "Tunnel started on port $LOCAL_TUNNEL_PORT"
fi

# ─── 2. .NET Backend ────────────────────────────────────────────────
echo ""
info "Step 2/3 — .NET backend (HTTPS on $BACKEND_PORT)"

# Always kill and restart the backend to pick up code changes
if port_in_use "$BACKEND_PORT"; then
  info "Stopping old backend..."
  kill_port "$BACKEND_PORT"
fi

cd "$SCRIPT_DIR/$BACKEND_DIR"
ASPNETCORE_ENVIRONMENT=Local dotnet run --no-launch-profile &
PIDS_TO_KILL+=($!)
cd "$SCRIPT_DIR"

info "Waiting for backend HTTPS..."
if wait_for_https "https://localhost:${BACKEND_PORT}/api/configuration" "backend" 120; then
  ok "Backend ready — https://localhost:${BACKEND_PORT}"
else
  die "Backend did not respond on HTTPS within 120s"
fi

# ─── 3. Angular Frontend ────────────────────────────────────────────
echo ""
info "Step 3/3 — Angular frontend"

if port_in_use "$FRONTEND_PORT"; then
  ok "Frontend already running — https://localhost:${FRONTEND_PORT}"
else
  cd "$SCRIPT_DIR/$FRONTEND_DIR"
  npm start &
  PIDS_TO_KILL+=($!)
  cd "$SCRIPT_DIR"

  info "Waiting for frontend..."
  wait_for_port "$FRONTEND_PORT" "frontend" 180 || die "Angular did not start within 180s"
  ok "Frontend ready — https://localhost:${FRONTEND_PORT}"
fi

# ─── Done ────────────────────────────────────────────────────────────
echo ""
echo -e "${GREEN}  ✓ Ready — open https://localhost:${FRONTEND_PORT}/${NC}"
echo -e "${GREEN}    Auth is automatic (${DEV_USER_EMAIL:-emamirelar@unops.org})${NC}"
echo -e "${GREEN}    Ctrl+C to stop${NC}"
echo ""

# Keep alive
wait
