import { Injectable, inject } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { filter, take } from 'rxjs/operators';
import { driver } from 'driver.js';

export interface WelcomeTourState {
  hasSeenWelcome: boolean;
  hasCompletedHomepageTour: boolean;
  completedTours: string[];
  firstVisitDate: string;
  lastWelcomeDate?: string;
}

@Injectable({
  providedIn: 'root'
})
export class WelcomeTourService {
  private router = inject(Router);
  private translateService = inject(TranslateService);

  private readonly STORAGE_KEY = 'unops-welcome-tour-state';
  private readonly WELCOME_DELAY = 1500; // 1.5 seconds delay for smooth experience

  constructor() {
    this.initializeWelcomeDetection();
  }

  private initializeWelcomeDetection(): void {
    // Listen for navigation events to detect homepage visits
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: NavigationEnd) => {
      // Check if user is on homepage and eligible for welcome tour
      if (this.isHomepage(event.url) && this.shouldShowWelcomeTour()) {
        // Add delay for smooth page load experience
        setTimeout(() => {
          this.showWelcomeTour();
        }, this.WELCOME_DELAY);
      }
    });
  }

  private isHomepage(url: string): boolean {
    const cleanUrl = url.split('?')[0].split('#')[0];
    return cleanUrl === '/' || cleanUrl === '';
  }

  private shouldShowWelcomeTour(): boolean {
    const state = this.getWelcomeTourState();

    // Don't show if user has already seen welcome or completed homepage tour
    if (state.hasSeenWelcome || state.hasCompletedHomepageTour) {
      return false;
    }

    // Don't show if user has completed the homepage tour via manual trigger
    if (state.completedTours.includes('homepage-tour')) {
      return false;
    }

    return true;
  }

  private async showWelcomeTour(): Promise<void> {
    console.log('🎉 Showing welcome tour for first-time user');

    // Get current language
    const currentLang = this.translateService.currentLang || this.translateService.defaultLang || 'en';

    // Create welcome messages
    const welcomeMessages = this.getWelcomeMessages(currentLang);

    // Show fancy welcome overlay first
    const welcomeDriver = driver({
      showProgress: false,
      allowClose: false,
      popoverOffset: 20,
      stagePadding: 5,
      steps: [
        {
          popover: {
            title: welcomeMessages.title,
            description: welcomeMessages.description,
            side: 'over',
            align: 'center',
            showButtons: ['next'],
            nextBtnText: welcomeMessages.startTourButton,
            showProgress: false
          }
        }
      ],
      onDestroyed: () => {
        // Mark welcome as seen
        this.markWelcomeAsSeen();

        // Start homepage tour after a brief pause
        setTimeout(() => {
          this.startHomepageTour();
        }, 500);
      }
    });

    welcomeDriver.drive();
  }

  private async startHomepageTour(): Promise<void> {
    try {
      // Load homepage tour configuration (now uses translation keys)
      const tourModule = await import(`../tours/homepage-tour.json`);
      const tourConfig = tourModule.default || tourModule;
      console.log('📋 Loaded homepage tour for welcome sequence');

      // Load tour registry for fallback selectors
      const registryModule = await import('../tours/tour-registry.json');
      const registry = registryModule.default || registryModule;

      // Convert tour steps (using simplified version of tour-control logic)
      const driverSteps = this.convertToDriverSteps(tourConfig, registry.fallbackSelectors);

      if (driverSteps.length === 0) {
        console.warn('❌ No valid steps found for homepage tour');
        return;
      }

      // Create tour instance with custom completion handler
      const homepageTour = driver({
        stagePadding: 5,
        showProgress: true,
        allowClose: true,
        popoverOffset: 10,
        steps: driverSteps,
        onDestroyed: () => {
          // Mark homepage tour as completed
          this.markHomepageTourCompleted();
          console.log('✅ Welcome sequence completed - homepage tour finished');
        }
      });

      homepageTour.drive();

    } catch (error) {
      console.error('❌ Failed to start homepage tour in welcome sequence:', error);
    }
  }

  private convertToDriverSteps(tourConfig: any, fallbackSelectors: any): any[] {
    // Simplified version of tour conversion logic
    return tourConfig.steps
      .map((step: any, index: number) => {
        const element = this.findBestElement(step, fallbackSelectors);

        if (!element && step.element) {
          console.warn(`⚠️ Welcome tour step ${index + 1} skipped - element not found: "${step.element}"`);
          return null;
        }

        return {
          element: element || undefined,
          popover: {
            title: this.translateText(step.popover.titleKey || step.popover.title),
            description: this.translateText(step.popover.descriptionKey || step.popover.description),
            side: step.popover.side === 'over' ? 'top' : step.popover.side,
            align: step.popover.align
          }
        };
      })
      .filter((step: any) => step !== null);
  }

  private findBestElement(step: any, fallbackSelectors: any): string | null {
    // If no element selector, this might be an intro step
    if (!step.element && !step.fallbackType) {
      return null;
    }

    // Try the main element first
    if (step.element && this.isElementVisible(step.element)) {
      return step.element;
    }

    // Try fallback selectors
    if (step.fallbackType && fallbackSelectors[step.fallbackType]) {
      for (const selector of fallbackSelectors[step.fallbackType]) {
        if (this.isElementVisible(selector)) {
          return selector;
        }
      }
    }

    return null;
  }

  private isElementVisible(selector: string): boolean {
    try {
      const element = document.querySelector(selector);
      if (!element) return false;

      const style = window.getComputedStyle(element as HTMLElement);
      return style.display !== 'none' &&
             style.visibility !== 'hidden' &&
             style.opacity !== '0';
    } catch {
      return false;
    }
  }

  private getWelcomeMessages(language: string) {
    return {
      title: this.translateService.instant('tour.welcome.title'),
      description: this.translateService.instant('tour.welcome.description'),
      startTourButton: this.translateService.instant('tour.welcome.startButton')
    };
  }

  // Public methods for managing tour state
  public getWelcomeTourState(): WelcomeTourState {
    const stored = localStorage.getItem(this.STORAGE_KEY);

    if (stored) {
      try {
        return JSON.parse(stored);
      } catch (error) {
        console.warn('Failed to parse welcome tour state, resetting...');
      }
    }

    // Default state for new users
    const defaultState: WelcomeTourState = {
      hasSeenWelcome: false,
      hasCompletedHomepageTour: false,
      completedTours: [],
      firstVisitDate: new Date().toISOString()
    };

    this.saveWelcomeTourState(defaultState);
    return defaultState;
  }

  public markWelcomeAsSeen(): void {
    const state = this.getWelcomeTourState();
    state.hasSeenWelcome = true;
    state.lastWelcomeDate = new Date().toISOString();
    this.saveWelcomeTourState(state);
    console.log('✅ Welcome overlay marked as seen');
  }

  public markHomepageTourCompleted(): void {
    const state = this.getWelcomeTourState();
    state.hasCompletedHomepageTour = true;

    if (!state.completedTours.includes('homepage-tour')) {
      state.completedTours.push('homepage-tour');
    }

    this.saveWelcomeTourState(state);
    console.log('✅ Homepage tour marked as completed');
  }

  public markTourCompleted(tourId: string): void {
    const state = this.getWelcomeTourState();

    if (!state.completedTours.includes(tourId)) {
      state.completedTours.push(tourId);
      this.saveWelcomeTourState(state);
      console.log(`✅ Tour "${tourId}" marked as completed`);
    }
  }

  public hasCompletedTour(tourId: string): boolean {
    const state = this.getWelcomeTourState();
    return state.completedTours.includes(tourId);
  }

  public resetWelcomeTourState(): void {
    localStorage.removeItem(this.STORAGE_KEY);
    console.log('🔄 Welcome tour state reset - user will see welcome on next homepage visit');
  }

  private saveWelcomeTourState(state: WelcomeTourState): void {
    try {
      localStorage.setItem(this.STORAGE_KEY, JSON.stringify(state));
    } catch (error) {
      console.error('Failed to save welcome tour state:', error);
    }
  }

  // Method to manually trigger welcome tour (for testing or reset)
  public triggerWelcomeTour(): void {
    if (this.isHomepage(this.router.url)) {
      this.showWelcomeTour();
    } else {
      console.warn('Welcome tour can only be triggered from homepage');
    }
  }

  private translateText(textOrKey: string): string {
    if (!textOrKey) return '';

    // If it looks like a translation key (contains dots), translate it
    if (textOrKey.includes('.') && !textOrKey.includes(' ')) {
      const translated = this.translateService.instant(textOrKey);
      // If translation key not found, it returns the key itself
      return translated !== textOrKey ? translated : textOrKey;
    }

    // Otherwise, return as is (for backward compatibility with existing literal text)
    return textOrKey;
  }
}
