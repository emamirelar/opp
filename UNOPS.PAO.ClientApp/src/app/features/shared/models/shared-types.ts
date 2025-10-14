import { EntityPermissions } from '@core/services/auth';

/**
 * Shared permission type for entity models
 * Extracted from EntityPermissions interface for consistent usage across all entity models
 * 
 * Structure:
 * {
 *   canRead: boolean;
 *   canCreate: boolean;
 *   canUpdate: boolean;
 *   canDelete: boolean;
 * }
 */
export type EntityPermissionSet = EntityPermissions['permissions'];

/**
 * Full entity permissions including metadata
 * 
 * Structure:
 * {
 *   route?: string;
 *   entity: string;
 *   hasAccess: boolean;
 *   permissions: EntityPermissionSet;
 * }
 */
export type FullEntityPermissions = EntityPermissions; 
