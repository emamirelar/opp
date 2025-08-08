import { Component, OnInit, inject } from '@angular/core';
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
    /* No special styling needed - button is now inline in the header */
  `]
})
export class TourControlComponent implements OnInit {
  private router = inject(Router);
  private translateService = inject(TranslateService);
  private welcomeTourService = inject(WelcomeTourService);
  private tourRegistry: any = null;

  private async loadTourRegistry() {
    if (!this.tourRegistry) {
      try {
        const registryModule = await import('../../tours/tour-registry.json');
        this.tourRegistry = registryModule.default || registryModule;
        console.log('📖 Tour registry loaded:', this.tourRegistry);
      } catch (error) {
        console.error('❌ Failed to load tour registry:', error);
        throw error;
      }
    }
    return this.tourRegistry;
  }

  ngOnInit() {
    console.log('🎯 TourControlComponent initialized');
  }

  // Development helper method - can be called from browser console
  public resetWelcomeTour(): void {
    this.welcomeTourService.resetWelcomeTourState();
    console.log('🔄 Welcome tour reset - refresh homepage to see welcome tour again');
  }

  async detectTour() {
    console.log('🎯 Starting tour for current page...');
    
    try {
      // Load tour registry
      const registry = await this.loadTourRegistry();
      
      // Get current URL
      const currentUrl = this.router.url;
      console.log('📍 Current URL:', currentUrl);
      
      // Find matching tour from registry
      let tourFileName = null;
      for (const route of registry.routes) {
        if (this.matchesRoute(currentUrl, route.pattern)) {
          tourFileName = route.tourFile;
          console.log('🎯 Found matching route:', route.pattern, '→', tourFileName);
          break;
        }
      }
      
      if (tourFileName) {
        // Load tour configuration (uses translation keys now)
        const tourModule = await import(`../../tours/${tourFileName}.json`);
        const tourConfig = tourModule.default || tourModule;
        console.log('📋 Loaded tour:', this.translateText(tourConfig.titleKey || tourConfig.title));
        
        // Convert tour steps to Driver.js format
        const driverSteps = this.convertToDriverSteps(tourConfig, registry.fallbackSelectors);
        
        if (driverSteps.length === 0) {
          console.warn('❌ No valid steps found for tour');
          return;
        }
        
        // Start the tour with Driver.js
        console.log('🚀 Starting Driver.js tour with', driverSteps.length, 'steps');
        
        const driverInstance = driver({
          showProgress: true,
          allowClose: tourConfig.allowClose !== false,
          popoverOffset: tourConfig.popoverOffset || 10,
          steps: driverSteps,
          onDestroyed: () => {
            // Mark tour as completed when user finishes or closes
            if (tourConfig.tourId) {
              this.welcomeTourService.markTourCompleted(tourConfig.tourId);
              console.log(`✅ Tour "${tourConfig.tourId}" marked as completed`);
            }
          }
        });
        
        driverInstance.drive();
        
      } else {
        console.log('❌ No tour found for current route');
        console.log('📝 Available routes:', registry.routes.map((r: any) => r.pattern));
        
        // Show fallback tour for missing pages
        this.showFallbackTour();
      }
    } catch (error) {
      console.error('❌ Failed to load or start tour:', error);
    }
  }

  private convertToDriverSteps(tourConfig: any, fallbackSelectors: any): any[] {
    console.log('🔄 Converting tour steps and checking element eligibility...');
    
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

        if (!element && !step.element) {
          // This is an intro/welcome step without a specific element
          console.log(`✅ Step ${index + 1} (intro): "${step.popover?.titleKey}"`);
        } else {
          console.log(`✅ Step ${index + 1} (${element}): "${step.popover?.titleKey}"`);
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

    console.log(`📊 Tour summary: ${validSteps.length}/${tourConfig.steps.length} steps will be shown`);
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
      console.log(`🔄 Trying fallback selectors for type: ${step.fallbackType}`);
      for (const selector of fallbackSelectors[step.fallbackType]) {
        const element = this.trySelector(selector);
        if (element) {
          console.log(`✅ Found element using fallback: ${selector}`);
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
    
    // Must have same number of segments
    if (currentSegments.length !== patternSegments.length) {
      return false;
    }
    
    // Check each segment
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
      console.log(`🚫 Skipping hidden element: ${element.tagName}${element.className ? '.' + element.className.split(' ').join('.') : ''}`);
      return false;
    }

    // Check if element is outside viewport (completely hidden)
    const rect = htmlElement.getBoundingClientRect();
    if (rect.width === 0 && rect.height === 0) {
      console.log(`🚫 Skipping zero-size element: ${element.tagName}${element.className ? '.' + element.className.split(' ').join('.') : ''}`);
      return false;
    }

    // Check if button/input is disabled
    if (htmlElement instanceof HTMLButtonElement || 
        htmlElement instanceof HTMLInputElement || 
        htmlElement instanceof HTMLSelectElement ||
        htmlElement instanceof HTMLTextAreaElement) {
      if (htmlElement.disabled) {
        console.log(`🚫 Skipping disabled form element: ${element.tagName}${element.className ? '.' + element.className.split(' ').join('.') : ''}`);
        return false;
      }
    }

    // Check for PrimeNG disabled state (common pattern in this app)
    if (htmlElement.classList.contains('p-disabled') ||
        htmlElement.classList.contains('p-button-disabled') ||
        htmlElement.hasAttribute('aria-disabled') ||
        htmlElement.getAttribute('aria-disabled') === 'true') {
      console.log(`🚫 Skipping PrimeNG disabled element: ${element.tagName}${element.className ? '.' + element.className.split(' ').join('.') : ''}`);
      return false;
    }

    // Check for role-based hidden elements (common in this app)
    if (htmlElement.style.display === 'none' ||
        htmlElement.hidden ||
        htmlElement.hasAttribute('hidden')) {
      console.log(`🚫 Skipping hidden attribute element: ${element.tagName}${element.className ? '.' + element.className.split(' ').join('.') : ''}`);
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
    console.log('🎯 Showing fallback tour message');
    
    const driverInstance = driver({
      showProgress: false,
      allowClose: true,
      popoverOffset: 10,
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
