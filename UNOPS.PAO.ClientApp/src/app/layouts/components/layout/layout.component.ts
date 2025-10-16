import { NgClass, CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit, Renderer2, ViewChild, effect, signal, TemplateRef, AfterViewInit, HostListener } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router, RouterModule } from '@angular/router';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { TopbarComponent } from '../topbar/topbar.component';
import { Subscription, filter } from 'rxjs';
import { LayoutService } from '@layouts/services/layout.service';
import { LanguageService } from '@shared/services/utils';
import { SplitterComponent, SplitterPanel, SplitterResizeEvent } from '@shared/components/layout/splitter/splitter.component';
import { AiAssistantPanelComponent } from '@features/ai/widgets/ai-assistant/ai-assistant-panel.component';
import { LoadingOverlayComponent, LoadingOverlayService } from '@shared/components/layout/loading-overlay/loading-overlay.component';
import { WelcomeTourService } from '@shared/services/ui';


@Component({
  selector: 'app-layout',
  imports: [
    TopbarComponent, 
    SidebarComponent, 
    RouterModule, 
    SplitterComponent,
    AiAssistantPanelComponent,
    NgClass,
    CommonModule,
    LoadingOverlayComponent
  ],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.scss',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LayoutComponent implements OnInit, OnDestroy, AfterViewInit{
  overlayMenuOpenSubscription: Subscription;
  menuOutsideClickListener: any;
  aiAssistantOutsideClickListener: any;
  breadcrumbs: string[] = [];
  private defaultAiAssistantSize = 30; // Default size when active (30%)
  private minAiAssistantPixels = 380; // Minimum width in pixels for AI assistant panel

  // Mobile detection
  isMobile: boolean = false;
  private mobileBreakpoint = 768;

  // Splitter panels configuration
  private _splitterPanels: SplitterPanel[] = [];
  
  get splitterPanels(): SplitterPanel[] {
    return this._splitterPanels;
  }
  
  // Cached splitter configuration to avoid recalculation
  private _splitterSizes: number[] = [];
  private _minSplitterSizes: number[] = [];
  private _lastAiAssistantActive: boolean | null = null;

  @ViewChild(SidebarComponent) sideBar!: SidebarComponent;
  @ViewChild(TopbarComponent) topBar!: TopbarComponent;
  @ViewChild(LoadingOverlayComponent) loadingOverlay!: LoadingOverlayComponent;
  @ViewChild('splitter') splitter!: SplitterComponent;
  @ViewChild('mainContentTemplate') mainContentTemplate!: TemplateRef<any>;
  @ViewChild('aiAssistantTemplate') aiAssistantTemplate!: TemplateRef<any>;

  constructor(
      public layoutService: LayoutService,
      public renderer: Renderer2,
      public router: Router,
      private activatedRoute: ActivatedRoute,
      private languageService: LanguageService,
      private cdr: ChangeDetectorRef,
      private loadingOverlayService: LoadingOverlayService,
      private welcomeTourService: WelcomeTourService
  ) {
      this.overlayMenuOpenSubscription = this.layoutService.overlayOpen$.subscribe(() => {
          if (!this.menuOutsideClickListener) {
              this.menuOutsideClickListener = this.renderer.listen('document', 'click', (event) => {
                  if (this.isOutsideClicked(event)) {
                      this.hideMenu();
                  }
              });
          }

          if (this.layoutService.layoutState().staticMenuMobileActive) {
              this.blockBodyScroll();
          }
      });

      this.router.events.pipe(filter((event) => event instanceof NavigationEnd)).subscribe(() => {
          this.updateBreadcrumbs(this.activatedRoute.root);
          this.hideMenu();
          // Re-initialize splitter panels when route changes (to handle AI route vs normal routes)
          this.initializeSplitterPanels();
      });
      
      // Listen for AI assistant state changes to add/remove outside click listeners
      effect(() => {
        const state = this.layoutService.layoutState();
        if (this.isMobile && state.aiAssistantActive) {
          this.addAiAssistantOutsideClickListener();
        } else {
          this.removeAiAssistantOutsideClickListener();
        }
      });

      // Listen for AI assistant active state changes to update sizes only
      effect(() => {
        const state = this.layoutService.layoutState();
        const isActive = state.aiAssistantActive ?? false;
        
        // Check if the state actually changed to avoid unnecessary recalculation
        if (this._lastAiAssistantActive !== isActive) {
          // Update the tracked state FIRST to prevent infinite loops
          this._lastAiAssistantActive = isActive;
          
          // Clear cache to force recalculation of sizes only
          this._splitterSizes = [];
          this._minSplitterSizes = [];
          
          // Reinitialize panels to update minSize property based on active state
          this.initializeSplitterPanels();
          
          // Update localStorage
          localStorage.setItem('aiAssistantActive', isActive.toString());
          
          // Force change detection
          setTimeout(() => {
            this.cdr.markForCheck();
          });
        }
      });

      // Initialize mobile detection without triggering change detection
      this.isMobile = window.innerWidth <= this.mobileBreakpoint;
  }

  @HostListener('window:resize', ['$event'])
  onResize(event: any) {
    this.checkMobile();
  }

  private checkMobile(): void {
    const wasMobile = this.isMobile;
    this.isMobile = window.innerWidth <= this.mobileBreakpoint;
    
    // If switching from desktop to mobile, close AI assistant
    if (!wasMobile && this.isMobile) {
      this.closeAiAssistantOnMobile();
    }
    
    // If mobile state changed or window resized, recalculate splitter sizes
    if (wasMobile !== this.isMobile) {
      this._lastAiAssistantActive = null;
      this._splitterSizes = [];
      this._minSplitterSizes = [];
      
      // Use setTimeout to defer change detection to avoid timing issues
      setTimeout(() => {
        this.cdr.markForCheck();
      });
    } else if (!this.isMobile) {
      // On desktop, recalculate minimum sizes when window resizes (for pixel-based constraints)
      this._minSplitterSizes = [];
      
      setTimeout(() => {
        this.cdr.markForCheck();
      });
    }
  }

  ngOnInit(): void {
    this.languageService.translationService.onLangChange.subscribe(() => {
      this.cdr.markForCheck();
    });
    
    // Close AI Assistant on mobile refresh
    if (this.isMobile) {
      this.closeAiAssistantOnMobile();
    }
    
    this.restoreSplitterState();
  }

  ngAfterViewInit(): void {
    // Register the loading overlay component with the service
    if (this.loadingOverlay) {
      this.loadingOverlayService.registerComponent(this.loadingOverlay);
    }

    // Initialize splitter panels with templates
    setTimeout(() => {
      this.initializeSplitterPanels();
    });
  }

  private initializeSplitterPanels(): void {
    // Check if we're on the AI route - if so, only show main content
    const isOnAiRoute = this.router.url.startsWith('/ai');
    
    if (isOnAiRoute) {
      // On AI route, only show main content panel (AI content component handles the AI display)
      this._splitterPanels = [
        {
          id: 'main-content',
          template: this.mainContentTemplate,
          resizable: false, // Not resizable when in AI fullscreen mode
          visible: true,
          data: { title: 'Main Content' }
        }
      ];
    } else {
      // Normal layout with both main content and AI assistant panels
      // Only set minimum size if AI assistant is active
      const isActive = this.layoutService.layoutState().aiAssistantActive;
      let minAiAssistantSize: number | undefined = undefined;
      
      if (isActive) {
        // Calculate minimum percentage for AI assistant based on 380px requirement
        const containerWidth = window.innerWidth - 250; // Approximate available width (minus sidebar)
        minAiAssistantSize = Math.max(20, (this.minAiAssistantPixels / containerWidth) * 100);
      }
      
      this._splitterPanels = [
        {
          id: 'main-content',
          template: this.mainContentTemplate,
          resizable: true,
          visible: true,
          data: { title: 'Main Content' }
        },
        {
          id: 'ai-assistant',
          template: this.aiAssistantTemplate,
          resizable: true,
          visible: true, // Always visible in DOM - visibility controlled by size calculations
          minSize: minAiAssistantSize, // Only set minimum size when active
          data: { title: 'AI Assistant' }
        }
      ];
    }
  }

  updateBreadcrumbs(route: any) {
      const breadcrumbs: string[] = [];
      while (route) {
          if (route.snapshot.data['breadcrumb']) {
              breadcrumbs.push(route.snapshot.data['breadcrumb']);
          }
          route = route.firstChild;
      }
      this.breadcrumbs = breadcrumbs.reverse();
  }

  isOutsideClicked(event: MouseEvent) {
      const sidebarEl = document.querySelector('.layout-sidebar');
      const topbarEl = document.querySelector('.layout-menu-button');
      const eventTarget = event.target as Node;

      return !(sidebarEl?.isSameNode(eventTarget) || sidebarEl?.contains(eventTarget) || topbarEl?.isSameNode(eventTarget) || topbarEl?.contains(eventTarget));
  }

  isAiAssistantOutsideClicked(event: MouseEvent) {
      const aiAssistantEl = document.querySelector('.ai-assistant-panel');
      const aiToggleEl = document.querySelector('[data-ai-assistant-toggle]');
      const eventTarget = event.target as Node;

      return !(aiAssistantEl?.isSameNode(eventTarget) || aiAssistantEl?.contains(eventTarget) || aiToggleEl?.isSameNode(eventTarget) || aiToggleEl?.contains(eventTarget));
  }

  hideMenu() {
      this.layoutService.layoutState.update((prev) => ({ ...prev, overlayMenuActive: false, staticMenuMobileActive: false, menuHoverActive: false }));
      if (this.menuOutsideClickListener) {
          this.menuOutsideClickListener();
          this.menuOutsideClickListener = null;
      }
      this.unblockBodyScroll();
      
      // Also close AI assistant on mobile when menu is hidden (e.g., route change)
      if (this.isMobile && this.layoutService.layoutState().aiAssistantActive) {
        this.closeAiAssistantOnMobile();
      }
  }

  blockBodyScroll(): void {
      if (document.body.classList) {
          document.body.classList.add('blocked-scroll');
      } else {
          document.body.className += ' blocked-scroll';
      }
  }

  unblockBodyScroll(): void {
      if (document.body.classList) {
          document.body.classList.remove('blocked-scroll');
      } else {
          document.body.className = document.body.className.replace(new RegExp('(^|\\b)' + 'blocked-scroll'.split(' ').join('|') + '(\\b|$)', 'gi'), ' ');
      }
  }

  get containerClass() {
      return {
          'layout-overlay': this.layoutService.layoutConfig().menuMode === 'overlay',
          'layout-static': this.layoutService.layoutConfig().menuMode === 'static',
          'layout-static-inactive': this.layoutService.layoutState().staticMenuDesktopInactive && this.layoutService.layoutConfig().menuMode === 'static',
          'layout-overlay-active': this.layoutService.layoutState().overlayMenuActive,
          'layout-mobile-active': this.layoutService.layoutState().staticMenuMobileActive
      };
  }

  ngOnDestroy() {
      if (this.overlayMenuOpenSubscription) {
          this.overlayMenuOpenSubscription.unsubscribe();
      }

      if (this.menuOutsideClickListener) {
          this.menuOutsideClickListener();
      }
      
      if (this.aiAssistantOutsideClickListener) {
          this.aiAssistantOutsideClickListener();
      }
  }

  rememberSplitterState(event: SplitterResizeEvent) {
    // Update the assistant state based on panel size (0 means hidden)
    const isActive = event.sizes.length >= 2 && event.sizes[1] > 0;
    const wasActive = this.layoutService.layoutState().aiAssistantActive ?? false;
    const panelSize = event.sizes.length >= 2 ? event.sizes[1] : 0;
    
    // Always update the layout state to trigger responsive recalculations
    // This ensures components can react to panel size changes, not just active/inactive changes
    this.layoutService.layoutState.update((prev) => ({ 
      ...prev, 
      aiAssistantActive: isActive,
      aiAssistantPanelSize: panelSize
    }));
    
    // Clear cache to force recalculation when state changes
    if (isActive !== wasActive) {
      this._lastAiAssistantActive = null;
      this._splitterSizes = [];
      this._minSplitterSizes = [];
      
      localStorage.setItem('aiAssistantActive', isActive.toString());
    }
    
    // Always save panel size for responsive calculations
    localStorage.setItem('aiAssistantPanelSize', panelSize.toString());
    
    // Only save splitter state if AI assistant is active (to preserve last active size)
    if (isActive) {
      localStorage.setItem('aiAssistantSplitterState', JSON.stringify(event.sizes));
    }
    
    // Use setTimeout to avoid triggering change detection during event handling
    setTimeout(() => {
      this.cdr.markForCheck();
    });
  }

  // No longer needed - panels are always present, only sizes change
  private updateAiAssistantPanelVisibility(isActive: boolean): void {
    // Panel is always visible, only size changes based on isActive state
    // The splitter will handle the size changes automatically via splitterSizes getter
  }

  private syncAiAssistantPanelVisibility(): void {
    // No longer needed - panels are always present
  }

  restoreSplitterState() {
    const aiAssistantActive = localStorage.getItem('aiAssistantActive');
    
    // If no saved state, default to false (closed)
    const isActive = aiAssistantActive === null ? false : aiAssistantActive === 'true';
    
    this.layoutService.layoutState.update((prev) => ({ ...prev, aiAssistantActive: isActive }));
    
    // Clear cache to force recalculation of sizes
    this._lastAiAssistantActive = null;
    this._splitterSizes = [];
    this._minSplitterSizes = [];
    
    // Initialize panels (only needed once during component initialization)
    this.initializeSplitterPanels();
    
    // IMPORTANT: Clear splitter's own state if AI assistant should be closed
    if (!isActive) {
      sessionStorage.removeItem('ai-assistant-splitter');
      localStorage.removeItem('ai-assistant-splitter');
    }
    
    // Save the default state if it wasn't already saved
    if (aiAssistantActive === null) {
      localStorage.setItem('aiAssistantActive', isActive.toString());
    }
    
    // Trigger change detection after state update
    setTimeout(() => {
      this.cdr.markForCheck();
    });
  }

  get splitterSizes(): number[] {
    const isActive = this.layoutService.layoutState().aiAssistantActive ?? false;
    
    // Only recalculate if the AI assistant state has changed
    if (this._lastAiAssistantActive !== isActive || this._splitterSizes.length === 0) {
      this._lastAiAssistantActive = isActive;
      this._splitterSizes = this.calculateSplitterSizes();
    }
    
    return this._splitterSizes;
  }

  get minSplitterSizes(): number[] {
    const isActive = this.layoutService.layoutState().aiAssistantActive ?? false;
    
    // Only recalculate if the AI assistant state has changed
    if (this._lastAiAssistantActive !== isActive || this._minSplitterSizes.length === 0) {
      this._minSplitterSizes = this.calculateMinSplitterSizes();
    }
    
    return this._minSplitterSizes;
  }

  private calculateSplitterSizes(): number[] {
    const isActive = this.layoutService.layoutState().aiAssistantActive;
    
    // When AI assistant is not active, give it 0% size but keep it in DOM
    if (!isActive) {
      return [100, 0]; // Main content takes full width, AI assistant is 0% (hidden but in DOM)
    }
    
    // Mobile behavior: AI assistant takes full width when active
    if (this.isMobile) {
      return [0, 100]; // Hide main content, show AI assistant full screen
    }
    
    // Desktop behavior with AI assistant active
    // Try to restore saved size, with minimum constraint
    const containerWidth = window.innerWidth - 250; // Approximate available width (minus sidebar)
    const minAiAssistantPercentage = Math.max(20, (this.minAiAssistantPixels / containerWidth) * 100);
    
    const savedState = localStorage.getItem('aiAssistantSplitterState');
    if (savedState) {
      try {
        const sizes = JSON.parse(savedState);
        if (Array.isArray(sizes) && sizes.length === 2) {
          // Ensure AI assistant panel meets the minimum pixel requirement
          const aiSize = Math.max(sizes[1], minAiAssistantPercentage);
          const mainSize = 100 - aiSize;
          return [mainSize, aiSize];
        }
      } catch (e) {
        console.warn('Could not parse saved splitter state:', e);
      }
    }
    
    // Default sizes: 70% main content, 30% AI assistant
    return [100 - this.defaultAiAssistantSize, this.defaultAiAssistantSize];
  }

  private calculateMinSplitterSizes(): number[] {
    const isActive = this.layoutService.layoutState().aiAssistantActive;
    
    // When AI assistant is not active, main content can be 100%, AI assistant stays at 0%
    if (!isActive) {
      return [0, 0]; // Main content minimum 0% (can be 100%), AI assistant fixed at 0%
    }
    
    // Mobile behavior: no minimum constraints needed
    if (this.isMobile) {
      return [0, 100];
    }
    
    // Desktop behavior with AI assistant active
    // Calculate minimum percentage based on 380px requirement
    const containerWidth = window.innerWidth - 250; // Approximate available width (minus sidebar)
    const minAiAssistantPercentage = Math.max(20, (this.minAiAssistantPixels / containerWidth) * 100);
    const maxMainContentPercentage = 100 - minAiAssistantPercentage;
    
    return [0, minAiAssistantPercentage]; // Main content can shrink to 0, AI assistant has pixel-based minimum
  }

  toggleAiAssistant() {
    const currentState = this.layoutService.layoutState();
    const newActiveState = !(currentState.aiAssistantActive ?? false);
    
    this.layoutService.layoutState.update((prev) => ({ 
      ...prev, 
      aiAssistantActive: newActiveState 
    }));
    
    // Clear cache to force recalculation of sizes
    this._lastAiAssistantActive = null;
    this._splitterSizes = [];
    this._minSplitterSizes = [];
    
    // Reinitialize panels to update minSize property based on new active state
    this.initializeSplitterPanels();
    
    localStorage.setItem('aiAssistantActive', newActiveState.toString());
    
    // Force immediate change detection
    this.cdr.markForCheck();
  }

  /**
   * Close AI Assistant on mobile refresh to avoid UI clutter
   */
  private closeAiAssistantOnMobile(): void {
    this.layoutService.layoutState.update((prev) => ({ 
      ...prev, 
      aiAssistantActive: false 
    }));
    
    // Clear cache to force recalculation of sizes
    this._lastAiAssistantActive = null;
    this._splitterSizes = [];
    this._minSplitterSizes = [];
    
    // Reinitialize panels to update minSize property based on new active state
    this.initializeSplitterPanels();
    
    // Update localStorage to reflect closed state
    localStorage.setItem('aiAssistantActive', 'false');
    
    // Clear any saved splitter state
    localStorage.removeItem('aiAssistantSplitterState');
    sessionStorage.removeItem('ai-assistant-splitter');
    
    // Remove outside click listener if active
    this.removeAiAssistantOutsideClickListener();
  }
  
  private addAiAssistantOutsideClickListener(): void {
    if (!this.aiAssistantOutsideClickListener) {
      this.aiAssistantOutsideClickListener = this.renderer.listen('document', 'click', (event) => {
        if (this.isAiAssistantOutsideClicked(event)) {
          this.closeAiAssistantOnMobile();
        }
      });
    }
  }
  
  private removeAiAssistantOutsideClickListener(): void {
    if (this.aiAssistantOutsideClickListener) {
      this.aiAssistantOutsideClickListener();
      this.aiAssistantOutsideClickListener = null;
    }
  }
}
