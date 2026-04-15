#!/usr/bin/env bash
# Opportunity+ — stop all local dev services

RED='\033[0;31m'; GREEN='\033[0;32m'; NC='\033[0m'
ok() { echo -e "${GREEN}[OK]${NC} $*"; }
no() { echo -e "     $* (not running)"; }

echo "Stopping Opportunity+ local services..."

# .NET backend
pkill -f "dotnet.*UNOPS.PAO.Server" 2>/dev/null && ok "Backend stopped" || no "Backend"

# Angular dev server (ng serve or webpack-dev-server)
pkill -f "ng serve" 2>/dev/null || pkill -f "webpack.*44426" 2>/dev/null
# Also kill node processes on the frontend port
lsof -iTCP:44426 -sTCP:LISTEN -t 2>/dev/null | xargs kill -9 2>/dev/null && ok "Frontend stopped" || no "Frontend"

# SSH tunnel
pkill -f "ssh.*${1:-6364}.*5432" 2>/dev/null && ok "SSH tunnel stopped" || no "Tunnel"

echo -e "${GREEN}Done.${NC}"
