import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  ElementRef,
  effect,
  inject,
  OnDestroy,
  OnInit,
  signal
} from '@angular/core';
import {MenuItem} from 'primeng/api';
import {MenuComponent} from '../menu/menu.component';
import {AuthService} from '@core/services/auth';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {LanguageService} from '@shared/services/utils';
import {Subscription} from 'rxjs/internal/Subscription';
import {CommonModule} from '@angular/common';
import { HttpClientModule} from '@angular/common/http';
import { RouterModule} from '@angular/router';

import {ButtonModule} from 'primeng/button';
import {GlobalFilterService} from '@core/services/filters';
import {LayoutService} from '@layouts/services/layout.service';
import {OpportunitySectionNavService, SectionDefinition} from '@shared/services/ui/opportunity-section-nav.service';

const SECTION_ICON_MAP: Record<string, string> = {
  analysis: 'analytics',
  overview: 'description',
  what: 'work',
  why: 'lightbulb',
  who: 'group',
  where: 'public',
  when: 'calendar_today',
  risks: 'warning',
  related: 'link',
  collaboration: 'forum',
  statement: 'edit_document',
  team: 'apartment',
};

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
  private layoutService = inject(LayoutService);
  private sectionNavService = inject(OpportunitySectionNavService);

  constructor(
    public el: ElementRef,
    private authService: AuthService,
    public languageService: LanguageService,
    private cdr: ChangeDetectorRef,
    private translateService: TranslateService
  ) {
    effect(() => {
      const sections = this.sectionNavService.sections();
      this.updateOpportunitySections(sections);
      this.cdr.markForCheck();
    });
  }

  private langChangeSubscription: Subscription = new Subscription;

  menuItems: MenuItem[] = [];
  adminMenuItems: MenuItem[] = [];
  globalFilterEnabled = signal<boolean>(true);

  private initializeMenuItems(isAdmin: boolean, userRoles: string[] = [], canManageOffice: boolean = false) {
    const opportunityItem = this.buildOpportunityMenuItem(this.sectionNavService.sections());

    this.menuItems = [
      {
        label: 'title.home',
        icon: 'home',
        routerLink: ['/'],
        routerLinkActiveOptions: { exact: true },
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
          opportunityItem,
          {
            label: 'title.opportunitiesAlt',
            icon: 'lightbulb',
            routerLink: ['/partnerships/opportunities-alt']
          },
          {
            label: 'title.partnershipAgreements',
            icon: 'description',
            routerLink: ['/partnerships/partnership-agreements']
          }
        ]
      }
    ];

    if (!isAdmin) {
      this.adminMenuItems = [];
      return;
    }

    const isPartnerGlobAdmin = userRoles.includes('PARTNER_GLOB_ADMIN');
    const isOrgUnitAdmin = userRoles.includes('ORG_UNIT_ADMIN');

    let adminItems: MenuItem[] = [];

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
        },
        {
          label: 'title.entityArtifactManager',
          icon: 'database',
          routerLink: ['/admin/entity-artifacts']
        },
        {
          label: 'title.bulkEntityArtifactUpdate',
          icon: 'upload_file',
          routerLink: ['/admin/bulk-entity-artifacts']
        }
      ];
    }
    else if (isOrgUnitAdmin) {
      adminItems = [
        {
          label: 'title.userManagement',
          icon: 'person',
          routerLink: ['/admin/user-management']
        }
      ];

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

  private buildOpportunityMenuItem(sections: SectionDefinition[]): MenuItem {
    const base: MenuItem = {
      label: 'title.opportunities',
      icon: 'lightbulb',
      routerLink: ['/partnerships/opportunities']
    };

    if (sections.length > 0) {
      base.items = sections.map(section => ({
        label: section.label,
        icon: SECTION_ICON_MAP[section.id] || section.icon,
        state: { sectionId: section.id },
        command: () => {
          this.sectionNavService.requestScrollToSection(section.id);
        }
      }));
    }

    return base;
  }

  private updateOpportunitySections(sections: SectionDefinition[]): void {
    if (!this.menuItems.length) return;

    const partnershipsGroup = this.menuItems.find(item => item.label === 'title.partnerships');
    if (!partnershipsGroup?.items) return;

    const oppIndex = partnershipsGroup.items.findIndex(item => item.label === 'title.opportunities');
    if (oppIndex === -1) return;

    partnershipsGroup.items[oppIndex] = this.buildOpportunityMenuItem(sections);

    this.menuItems = [...this.menuItems];
  }

  closeSidebar(): void {
    this.layoutService.layoutState.update((prev) => ({
      ...prev,
      overlayMenuActive: false,
      staticMenuMobileActive: false,
      menuHoverActive: false
    }));
  }

  ngOnInit() {
    this.globalFilterEnabled.set(this.globalFilterService.isFilterEnabled());

    this.authService.isAdmin().subscribe((isAdmin: boolean) => {
      if (isAdmin) {
        this.authService.getUserRoles().subscribe({
          next: (userRoles) => {
            const canManageOffice = userRoles.includes('PARTNER_GLOB_ADMIN') || userRoles.includes('ORG_UNIT_ADMIN');

            this.initializeMenuItems(isAdmin, userRoles, canManageOffice);
            this.cdr.detectChanges();
          },
          error: (err) => {
            console.error('DEBUG - Error getting user roles from AuthService:', err);
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

    this.initializeMenuItems(false);
  }

  ngOnDestroy(): void {
    this.langChangeSubscription?.unsubscribe();
  }
}
