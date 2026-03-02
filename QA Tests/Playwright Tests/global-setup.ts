/**
 * @fileoverview Playwright global setup — runs once before all test suites.
 *
 * Responsibilities:
 *   1. Refresh the gcloud IAM access token for Cloud SQL proxy authentication.
 *   2. Verify the Cloud SQL proxy is listening on port 5432.
 *   3. Verify the backend API is reachable.
 *   4. Verify the frontend is reachable.
 *
 * If any prerequisite is missing, this script logs actionable instructions
 * but does NOT block test execution (individual tests will fail with clear
 * messages instead of a cryptic global setup error).
 */

import { execSync } from 'child_process';
import * as fs from 'fs';
import * as path from 'path';
import * as net from 'net';
import * as dotenv from 'dotenv';

dotenv.config({ path: path.resolve(__dirname, '.env') });

const API_BASE_URL = process.env.API_BASE_URL || 'http://localhost:5159';
const BASE_URL = process.env.BASE_URL || 'http://localhost:4200';

async function globalSetup(): Promise<void> {
  console.log('\n=== Playwright Global Setup ===\n');

  // 1. Refresh gcloud IAM token
  refreshGcloudToken();

  // 2. Verify prerequisites
  const proxyOk = await checkPort(5432);
  const backendOk = await checkHttp(API_BASE_URL);
  const frontendOk = await checkHttp(BASE_URL);

  // Summary
  console.log('\n--- Prerequisite Check ---');
  logStatus(proxyOk,   'Cloud SQL Proxy (port 5432)');
  logStatus(backendOk, `Backend API (${API_BASE_URL})`);
  logStatus(frontendOk,`Frontend    (${BASE_URL})`);

  if (!proxyOk || !backendOk || !frontendOk) {
    console.log('\n⚠  Some services are not running. Real-API tests will fail.');
    console.log('   Start all services with:  .\\start-servers.ps1  (from QA Tests/)');
    console.log('   Or start them manually:');
    if (!proxyOk)    console.log('     - Cloud SQL proxy: cloud_sql_proxy --private-ip <instance>');
    if (!backendOk)  console.log('     - Backend:  dotnet run --project UNOPS.PAO.Server  (or use Visual Studio)');
    if (!frontendOk) console.log('     - Frontend: cd UNOPS.PAO.ClientApp && ng serve --port 4200');
    console.log('');
  } else {
    console.log('\n✓  All services running — real-API tests are ready.\n');
  }
}

function refreshGcloudToken(): void {
  const tokenFile = path.join(
    process.env.TEMP || process.env.TMPDIR || '/tmp',
    'gcloud_token.txt',
  );

  try {
    const token = execSync('gcloud auth print-access-token', {
      encoding: 'utf-8',
      timeout: 15_000,
      stdio: ['pipe', 'pipe', 'pipe'],
    }).trim();

    if (token.length > 50) {
      fs.writeFileSync(tokenFile, token, 'utf-8');
      console.log(`[gcloud] Token refreshed (${token.length} chars)`);
    } else {
      console.warn('[gcloud] Token too short — skipping');
    }
  } catch {
    console.warn('[gcloud] Token refresh failed (cached token may still work)');
  }
}

function checkPort(port: number): Promise<boolean> {
  return new Promise((resolve) => {
    const socket = new net.Socket();
    socket.setTimeout(2_000);
    socket.on('connect', () => { socket.destroy(); resolve(true); });
    socket.on('timeout', () => { socket.destroy(); resolve(false); });
    socket.on('error',   () => { socket.destroy(); resolve(false); });
    socket.connect(port, '127.0.0.1');
  });
}

async function checkHttp(url: string): Promise<boolean> {
  try {
    await fetch(url, { signal: AbortSignal.timeout(5_000) });
    return true;
  } catch {
    return false;
  }
}

function logStatus(ok: boolean, label: string): void {
  console.log(`  ${ok ? '✓' : '✗'}  ${label}`);
}

export default globalSetup;
