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
  restrictedRoleSignal = signal<boolean>(false);
  
  // Initialize menu items in ngOnInit after signals are available
  private initializeMenuItems(isAdmin: boolean) {
    this.menuItems = [
      {
        label: 'title.home',
        icon: 'home',
        routerLink: ['/'],
      },
      {
        label: 'title.partnerships',
        icon: 'handshake',
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
        routerLink: ['/initiatives']
      }
    ];

    this.adminMenuItems = !isAdmin ? [] : [
      {
        label: 'title.admin',
        icon: 'admin_panel_settings',
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
  }

  get combinedMenuItems(): MenuItem[] {
    return [...this.menuItems, ...this.adminMenuItems];
  }

  ngOnInit() {
    this.authService.isAdmin().subscribe((isAdmin: boolean) => {
      this.initializeMenuItems(isAdmin);
      this.cdr.detectChanges();
    });

    this.langChangeSubscription = this.translateService.onLangChange.subscribe(() => {
      this.cdr.detectChanges();
    });
    
    // Initialize menu items on startup
    this.initializeMenuItems(false);
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
}
