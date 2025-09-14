import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  ElementRef,
  inject,
  OnDestroy,
  OnInit,
  signal
} from '@angular/core';
import {MenuItem} from 'primeng/api';
import {MenuComponent} from '../menu/menu.component';
import {AuthService} from '../../../../essentials/services/auth.service';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {LanguageService} from '../../../services/language.service';
import {Subscription} from 'rxjs/internal/Subscription';
import {CommonModule} from '@angular/common';
import { HttpClientModule} from '@angular/common/http';
import { RouterModule} from '@angular/router';

import {ButtonModule} from 'primeng/button';
import {GlobalFilterService} from '@services/global-filter.service';


@Component({
  selector: 'app-sidebar',
  imports: [MenuComponent, CommonModule, HttpClientModule, RouterModule, ButtonModule, TranslateModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss'],
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SidebarComponent implements OnInit, OnDestroy {
  private globalFilterService = inject(GlobalFilterService);


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
  globalFilterEnabled = signal<boolean>(true);

  // Initialize menu items in ngOnInit after signals are available
  private initializeMenuItems(isAdmin: boolean, userRoles: string[] = [], canManageOffice: boolean = false) {
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
            icon: 'chat',
            routerLink: ['/partnerships/interactions']
          },
          // {
          //   label: 'title.partnerTree',
          //   icon: 'account_tree',
          //   routerLink: ['/partnerships/partner-tree']
          // },
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

    if (!isAdmin) {
      this.adminMenuItems = [];
      return;
    }

    // Check user roles to determine which admin items to show
    const isPartnerGlobAdmin = userRoles.includes('PARTNER_GLOB_ADMIN');
    const isOrgUnitAdmin = userRoles.includes('ORG_UNIT_ADMIN');

    let adminItems: MenuItem[] = [];

    // Check if user is PARTNER_GLOB_ADMIN, if yes, add all
    if (isPartnerGlobAdmin) {
      adminItems = [
        {
          label: 'title.partnerTree',
          icon: 'account_tree',
          routerLink: ['/admin/partner-tree']
        },
        {
          label: 'title.aiPromptsAdmin',
          icon: 'psychology',
          routerLink: ['/admin/ai-prompt-management']
        },
        {
          label: 'title.userManagement',
          icon: 'person',
          routerLink: ['/admin/user-management']
        },
        {
          label: 'title.manageOffice',
          icon: 'business',
          routerLink: ['/admin/office-management']
        },
        {
          label: 'title.managerEntities',
          icon: 'settings',
          routerLink: ['/admin/entity-manager']
        },
        {
          label: 'title.translationWorkbench',
          icon: 'translate',
          routerLink: ['/admin/translations']
        }
      ];
    }
    // Check if user is ORG_UNIT_ADMIN (but not PARTNER_GLOB_ADMIN), if yes, add only usermanagement and conditionally office management
    else if (isOrgUnitAdmin) {
      adminItems = [
        {
          label: 'title.userManagement',
          icon: 'person',
          routerLink: ['/admin/user-management']
        }
      ];

      // Add office management if self-management is enabled
      if (canManageOffice) {
        adminItems.push({
          label: 'title.manageOffice',
          icon: 'business',
          routerLink: ['/admin/office-management']
        });
      }
    }

    this.adminMenuItems = adminItems.length > 0 ? [
      {
        label: 'title.admin',
        icon: 'admin_panel_settings',
        items: adminItems
      }
    ] : [];
  }

  ngOnInit() {
    // Initialize global filter state from service
    this.globalFilterEnabled.set(this.globalFilterService.isFilterEnabled());

    this.authService.isAdmin().subscribe((isAdmin: boolean) => {
      if (isAdmin) {
        // Use AuthService getUserRoles instead of API call
        this.authService.getUserRoles().subscribe({
          next: (userRoles) => {
            // Convert roles to match expected format (remove 'PARTNER_' prefix if needed)
            const rolesToCheck = userRoles.map(role => role.replace('PARTNER_', ''));

            // For canManageOffice, we can set a default or derive from roles
            const canManageOffice = userRoles.includes('PARTNER_GLOB_ADMIN') || userRoles.includes('ORG_UNIT_ADMIN');

            this.initializeMenuItems(isAdmin, userRoles, canManageOffice);
            this.cdr.detectChanges();
          },
          error: (err) => {
            console.error('DEBUG - Error getting user roles from AuthService:', err);
            // Fallback to basic admin menu
            this.initializeMenuItems(isAdmin, [], false);
            this.cdr.detectChanges();
          }
        });
      } else {
        this.initializeMenuItems(isAdmin);
        this.cdr.detectChanges();
      }
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
}
