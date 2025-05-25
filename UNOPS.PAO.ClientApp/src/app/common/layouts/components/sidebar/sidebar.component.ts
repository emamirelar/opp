import { Component, OnInit, ElementRef, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef, signal } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { MenuComponent } from '../menu/menu.component';
import { AuthService } from '../../../../essentials/services/auth.service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LanguageService } from '../../../services/language.service';
import { Subscription } from 'rxjs/internal/Subscription';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-sidebar',
  imports: [MenuComponent, CommonModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss'],
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SidebarComponent implements OnInit, OnDestroy {
  constructor(
    public el: ElementRef,
    private authService: AuthService,
    public languageService: LanguageService,
    private cdr: ChangeDetectorRef,
    private translateService: TranslateService
  ) { }

  private langChangeSubscription: Subscription = new Subscription;

  // Define menu items
  menuItems: MenuItem[] = [];
  adminMenuItems: MenuItem[] = [];
  externalMenuItems: MenuItem[] = [];

  // Initialize signals
  internalUserSignal = signal<boolean>(false);
  adminUserSignal = signal<boolean>(false);
  
  // Initialize menu items in ngOnInit after signals are available
  private initializeMenuItems() {
    this.menuItems = [
      {
        label: 'title.home',
        icon: 'home',
        routerLink: ['/'],
      },
      {
        label: 'title.partnerships',
        icon: 'handshake',
        visible: this.isPartnerOrInternalUser(),
        items: [
          {
            label: 'title.partners',
            icon: 'corporate_fare',
            routerLink: ['/partnerships/partners']
          },
          {
            label: 'title.contacts',
            icon: 'contacts',
            routerLink: ['/partnerships/contacts']
          },
          {
            label: 'title.interactions',
            icon: 'link',
            routerLink: ['/partnerships/interactions']
          },
          {
            label: 'title.partnershipAgreements',
            icon: 'description',
            routerLink: ['/partnerships/partnership-agreements']
          }
        ]
      },
      {
        label: 'title.leads',
        icon: 'trending_up',
        routerLink: ['/leads']
      },
      {
        label: 'title.initiatives',
        icon: 'lightbulb',
        routerLink: ['/initiatives'],
        visible: this.isInternalUser() || this.isAdmin()
      }
    ];

    this.adminMenuItems = [
      {
        label: 'title.admin',
        icon: 'admin_panel_settings',
        visible: this.isAdmin(),
        items: [
          {
            label: 'title.partnerTree',
            icon: 'account_tree',
            routerLink: ['/admin/partner-tree']
          },
          {
            label: 'title.aiPromptsAdmin',
            icon: 'psychology',
            routerLink: ['/admin/ai-prompts']
          },
          {
            label: 'title.manageOffice',
            icon: 'business',
            routerLink: ['/admin/office-management']
          },
          {
            label: 'title.translationWorkbench',
            icon: 'translate',
            routerLink: ['/admin/translations']
          }
        ]
      }
    ];

    this.externalMenuItems = [
      {
        label: 'title.home',
        icon: 'home',
        routerLink: ['/'],
      }
    ];
  }

  get combinedMenuItems(): MenuItem[] {
    return [...this.menuItems, ...this.adminMenuItems];
  }

  ngOnInit() {
    this.authService.isInternal().subscribe((isInternal) => {
      this.internalUserSignal.set(isInternal);
      this.initializeMenuItems();
      this.cdr.detectChanges();
    });

    this.authService.isAdmin().subscribe((isAdmin: boolean) => {
      this.adminUserSignal.set(isAdmin);
      this.initializeMenuItems();
      this.cdr.detectChanges();
    });

    this.langChangeSubscription = this.translateService.onLangChange.subscribe(() => {
      this.cdr.detectChanges();
    });
    
    // Initialize menu items on startup
    this.initializeMenuItems();
  }

  ngOnDestroy(): void {
    this.langChangeSubscription?.unsubscribe();
  }

  private translateMenu(menu: MenuItem[]) {
    for (const item of menu) {
      if (item.label) {
        item.label = this.translateService.instant(item.label);
      }
      if (item.items) {
        this.translateMenu(item.items);
      }
    }
  }

  isAdmin(): boolean {
    if (this.authService.hasDevCookie()) {
      const cookies = document.cookie.split(';').map(c => c.trim());
      const devCookie = cookies.find(c => c.startsWith('dev-user-email='));
      if (devCookie) {
        const email = devCookie.substring('dev-user-email='.length);
        return email.toLowerCase().includes('admin');
      }
    }
    
    return this.adminUserSignal();
  }

  isInternalUser(): boolean {
    if (this.authService.hasDevCookie()) {
      const cookies = document.cookie.split(';').map(c => c.trim());
      const devCookie = cookies.find(c => c.startsWith('dev-user-email='));
      if (devCookie) {
        const email = devCookie.substring('dev-user-email='.length);
        return email.endsWith('@unops.org');
      }
    }
    
    return this.internalUserSignal();
  }

  isPartnerUser(): boolean {
    if (this.authService.hasDevCookie()) {
      const cookies = document.cookie.split(';').map(c => c.trim());
      const devCookie = cookies.find(c => c.startsWith('dev-user-email='));
      if (devCookie) {
        const email = devCookie.substring('dev-user-email='.length);
        return email.includes('partner');
      }
    }
    
    return this.authService.hasRole('Partner') as unknown as boolean;
  }

  isExternalUser(): boolean {
    if (this.authService.hasDevCookie()) {
      const cookies = document.cookie.split(';').map(c => c.trim());
      const devCookie = cookies.find(c => c.startsWith('dev-user-email='));
      if (devCookie) {
        const email = devCookie.substring('dev-user-email='.length);
        return email.includes('example.com');
      }
    }
    
    return this.authService.hasRole('External') as unknown as boolean;
  }
  
  isPartnerOrInternalUser(): boolean {
    return this.isPartnerUser() || this.isInternalUser() || this.isAdmin();
  }
}
