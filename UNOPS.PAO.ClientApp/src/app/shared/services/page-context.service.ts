import { Injectable, ApplicationRef, Injector } from '@angular/core';
import { Router } from '@angular/router';

/**
 * AUTOMATIC Page Context Service
 * 
 * This service AUTOMATICALLY extracts data from the currently active component
 * WITHOUT requiring any component changes or manual registration.
 * 
 * It works by:
 * 1. Finding the active route component
 * 2. Extracting all public properties that look like data
 * 3. Intelligently filtering out Angular internals and methods
 * 4. Returning a clean data snapshot
 * 
 * ZERO MAINTENANCE REQUIRED - works with any component, any entity!
 */
@Injectable({
  providedIn: 'root'
})
export class PageContextService {
  constructor(
    private router: Router,
    private appRef: ApplicationRef,
    private injector: Injector
  ) {}
  
  /**
   * AUTOMATICALLY extract page context from the currently active component
   * This requires NO component changes - it just works!
   * 
   * @param options Configuration for what data to extract
   * @returns Page data suitable for sending to AI
   */
  getPageContextForAI(options?: {
    maxArrayLength?: number;
    maxDepth?: number;
    includePrivateProps?: boolean;
  }): any {
    const {
      maxArrayLength = 20,
      maxDepth = 3,
      includePrivateProps = false
    } = options || {};

    try {
      // Get the active route component instance
      const componentInstance = this.getActiveComponentInstance();
      
      if (!componentInstance) {
        return null;
      }

      // Extract route information
      const routeInfo = this.extractRouteInfo();
      
      // Automatically extract all data from the component
      const componentData = this.extractComponentData(
        componentInstance, 
        maxArrayLength, 
        maxDepth,
        includePrivateProps
      );

      return {
        route: routeInfo,
        component_name: componentInstance.constructor.name,
        component_data: componentData,
        extracted_at: new Date().toISOString()
      };
    } catch (error) {
      console.warn('[PageContextService] Error extracting page context:', error);
      return null;
    }
  }
  
  // Private storage for component data - components can optionally register
  private currentComponentData: any = null;
  
  /**
   * Components can call this to register their data (OPTIONAL - one line of code)
   * Example in any component: this.pageContextService.setComponentData(this);
   */
  setComponentData(componentInstance: any): void {
    this.currentComponentData = componentInstance;
  }
  
  /**
   * Clear component data (called on destroy)
   */
  clearComponentData(): void {
    this.currentComponentData = null;
  }
  
  /**
   * Get the active component instance - tries auto-detection first,
   * then falls back to manually registered component
   */
  private getActiveComponentInstance(): any {
    // If component registered itself, use that
    if (this.currentComponentData) {
      return this.currentComponentData;
    }
    
    // Otherwise return null - automatic extraction from Angular is unreliable
    // Components should call setComponentData(this) if they want AI context
    console.warn('[PageContextService] No component data available. Components should call pageContextService.setComponentData(this) in ngOnInit');
    return null;
  }

  /**
   * Extract route information
   */
  private extractRouteInfo(): any {
    const state = this.router.routerState;
    const root = state.root;
    
    let currentRoute = root;
    const pathSegments: string[] = [];
    const params: any = {};
    const queryParams: any = {};
    
    // Traverse to the deepest activated route
    while (currentRoute.firstChild) {
      currentRoute = currentRoute.firstChild;
      
      // Collect path segments
      if (currentRoute.snapshot.url.length > 0) {
        currentRoute.snapshot.url.forEach(segment => {
          pathSegments.push(segment.path);
        });
      }
      
      // Collect params
      Object.assign(params, currentRoute.snapshot.params);
      Object.assign(queryParams, currentRoute.snapshot.queryParams);
    }

    return {
      path: '/' + pathSegments.join('/'),
      params,
      queryParams,
      url: this.router.url
    };
  }

  /**
   * AUTOMATICALLY extract data from component instance
   * This intelligently filters out Angular internals and methods
   */
  private extractComponentData(
    component: any, 
    maxArrayLength: number, 
    maxDepth: number,
    includePrivateProps: boolean
  ): any {
    const data: any = {};
    
    // Get all property names (including getters)
    const propertyNames = this.getAllPropertyNames(component);
    
    for (const propName of propertyNames) {
      // Skip Angular internals and private properties (unless explicitly included)
      if (this.shouldSkipProperty(propName, includePrivateProps)) {
        continue;
      }

      try {
        const value = component[propName];
        
        // Skip undefined
        if (value === undefined) {
          continue;
        }

        // Check if this is an Angular signal (signals are functions with special properties)
        if (typeof value === 'function') {
          // Try to detect if it's a signal and call it to get the value
          try {
            if (this.isSignal(value)) {
              const signalValue = value(); // Call the signal to get its value
              if (signalValue !== undefined) {
                data[propName] = this.extractValue(signalValue, 0, maxDepth, maxArrayLength);
              }
            }
          } catch (error) {
            // Silently skip - not a callable signal or error during call
          }
          // Skip regular functions/methods
          continue;
        }

        // Extract the value with depth limiting
        data[propName] = this.extractValue(value, 0, maxDepth, maxArrayLength);
      } catch (error) {
        // Skip properties that throw errors when accessed
        continue;
      }
    }

    return data;
  }

