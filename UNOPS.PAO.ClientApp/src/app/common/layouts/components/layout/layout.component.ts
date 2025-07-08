import { NgClass } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit, Renderer2, ViewChild, effect, signal, TemplateRef, AfterViewInit, HostListener } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router, RouterModule } from '@angular/router';
import { FooterComponent } from '../footer/footer.component';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { TopbarComponent } from '../topbar/topbar.component';
import { Subscription, filter } from 'rxjs';
import { LayoutService } from '../../services/layout.service';
import { BreadcrumbComponent } from './breadcrumb/breadcrumb.component';
import { LanguageService } from '../../../services/language.service';
import { SplitterComponent, SplitterPanel, SplitterResizeEvent } from '../../../reusables/components/splitter/splitter.component';
import { AiAssistantPanelComponent } from '../../../reusables/widgets/ai-assistant/ai-assistant-panel.component';
import { LoadingOverlayComponent, LoadingOverlayService } from '../../../reusables/components/loading-overlay/loading-overlay.component';
import { EntityDetailsPanelComponent } from '../../../components/entity-details-panel/entity-details-panel.component';

@Component({
  selector: 'app-layout',
  imports: [
    TopbarComponent, 
    SidebarComponent, 
    RouterModule, 
    FooterComponent, 
    BreadcrumbComponent, 
    SplitterComponent,
    AiAssistantPanelComponent,
    NgClass,
    LoadingOverlayComponent,
    EntityDetailsPanelComponent
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
  private minAiAssistantSize = 20; // Minimum size when active (20%)

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
      private loadingOverlayService: LoadingOverlayService
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
    
    // If mobile state changed, recalculate splitter sizes
    if (wasMobile !== this.isMobile) {
      this._lastAiAssistantActive = null;
      this._splitterSizes = [];
      this._minSplitterSizes = [];
      
      // Use setTimeout to defer change detection to avoid timing issues
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
        visible: true, // Always visible, but size will be 0 when inactive
        data: { title: 'AI Assistant' }
      }
    ];
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
    const isActive = event.sizes[1] > 0;
    const wasActive = this.layoutService.layoutState().aiAssistantActive ?? false;
    
    // Only update if the active state actually changed
    if (isActive !== wasActive) {
      this.layoutService.layoutState.update((prev) => ({ ...prev, aiAssistantActive: isActive }));
      
      // Clear cache to force recalculation
      this._lastAiAssistantActive = null;
      this._splitterSizes = [];
      this._minSplitterSizes = [];
      
      localStorage.setItem('aiAssistantActive', isActive.toString());
      
      // Use setTimeout to avoid triggering change detection during event handling
      setTimeout(() => {
        this.cdr.markForCheck();
      });
    }
    
    // Only save splitter state if AI assistant is active (to preserve last active size)
    if (isActive) {
      localStorage.setItem('aiAssistantSplitterState', JSON.stringify(event.sizes));
    }
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
    
    // Clear cache to force recalculation
    this._lastAiAssistantActive = null;
    this._splitterSizes = [];
    this._minSplitterSizes = [];
    
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
    
    // Mobile behavior: AI assistant takes full width when active, 0 when inactive
    if (this.isMobile) {
      return isActive ? [0, 100] : [100, 0];
    }
    
    // Desktop behavior (existing logic)
    if (!isActive) {
      return [100, 0]; // AI assistant hidden (width = 0)
    }
    
    // Try to restore saved size, with minimum constraint
    const savedState = localStorage.getItem('aiAssistantSplitterState');
    if (savedState) {
      try {
        const sizes = JSON.parse(savedState);
        if (Array.isArray(sizes) && sizes.length === 2) {
          // Ensure AI assistant panel is at least 20% if active
          const aiSize = Math.max(sizes[1], this.minAiAssistantSize);
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
    
    // Mobile behavior: no minimum constraints needed
    if (this.isMobile) {
      return isActive ? [0, 100] : [100, 0];
    }
    
    // Desktop behavior (existing logic)
    return isActive ? [50, this.minAiAssistantSize] : [100, 0];
  }

  toggleAiAssistant() {
    const currentState = this.layoutService.layoutState();
    const newActiveState = !(currentState.aiAssistantActive ?? false);
    
    this.layoutService.layoutState.update((prev) => ({ 
      ...prev, 
      aiAssistantActive: newActiveState 
    }));
    
    // Clear cache to force recalculation
    this._lastAiAssistantActive = null;
    this._splitterSizes = [];
    this._minSplitterSizes = [];
    
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
    
    // Clear cache to force recalculation
    this._lastAiAssistantActive = null;
    this._splitterSizes = [];
    this._minSplitterSizes = [];
    
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
