import { Component, OnInit, inject, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { driver } from 'driver.js';
import { WelcomeTourService } from '../../services/welcome-tour.service';

@Component({
  selector: 'app-tour-control',
  standalone: true,
  imports: [CommonModule, ButtonModule, TooltipModule, TranslateModule],
  templateUrl: './tour-control.component.html',
  styles: [`
    :host ::ng-deep .tour-button {
      position: relative;
      transition: all 0.2s ease;
    }

    :host ::ng-deep .tour-button:hover {
      background: var(--unops-neutral-50) !important;
      transform: translateY(-1px);
    }

    :host ::ng-deep .tour-button:focus {
      box-shadow: 0 0 0 2px var(--unops-primary-lighter) !important;
    }

    :host ::ng-deep .tour-button i {
      color: var(--unops-primary);
    }

    :host ::ng-deep .tour-button:hover i {
      color: var(--unops-primary-dark);
    }
  `]
})
export class TourControlComponent implements OnInit {
  @Input() customTourFile?: string; // For dialog-specific tours
  @Input() tourContext?: string; // Additional context for tour selection
  @Input() hideNotificationDot: boolean = false; // Allow hiding the notification dot

  private router = inject(Router);
  private translateService = inject(TranslateService);
  private welcomeTourService = inject(WelcomeTourService);
  private tourRegistry: any = null;

  private async loadTourRegistry() {
    if (!this.tourRegistry) {
      try {
        const registryModule = await import('../../tours/tour-registry.json');
        this.tourRegistry = registryModule.default || registryModule;
      } catch (error) {
        console.error('❌ Failed to load tour registry:', error);
        throw error;
      }
    }
    return this.tourRegistry;
  }

  ngOnInit() {
  }

  showNotificationDot(): boolean {
    // Don't show if explicitly hidden
    if (this.hideNotificationDot) {
      return false;
    }

    // Show notification dot to encourage tour usage
    // Could be enhanced to check if user has taken tours recently
    return true;
  }

  // Development helper method - can be called from browser console
  public resetWelcomeTour(): void {
    this.welcomeTourService.resetWelcomeTourState();
  }

  async detectTour() {

    try {
      // Load tour registry
      const registry = await this.loadTourRegistry();

      let tourFileName = null;

      // Check for custom tour file first (for dialogs)
      if (this.customTourFile) {
        tourFileName = this.customTourFile;
      } else {
        // Get current URL and find matching tour from registry
        const currentUrl = this.router.url;

        for (const route of registry.routes) {
          if (this.matchesRoute(currentUrl, route.pattern)) {
            tourFileName = route.tourFile;
            break;
          }
        }
      }

      if (tourFileName) {
        // Load tour configuration (uses translation keys now)
        const tourModule = await import(`../../tours/${tourFileName}.json`);
        const tourConfig = tourModule.default || tourModule;

        // Convert tour steps to Driver.js format
        const driverSteps = this.convertToDriverSteps(tourConfig, registry.fallbackSelectors);

        if (driverSteps.length === 0) {
          console.warn('❌ No valid steps found for tour');
          return;
        }

        // Scroll to top before starting the tour
        window.scrollTo({
          top: 0,
          behavior: 'smooth'
        });

        // Start the tour with Driver.js after scroll completes
        
        setTimeout(() => {
          const driverInstance = driver({
            stagePadding: 5,
            showProgress: true,
            allowClose: tourConfig.allowClose !== false,
            popoverOffset: tourConfig.popoverOffset || 10,
            steps: driverSteps,
            smoothScroll: false,
            onDestroyed: () => {
              // Mark tour as completed when user finishes or closes
              if (tourConfig.tourId) {
                this.welcomeTourService.markTourCompleted(tourConfig.tourId);
              }
            }
          });

          driverInstance.drive();
        }, 500);

      } else {
        // Show fallback tour for missing pages
        this.showFallbackTour();
      }
    } catch (error) {
      console.error('❌ Failed to load or start tour:', error);
    }
  }

  private convertToDriverSteps(tourConfig: any, fallbackSelectors: any): any[] {

    const validSteps = tourConfig.steps
      .map((step: any, index: number) => {
        const element = this.findBestElement(step, fallbackSelectors);

        if (!element && step.element) {
          console.warn(`⚠️ Step ${index + 1} skipped - element not found, hidden, or disabled: "${step.element}"`);
          console.warn(`   📝 Step title key: "${step.popover?.titleKey}"`);
          if (step.fallbackType) {
            console.warn(`   🔄 Tried fallback type: "${step.fallbackType}"`);
          }
          // Skip steps where element is not found, hidden, or disabled
          return null;
        }

        return {
          element: element || undefined,
          popover: {
            title: this.translateText(step.popover.titleKey || step.popover.title),
            description: this.translateText(step.popover.descriptionKey || step.popover.description),
            side: step.popover.side === 'over' ? undefined : step.popover.side, // Let Driver.js handle 'over' positioning
            align: step.popover.align
          }
        };
      })
      .filter((step: any) => step !== null);

    return validSteps;
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

  private findBestElement(step: any, fallbackSelectors: any): string | null {
    // If no element selector, this might be an intro step
    if (!step.element && !step.fallbackType) {
      return null;
    }

    // Try the main element first
    if (step.element) {
      const element = this.trySelector(step.element);
      if (element) return step.element;
    }

    // Try fallback selectors from the registry based on fallbackType
    if (step.fallbackType && fallbackSelectors[step.fallbackType]) {
      for (const selector of fallbackSelectors[step.fallbackType]) {
        const element = this.trySelector(selector);
        if (element) {
          return selector;
        }
      }
    }

    // Try alternative selectors from the tour JSON (legacy support)
    const selectors = step.options?.selectors || [];
    for (const selector of selectors) {
      const element = this.trySelector(selector);
      if (element) return selector;
    }

    return null;
  }



  private matchesRoute(currentUrl: string, routePattern: string): boolean {
    // Clean URLs (remove query params and fragments)
    const cleanCurrentUrl = currentUrl.split('?')[0].split('#')[0];
    const cleanRoutePattern = routePattern.split('?')[0].split('#')[0];

    // Exact match
    if (cleanCurrentUrl === cleanRoutePattern) {
      return true;
    }

    // Handle parameterized routes (e.g., /partnerships/partners/:id)
    if (cleanRoutePattern.includes(':')) {
      return this.matchesParameterizedRoute(cleanCurrentUrl, cleanRoutePattern);
    }

    // Handle exact prefix matches for non-parameterized routes
    if (cleanCurrentUrl.startsWith(cleanRoutePattern)) {
      const remainder = cleanCurrentUrl.substring(cleanRoutePattern.length);
      return remainder === '' || remainder.startsWith('/') || remainder.startsWith('?');
    }

    return false;
  }

  private matchesParameterizedRoute(currentUrl: string, routePattern: string): boolean {
    // Split both URLs into segments
    const currentSegments = currentUrl.split('/').filter(s => s.length > 0);
    const patternSegments = routePattern.split('/').filter(s => s.length > 0);

    // Must have at least as many segments as the pattern (allows nested routes)
    if (currentSegments.length < patternSegments.length) {
      return false;
    }

    // Check each segment of the pattern
    for (let i = 0; i < patternSegments.length; i++) {
      const patternSegment = patternSegments[i];
      const currentSegment = currentSegments[i];

      // If pattern segment is a parameter (starts with :), it matches any value
      if (patternSegment.startsWith(':')) {
        // Parameter can be any non-empty value
        if (!currentSegment || currentSegment.length === 0) {
          return false;
        }
        continue;
      }

      // Otherwise, segments must match exactly
      if (patternSegment !== currentSegment) {
        return false;
      }
    }

    return true;
  }

  private trySelector(selector: string): Element | null {
    try {
      // Handle :contains() pseudo-selector manually
      if (selector.includes(':contains(')) {
        return this.findElementByText(selector);
      }

      // Skip other invalid selectors
      if (selector.includes('|') ||
          selector.includes('.|') ||
          selector.includes(':has(')) {
        return null;
      }

      // Clean up common selector issues
      const cleanSelector = selector
        .replace(/\s+/g, ' ')  // normalize whitespace
        .trim();

      if (!cleanSelector) return null;

      const element = document.querySelector(cleanSelector);

      // Check if element exists and is visible/enabled for tour purposes
      if (element && this.isElementTourEligible(element)) {
        return element;
      }

      return null;
    } catch (error) {
      // Invalid selector, skip it
      console.warn(`Invalid selector skipped: "${selector}"`);
      return null;
    }
  }

  /**
   * Check if an element is eligible for tour highlighting.
   * Skips elements that are hidden, disabled, or not accessible based on user permissions.
   */
  private isElementTourEligible(element: Element): boolean {
    const htmlElement = element as HTMLElement;

    // Check if element is visible
    const style = window.getComputedStyle(htmlElement);
    if (style.display === 'none' ||
        style.visibility === 'hidden' ||
        style.opacity === '0') {
      return false;
    }

    // Check if element is outside viewport (completely hidden)
    const rect = htmlElement.getBoundingClientRect();
    if (rect.width === 0 && rect.height === 0) {
      return false;
    }

    // Check if button/input is disabled
    if (htmlElement instanceof HTMLButtonElement ||
        htmlElement instanceof HTMLInputElement ||
        htmlElement instanceof HTMLSelectElement ||
        htmlElement instanceof HTMLTextAreaElement) {
      if (htmlElement.disabled) {
        return false;
      }
    }

    // Check for PrimeNG disabled state (common pattern in this app)
    if (htmlElement.classList.contains('p-disabled') ||
        htmlElement.classList.contains('p-button-disabled') ||
        htmlElement.hasAttribute('aria-disabled') ||
        htmlElement.getAttribute('aria-disabled') === 'true') {
      return false;
    }

    // Check for role-based hidden elements (common in this app)
    if (htmlElement.style.display === 'none' ||
        htmlElement.hidden ||
        htmlElement.hasAttribute('hidden')) {
      return false;
    }

    // Element passed all checks
    return true;
  }

  private findElementByText(selector: string): Element | null {
    try {
      // Extract the base selector and text from :contains()
      const containsMatch = selector.match(/^(.+?):contains\("([^"]+)"\)$/);
      if (!containsMatch) return null;

      const [, baseSelector, textContent] = containsMatch;
      const elements = document.querySelectorAll(baseSelector);

      for (let i = 0; i < elements.length; i++) {
        const element = elements[i];
        if (element.textContent?.includes(textContent) && this.isElementTourEligible(element)) {
          return element;
        }
      }

      return null;
    } catch (error) {
      console.warn(`Failed to find element by text: "${selector}"`);
      return null;
    }
  }

  private showFallbackTour() {

    const driverInstance = driver({
      stagePadding: 8,
      showProgress: false,
      allowClose: true,
      popoverOffset: 10,
      nextBtnText: '—›',
      prevBtnText: '‹—',
      doneBtnText: '✕',
      steps: [
        {
          popover: {
            title: this.translateService.instant('tour.fallback.title'),
            description: this.translateService.instant('tour.fallback.description'),
            side: 'over',
            align: 'center'
          }
        }
      ]
    });

    driverInstance.drive();
  }
}