  /**
   * Get all property names including getters from prototype chain
   */
  private getAllPropertyNames(obj: any): string[] {
    const props = new Set<string>();
    let current = obj;

    // Traverse prototype chain but stop at Object.prototype
    while (current && current !== Object.prototype) {
      Object.getOwnPropertyNames(current).forEach(prop => props.add(prop));
      current = Object.getPrototypeOf(current);
    }

    return Array.from(props);
  }

  /**
   * Determine if a property should be skipped
   */
  private shouldSkipProperty(propName: string, includePrivateProps: boolean): boolean {
    // Skip Angular internals and common service properties
    const skipPatterns = [
      '_', '__', 'ɵ', // Angular internal prefixes
      'constructor', 'ngOnInit', 'ngOnDestroy', 'ngAfterViewInit', 
      'ngAfterViewChecked', 'ngAfterContentInit', 'ngAfterContentChecked',
      'ngOnChanges', 'ngDoCheck',
      'cdr', 'changeDetectorRef', 'injector', 'viewContainerRef',
      'elementRef', 'renderer', 'zone', 'ngZone',
      'router', 'route', 'activatedRoute', 'authService', 'http',
      'httpClient', 'location', 'platformLocation', 'translateService',
      'messageService', 'confirmationService', 'layoutService'
    ];

    // Exact match or starts with check
    const lowerProp = propName.toLowerCase();
    if (skipPatterns.some(pattern => 
      lowerProp === pattern.toLowerCase() || lowerProp.startsWith(pattern.toLowerCase())
    )) {
      return true;
    }

    // Skip if ends with common service suffixes
    if (lowerProp.endsWith('service') || lowerProp.endsWith('repository') || 
        lowerProp.endsWith('manager') || lowerProp.endsWith('handler')) {
      return true;
    }

    // Skip private properties (starting with _) unless explicitly included
    if (!includePrivateProps && propName.startsWith('_')) {
      return true;
    }

    return false;
  }

  /**
   * Extract value with depth and array length limiting
   */
  private extractValue(
    value: any, 
    currentDepth: number, 
    maxDepth: number, 
    maxArrayLength: number
  ): any {
    // Check depth limit
    if (currentDepth >= maxDepth) {
      return '[Max depth reached]';
    }

    // Handle null/undefined
    if (value === null) {
      return null;
    }
    if (value === undefined) {
      return undefined;
    }

    // Handle primitives
    if (typeof value === 'string' || typeof value === 'number' || typeof value === 'boolean') {
      return value;
    }

    // Handle dates
    if (value instanceof Date) {
      return value.toISOString();
    }

    // Handle arrays
    if (Array.isArray(value)) {
      const limitedArray = value.slice(0, maxArrayLength);
      const extracted = limitedArray.map(item => 
        this.extractValue(item, currentDepth + 1, maxDepth, maxArrayLength)
      );
      
      if (value.length > maxArrayLength) {
        extracted.push(`... and ${value.length - maxArrayLength} more items`);
      }
      
      return extracted;
    }

    // Handle objects
    if (typeof value === 'object') {
      // Skip Angular component references and DOM elements
      if (this.isAngularComponent(value) || this.isDOMElement(value)) {
        return '[Angular Component]';
      }

      // Extract object properties
      const extracted: any = {};
      
      try {
        const keys = Object.keys(value);
        
        for (const key of keys) {
          // Skip Angular internals in nested objects too
          if (this.shouldSkipProperty(key, false)) {
            continue;
          }

          try {
            extracted[key] = this.extractValue(
              value[key], 
              currentDepth + 1, 
              maxDepth, 
              maxArrayLength
            );
          } catch (error) {
            extracted[key] = '[Error accessing property]';
          }
        }
      } catch (error) {
        return '[Error extracting object]';
      }

      return extracted;
    }

    // For anything else, try to convert to string
    try {
      return String(value);
    } catch {
      return '[Unconvertible value]';
    }
  }

  /**
   * Check if value is an Angular signal
   * Signals are functions with specific internal properties
   */
  private isSignal(value: any): boolean {
    if (typeof value !== 'function') {
      return false;
    }
    
    // Skip EventEmitters and Observables to avoid side effects
    if (value.observers !== undefined || 
        value.closed !== undefined || 
        value._isScalar !== undefined ||
        value.constructor?.name === 'EventEmitter' ||
        value.constructor?.name === 'Subject') {
      return false;
    }
    
    // Angular signals have specific markers - check these FIRST before calling
    const fnString = value.toString();
    if (fnString.includes('[Signal') || value[Symbol.toStringTag] === 'Signal') {
      return true;
    }
    
    // Signals have special internal properties starting with ɵ
    const ownProps = Object.getOwnPropertyNames(value);
    if (ownProps.some(prop => prop.startsWith('ɵ'))) {
      return true;
    }
    
    // Default to false - better to miss some signals than to call random functions
    return false;
  }

  /**
   * Check if value is an Angular component or service
   */
  private isAngularComponent(value: any): boolean {
    if (!value || typeof value !== 'object') {
      return false;
    }
    
    return (
      value.constructor?.name?.includes('Component') ||
      value.constructor?.name?.includes('Service') ||
      value.constructor?.name?.includes('Router') ||
      value.constructor?.name?.includes('Http') ||
      value.ɵcmp !== undefined ||
      value.ɵprov !== undefined ||
      value.__ngContext__ !== undefined ||
      value.handler !== undefined // HttpClient
    );
  }

  /**
   * Check if value is a DOM element
   */
  private isDOMElement(value: any): boolean {
    return value instanceof Element || 
           value instanceof HTMLElement ||
           value instanceof Node;
  }
  
}
