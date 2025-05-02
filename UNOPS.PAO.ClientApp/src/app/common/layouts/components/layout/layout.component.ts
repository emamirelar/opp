import { NgClass } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit, Renderer2, ViewChild } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router, RouterModule } from '@angular/router';
import { FooterComponent } from '../footer/footer.component';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { TopbarComponent } from '../topbar/topbar.component';
import { Subscription, filter } from 'rxjs';
import { LayoutService } from '../../services/layout.service';
import { BreadcrumbComponent } from './breadcrumb/breadcrumb.component';
import { LanguageService } from '../../../services/language.service';
import { AiAssistantComponent } from '../../../reusables/widgets/ai-assistant/ai-assistant.component';
import { LoadingOverlayComponent, LoadingOverlayService } from '../../../reusables/components/loading-overlay/loading-overlay.component';

@Component({
  selector: 'app-layout',
  imports: [
    TopbarComponent, 
    SidebarComponent, 
    RouterModule, 
    FooterComponent, 
    BreadcrumbComponent, 
    AiAssistantComponent, 
    NgClass,
    LoadingOverlayComponent
  ],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.scss',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LayoutComponent implements OnInit, OnDestroy{
  overlayMenuOpenSubscription: Subscription;
  menuOutsideClickListener: any;
  breadcrumbs: string[] = [];
  private langChangeSubscription: Subscription = new Subscription;

  @ViewChild(SidebarComponent) sideBar!: SidebarComponent;
  @ViewChild(TopbarComponent) topBar!: TopbarComponent;
  @ViewChild(LoadingOverlayComponent) loadingOverlay!: LoadingOverlayComponent;

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
  }

  ngOnInit(): void {
    this.langChangeSubscription = this.languageService.translationService.onLangChange.subscribe(() => {
        this.cdr.detectChanges();
    });
  }

  ngAfterViewInit(): void {
    // Register the loading overlay component with the service
    if (this.loadingOverlay) {
      this.loadingOverlayService.registerComponent(this.loadingOverlay);
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

  hideMenu() {
      this.layoutService.layoutState.update((prev) => ({ ...prev, overlayMenuActive: false, staticMenuMobileActive: false, menuHoverActive: false }));
      if (this.menuOutsideClickListener) {
          this.menuOutsideClickListener();
          this.menuOutsideClickListener = null;
      }
      this.unblockBodyScroll();
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
  }
}
