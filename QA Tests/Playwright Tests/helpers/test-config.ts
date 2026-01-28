/**
 * @fileoverview Test Configuration Helper
 * Centralizes test configuration and environment variables
 */

import * as dotenv from 'dotenv';
import * as path from 'path';

// Load environment variables from .env file
dotenv.config({ path: path.resolve(__dirname, '../.env') });

/**
 * Test configuration object with defaults
 */
export const testConfig = {
  // Authentication
  auth: {
    email: process.env.TEST_USER_EMAIL || 'testuser@unops.org',
    password: process.env.TEST_USER_PASSWORD || 'TestPassword123!',
  },
  
  // URLs
  urls: {
    base: process.env.BASE_URL || 'http://localhost:4200',
    api: process.env.API_BASE_URL || 'http://localhost:5000',
  },
  
  // Timeouts
  timeouts: {
    default: parseInt(process.env.DEFAULT_TIMEOUT || '10000', 10),
    long: parseInt(process.env.LONG_TIMEOUT || '30000', 10),
    short: parseInt(process.env.SHORT_TIMEOUT || '5000', 10),
    navigation: parseInt(process.env.NAVIGATION_TIMEOUT || '15000', 10),
  },
  
  // Test Data
  testData: {
    partner: {
      name: process.env.TEST_PARTNER_NAME || 'Test Partner Organization',
    },
    contact: {
      name: process.env.TEST_CONTACT_NAME || 'Test Contact Name',
    },
    opportunity: {
      name: process.env.TEST_OPPORTUNITY_NAME || 'Test Opportunity',
    },
  },
  
  // Debug Settings
  debug: {
    enabled: process.env.DEBUG === 'true',
    traceOnFailure: process.env.TRACE_ON_FAILURE !== 'false',
    screenshotOnFailure: process.env.SCREENSHOT_ON_FAILURE !== 'false',
    videoOnFailure: process.env.VIDEO_ON_FAILURE !== 'false',
  },
};

/**
 * Get test user credentials
 */
export function getTestCredentials() {
  return {
    email: testConfig.auth.email,
    password: testConfig.auth.password,
  };
}

/**
 * Get base URL
 */
export function getBaseUrl(): string {
  return testConfig.urls.base;
}

/**
 * Get API base URL
 */
export function getApiBaseUrl(): string {
  return testConfig.urls.api;
}

/**
 * Get timeout value by type
 */
export function getTimeout(type: 'default' | 'long' | 'short' | 'navigation' = 'default'): number {
  return testConfig.timeouts[type];
}
