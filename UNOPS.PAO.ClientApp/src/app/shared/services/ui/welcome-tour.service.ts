import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, NavigationEnd } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { filter, take, firstValueFrom } from 'rxjs';
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
  private http = inject(HttpClient);
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

    // Get current language
    const currentLang = this.translateService.currentLang || this.translateService.defaultLang || 'en';

    // Create welcome messages
    const welcomeMessages = this.getWelcomeMessages(currentLang);

    // Scroll to top before showing welcome tour
    window.scrollTo({
      top: 0,
      behavior: 'smooth'
    });

    // Show fancy welcome overlay first after scroll completes
    setTimeout(() => {
      let tourCompleted = false;

      const welcomeDriver = driver({
        showProgress: false,
        allowClose: true, // Allow users to close the welcome step without starting the tour
        popoverOffset: 20,
        stagePadding: 5,
        smoothScroll: false,
        steps: [
          {
            popover: {
              title: welcomeMessages.title,
              description: welcomeMessages.description,
              side: 'over',
              align: 'center',
              nextBtnText: welcomeMessages.startTourButton,
              showProgress: false
            }
          }
        ],
        onNextClick: () => {
          // User clicked "Next" - mark that they want to start the tour
          tourCompleted = true;
          welcomeDriver.destroy();
        },
        onCloseClick: () => {
          // User clicked close button - just close without starting tour
          welcomeDriver.destroy();
        },
        onDestroyed: () => {
          // Mark welcome as seen regardless of whether they started the tour
          this.markWelcomeAsSeen();

          // Only start homepage tour if user clicked "Next"
          if (tourCompleted) {
            // User clicked "Next" - start the homepage tour
            setTimeout(() => {
              this.startHomepageTour();
            }, 500);
          }
          // If user closed via X button, just mark as seen and don't start the tour
        }
      });

      welcomeDriver.drive();
    }, 500);
  }

  private async startHomepageTour(): Promise<void> {
    try {
      // Load homepage tour configuration (now uses translation keys)
      const tourConfig = await firstValueFrom(
        this.http.get<any>('/assets/tours/homepage-tour.json')
      );

      // Load tour registry for fallback selectors
      const registry = await firstValueFrom(
        this.http.get<any>('/assets/tours/tour-registry.json')
      );

      // Convert tour steps (using simplified version of tour-control logic)
      const driverSteps = this.convertToDriverSteps(tourConfig, registry.fallbackSelectors);

      if (driverSteps.length === 0) {
        console.warn('❌ No valid steps found for homepage tour');
        return;
      }

      // Ensure we're at the top of the page before starting homepage tour
      window.scrollTo({
        top: 0,
        behavior: 'smooth'
      });

      // Create tour instance with custom completion handler after scroll completes
      setTimeout(() => {
        const homepageTour = driver({
          stagePadding: 5,
          showProgress: true,
          allowClose: true,
          popoverOffset: 10,
          steps: driverSteps,
          smoothScroll: false,
          onDestroyed: () => {
            // Mark homepage tour as completed
            this.markHomepageTourCompleted();
          }
        });

        homepageTour.drive();
      }, 500);

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
  }

  public markHomepageTourCompleted(): void {
    const state = this.getWelcomeTourState();
    state.hasCompletedHomepageTour = true;

    if (!state.completedTours.includes('homepage-tour')) {
      state.completedTours.push('homepage-tour');
    }

    this.saveWelcomeTourState(state);
  }

  public markTourCompleted(tourId: string): void {
    const state = this.getWelcomeTourState();

    if (!state.completedTours.includes(tourId)) {
      state.completedTours.push(tourId);
      this.saveWelcomeTourState(state);
    }
  }

  public hasCompletedTour(tourId: string): boolean {
    const state = this.getWelcomeTourState();
    return state.completedTours.includes(tourId);
  }

  public resetWelcomeTourState(): void {
    localStorage.removeItem(this.STORAGE_KEY);
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
