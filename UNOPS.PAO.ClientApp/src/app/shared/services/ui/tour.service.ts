import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, NavigationEnd } from '@angular/router';
import { BehaviorSubject, filter, firstValueFrom } from 'rxjs';
import { driver, Driver } from 'driver.js';
import { TourConfig, TourPreferences, TourTrigger } from '../../interfaces/tour.interface';

@Injectable({
  providedIn: 'root'
})
export class TourService {
  private http = inject(HttpClient);
  private currentDriver: Driver | null = null;
  private toursCache = new Map<string, TourConfig>();
  private preferencesSubject = new BehaviorSubject<TourPreferences>(this.getDefaultPreferences());
  private availableTours: string[] = [];

  public preferences$ = this.preferencesSubject.asObservable();

  constructor(private router: Router) {
    this.initializeTours();
    this.setupRouteListener();
  }

  private async initializeTours() {
    // Dynamically discover all tour files from the tour registry
    try {
      // Load tour registry to get all available tour files dynamically
      const registry = await firstValueFrom(
        this.http.get<any>('/assets/tours/tour-registry.json')
      );

      // Extract all tour file names from the registry
      const knownTourFiles = registry.routes.map((route: any) => route.tourFile);

      // Remove duplicates (in case multiple routes use the same tour)
      const uniqueTourFiles = [...new Set(knownTourFiles)];


      // Filter out any tours that don't actually exist
      const existingTours: any[] = [];
      for (const tourId of uniqueTourFiles) {
        try {
          // Check if tour file exists
          await firstValueFrom(
            this.http.get<any>(`/assets/tours/${tourId}.json`)
          );
          existingTours.push(tourId);
        } catch (error) {
          console.warn(`?? Tour file referenced in registry but not found: ${tourId}.json`);
        }
      }

      this.availableTours = existingTours;
    } catch (error) {
      console.error('? Error initializing tours:', error);
      this.availableTours = [];
    }
  }

