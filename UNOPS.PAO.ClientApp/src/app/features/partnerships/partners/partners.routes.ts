import { Routes } from '@angular/router';
import { authGuard, routePermissionGuard } from '@core/guards';
import { PartnerTreeComponent } from '@partnerships/partners/components/partner-tree/partner-tree.component';
import { PartnerComponent } from '@partnerships/partners/components/partner/partner.component';
import { PartnerViewComponent } from '@partnerships/partners/components/partner/view/partner-view.component';
import { PartnerDataComponent } from '@partnerships/partners/components/partner/data/partner-data.component';
import { PartnerTabsComponent } from '@partnerships/partners/components/partner/tabs/partner-tabs.component';
import { PartnerTreeViewComponent } from '@partnerships/partners/components/partner-tree/view/partner-tree-view.component';
import { PartnerTreeDetailsComponent } from '@partnerships/partners/components/partner-tree/view/details/partner-tree-details.component';
import { PartnerTreeDataComponent } from '@partnerships/partners/components/partner-tree/view/data/partner-tree-data.component';
import { PartnerDataResolver } from '@partnerships/partners/resolvers/partner-data.resolver';
import { PartnerTreePageComponent } from '@partnerships/partners/components/partner-tree-page/partner-tree-page.component';

export const PARTNERS_ROUTES: Routes = [
  {
    path: 'partner-tree',
    data: { breadcrumb: 'Partner Tree' },
    component: PartnerTreePageComponent,
    canActivate: [authGuard, routePermissionGuard]
  },
  {
    path: '',
    component: PartnerComponent,
    canActivate: [authGuard, routePermissionGuard],
    data: { breadcrumb: 'Partners' }
  },
  {
    path: ':recordId',
    component: PartnerTabsComponent,
    canActivate: [authGuard, routePermissionGuard],
    data: { breadcrumb: 'Partner' },
    resolve: {
      partnerData: PartnerDataResolver
    },
    children: [
      {
        path: '',
        component: PartnerViewComponent,
        data: { breadcrumb: 'Details' }
      },
      {
        path: 'data',
        component: PartnerDataComponent,
        data: { breadcrumb: 'Data' }
      },
      {
        path: 'opportunities',
        loadComponent: () => import('@partnerships/partners/components/partner/view/opportunities/partner-view-opportunities.component').then(m => m.PartnerViewOpportunitiesComponent),
        data: { breadcrumb: 'Opportunities' }
      },
      {
        path: 'contacts',
        loadComponent: () => import('@partnerships/partners/components/partner/contacts/partner-contacts.component').then(m => m.PartnerContactsComponent),
        data: { breadcrumb: 'Contacts' }
      },
      {
        path: 'interactions',
        loadComponent: () => import('@partnerships/partners/components/partner/view/interactions/partner-view-interactions.component').then(m => m.PartnerViewInteractionsComponent),
        data: { breadcrumb: 'Interactions' }
      },
      {
        path: 'funding-agreements',
        loadComponent: () => import('@partnerships/partners/components/partner/funding-agreements/partner-funding-agreements.component').then(m => m.PartnerFundingAgreementsComponent),
        data: { breadcrumb: 'Funding & Agreements' }
      }
    ]
  }
];