  private setupRouteListener() {
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        this.handleRouteChange(event.urlAfterRedirects);
      });
  }

  /**
   * Load tour configuration from JSON file
   */
  async loadTour(tourId: string): Promise<TourConfig | null> {
    if (this.toursCache.has(tourId)) {
      return this.toursCache.get(tourId)!;
    }

    try {
      // Load from assets via HTTP
      const tourConfig = await firstValueFrom(
        this.http.get<TourConfig>(`/assets/tours/${tourId}.json`)
      );
      this.toursCache.set(tourId, tourConfig);
      return tourConfig;
    } catch (error) {
      console.warn(`Tour ${tourId} not found:`, error);
      return null;
    }
  }

  /**
   * Start a tour with specified trigger
   */
  async startTour(tourId: string, trigger: TourTrigger = TourTrigger.MANUAL): Promise<boolean> {
    const tourConfig = await this.loadTour(tourId);
    if (!tourConfig) {
      console.error(`Tour ${tourId} not found`);
      return false;
    }

    // Check if tour should be shown based on preferences
    if (!this.shouldShowTour(tourId, trigger)) {
      return false;
    }

    const driverSteps = this.convertToDriverSteps(tourConfig);

    if (driverSteps.length === 0) {
      console.warn(`No valid steps found for tour ${tourId}`);
      return false;
    }

    // Scroll to top before starting the tour
    window.scrollTo({
      top: 0,
      behavior: 'smooth'
    });

    // Small delay to ensure scroll animation completes
    setTimeout(() => {
      this.currentDriver = driver({
        stagePadding: 5,
        showProgress: true,
        allowClose: tourConfig.allowClose,
        popoverOffset: tourConfig.popoverOffset,
        steps: driverSteps,
        overlayColor: 'rgba(0, 0, 0, 0.4)',
        smoothScroll: false,
      });

      this.trackTourProgress(tourId, 0, 'start');
      this.currentDriver.drive();
    }, 500);
    return true;
  }

  /**
   * Convert tour config to Driver.js steps
   */
  private convertToDriverSteps(config: TourConfig): any[] {
    return config.steps
      .map((step, index) => {
        const element = this.findBestElement(step);

        // Skip steps where no element is found (except welcome/overview steps)
        if (!element && step.element) {
          console.warn(`Element not found for step ${index + 1} in tour ${config.tourId}`);
          return null;
        }

        return {
          element: element || undefined,
          popover: {
            title: step.popover.title,
            description: step.popover.description,
            side: step.popover.side === 'over' ? undefined : step.popover.side, // Let Driver.js handle 'over' positioning
            align: step.popover.align,
            showButtons: config.showButtons,
          }
        };
      })
      .filter(step => step !== null);
  }

  /**
   * Find the best available element using fallback selectors
   */
  private findBestElement(step: any): string | null {
    // If no element specified, this is likely a welcome/overview step
    if (!step.element && (!step.options?.selectors || step.options.selectors.length === 0)) {
      return null;
    }

    // Try primary selector first
    if (step.element && document.querySelector(step.element)) {
      return step.element;
    }

    // Try fallback selectors in order
    const selectors = step.options?.selectors || [];
    for (const selector of selectors) {
      if (document.querySelector(selector)) {
        return selector;
      }
    }

    return null;
  }

  /**
   * Handle route changes and auto-start tours
   */
  private async handleRouteChange(url: string) {
    const preferences = this.preferencesSubject.value;
    if (!preferences.autoStart) return;

    // Find tours that match the current route
    const matchingTours = await this.findToursForRoute(url);

    for (const tourId of matchingTours) {
      if (this.shouldShowTour(tourId, TourTrigger.AUTO)) {
        // Delay to ensure page elements are rendered
        setTimeout(() => {
          this.startTour(tourId, TourTrigger.AUTO);
        }, 1000);
        break; // Only show one auto tour per route
      }
    }
  }

  /**
   * Find tours that match the current route
   */
  private async findToursForRoute(url: string): Promise<string[]> {
    const matchingTours: string[] = [];

    for (const tourId of this.availableTours) {
      const config = await this.loadTour(tourId);
      if (config && this.routeMatches(url, config.route)) {
        matchingTours.push(tourId);
      }
    }

    return matchingTours;
  }

  /**
   * Check if current route matches tour route
   */
  private routeMatches(currentUrl: string, tourRoute: string): boolean {
    // Skip tours that don't have direct routes (e.g., modal dialogs)
    if (tourRoute === 'Modal dialog (no direct route)' || !tourRoute || tourRoute.trim() === '') {
      return false;
    }

    // Clean up URLs for comparison
    const cleanCurrentUrl = currentUrl.split('?')[0].split('#')[0]; // Remove query params and fragments
    const cleanTourRoute = tourRoute.split('?')[0].split('#')[0];

    // Exact match
    if (cleanCurrentUrl === cleanTourRoute) {
      return true;
    }

    // Handle Angular route parameters (e.g., :id, :recordId)
    if (this.matchesRoutePattern(cleanCurrentUrl, cleanTourRoute)) {
      return true;
    }

    // Check if current URL starts with the tour route (for static routes)
    if (!cleanTourRoute.includes(':') && cleanCurrentUrl.startsWith(cleanTourRoute)) {
      // Make sure it's a logical extension (e.g., /partnerships/partners/123)
      const remainder = cleanCurrentUrl.substring(cleanTourRoute.length);
      return remainder === '' || remainder.startsWith('/') || remainder.startsWith('?');
    }

    // Legacy wildcard matching for * patterns
    return this.wildcardMatch(cleanCurrentUrl, cleanTourRoute);
  }

  /**
   * Check if URL matches a route pattern with Angular parameters
   */
  private matchesRoutePattern(url: string, pattern: string): boolean {
    // Split both URL and pattern into segments
    const urlSegments = url.split('/').filter(segment => segment !== '');
    const patternSegments = pattern.split('/').filter(segment => segment !== '');

    // Must have at least as many segments as the pattern (allows nested routes)
    if (urlSegments.length < patternSegments.length) {
      return false;
    }

    // Check each segment of the pattern
    for (let i = 0; i < patternSegments.length; i++) {
      const patternSegment = patternSegments[i];
      const urlSegment = urlSegments[i];

      // If pattern segment is a parameter (starts with :), it matches any non-empty segment
      if (patternSegment.startsWith(':')) {
        if (!urlSegment || urlSegment.trim() === '') {
          return false;
        }
        continue;
      }

      // For static segments, they must match exactly
      if (patternSegment !== urlSegment) {
        return false;
      }
    }

    return true;
  }

  private wildcardMatch(url: string, pattern: string): boolean {
    const regex = new RegExp(pattern.replace(/\*/g, '.*'));
    return regex.test(url);
  }

  /**
   * Check if tour should be shown based on preferences and trigger
   */
  private shouldShowTour(tourId: string, trigger: TourTrigger): boolean {
    const preferences = this.preferencesSubject.value;

    // Check if tour was already completed
    if (preferences.completedTours.includes(tourId)) {
      return false;
    }

    // Check if tour was skipped (only block auto triggers)
    if (trigger === TourTrigger.AUTO && preferences.skippedTours.includes(tourId)) {
      return false;
    }

    // Check onboarding preferences
    if (trigger === TourTrigger.ONBOARDING && !preferences.showOnboarding) {
      return false;
    }

    return true;
  }

  /**
   * Track tour progress and events
   */
  private trackTourProgress(tourId: string, stepIndex: number, action: string) {
    const preferences = this.preferencesSubject.value;

    preferences.tourProgress[tourId] = {
      tourId,
      completed: false,
      currentStep: stepIndex
    };

    this.updatePreferences(preferences);

    // Analytics/logging could be added here
  }

  /**
   * Handle tour completion or closure
   */
  private handleTourClose(tourId: string, stepIndex: number, completed: boolean) {
    const preferences = this.preferencesSubject.value;

    if (completed) {
      preferences.completedTours.push(tourId);
      preferences.tourProgress[tourId] = {
        tourId,
        completed: true,
        currentStep: stepIndex,
        completedAt: new Date()
      };
    } else {
      // Mark as skipped if closed before completion
      if (!preferences.skippedTours.includes(tourId)) {
        preferences.skippedTours.push(tourId);
      }
    }

    this.updatePreferences(preferences);
  }

  /**
   * Public API methods
   */

  stopCurrentTour() {
    if (this.currentDriver) {
      this.currentDriver.destroy();
      this.currentDriver = null;
    }
  }

  getAvailableTours(): string[] {
    return [...this.availableTours];
  }

  async getTourConfig(tourId: string): Promise<TourConfig | null> {
    return this.loadTour(tourId);
  }

  resetTourProgress(tourId?: string) {
    const preferences = this.preferencesSubject.value;

    if (tourId) {
      preferences.completedTours = preferences.completedTours.filter(id => id !== tourId);
      preferences.skippedTours = preferences.skippedTours.filter(id => id !== tourId);
      delete preferences.tourProgress[tourId];
    } else {
      preferences.completedTours = [];
      preferences.skippedTours = [];
      preferences.tourProgress = {};
    }

    this.updatePreferences(preferences);
  }

  updatePreferences(newPreferences: Partial<TourPreferences>) {
    const current = this.preferencesSubject.value;
    const updated = { ...current, ...newPreferences };
    this.preferencesSubject.next(updated);
    this.savePreferences(updated);
  }

  /**
   * Preference management
   */
  private getDefaultPreferences(): TourPreferences {
    const stored = localStorage.getItem('tour-preferences');
    if (stored) {
      try {
        return JSON.parse(stored);
      } catch (error) {
        console.warn('Failed to parse stored tour preferences:', error);
      }
    }

    return {
      autoStart: true,
      completedTours: [],
      skippedTours: [],
      showOnboarding: true,
      tourProgress: {}
    };
  }

  private savePreferences(preferences: TourPreferences) {
    try {
      localStorage.setItem('tour-preferences', JSON.stringify(preferences));
    } catch (error) {
      console.warn('Failed to save tour preferences:', error);
    }
  }

  /**
   * Special tour types
   */

  async startOnboardingFlow() {
    const onboardingTours = ['partner-tour', 'contact-tour', 'interaction-tour'];

    for (const tourId of onboardingTours) {
      if (this.shouldShowTour(tourId, TourTrigger.ONBOARDING)) {
        await this.startTour(tourId, TourTrigger.ONBOARDING);
        break; // Start one at a time
      }
    }
  }

  async startFeatureTour(feature: string) {
    const featureTourMap: Record<string, string> = {
      'ai': 'aiassistant-tour',
      'business-cards': 'businesscardscanner-tour',
      'entity-manager': 'entitymanager-tour'
    };

    const tourId = featureTourMap[feature];
    if (tourId) {
      return this.startTour(tourId, TourTrigger.FEATURE_ANNOUNCEMENT);
    }

    return false;
  }

  /**
   * Start tour by entity type (useful for modal dialogs and context-sensitive help)
   */
  async startTourByEntity(entityType: string): Promise<boolean> {
    const entityTourMap: Record<string, string> = {
      'interaction': 'interaction-tour',
      'partner': 'partner-tour',
      'contact': 'contact-tour',
      'contacttabs': 'contacttabs-tour',
      'contactview': 'contactview-tour',
      'partnertree': 'partnertree-tour',
      'partnertreedetails': 'partnertreedetails-tour',
      'partnertreeview': 'partnertreeview-tour',
      'usermanagement': 'usermanagement-tour',
      'interactionlist': 'interactionlist-tour'
    };

    const tourId = entityTourMap[entityType.toLowerCase()];
    if (tourId) {
      return this.startTour(tourId, TourTrigger.MANUAL);
    }

    console.warn(`No tour found for entity type: ${entityType}`);
    return false;
  }

  /**
   * Get tours that match a specific route
   */
  async getToursForCurrentRoute(): Promise<TourConfig[]> {
    const currentUrl = this.router.url;
    const matchingTourIds = await this.findToursForRoute(currentUrl);
    const tourConfigs: TourConfig[] = [];

    for (const tourId of matchingTourIds) {
      const config = await this.loadTour(tourId);
      if (config) {
        tourConfigs.push(config);
      }
    }

    return tourConfigs;
  }

  /**
   * Manually test route detection (for debugging)
   */
  async testRouteDetection(url?: string): Promise<string[]> {
    const testUrl = url || this.router.url;
    return this.findToursForRoute(testUrl);
  }
}
