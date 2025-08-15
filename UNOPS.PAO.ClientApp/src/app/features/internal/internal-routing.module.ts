import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LayoutComponent } from '../../common/layouts/components/layout/layout.component';
import { HomeComponent } from '../../common/pages/components/home/home.component';
import { authGuard, adminGuard, routePermissionGuard } from '../../essentials/guards';
import { InteractionListComponent } from './components/interaction/list/interaction-list.component';
import { PartnerTreeComponent } from './components/partner-tree/partner-tree.component';
import { PartnerComponent } from './components/partner/partner.component';
import { PartnerViewComponent } from './components/partner/view/partner-view.component';
import { PartnerDataComponent } from './components/partner/data/partner-data.component';
import { ContactListComponent } from './components/contact/list/contact-list.component';
import { ContactViewComponent } from './components/contact/view/contact-view.component';
import { ContactTabsComponent } from './components/contact/tabs/contact-tabs.component';
import { PartnerTabsComponent } from './components/partner/tabs/partner-tabs.component';
import { ComingSoonComponent } from '../../common/pages/components/coming-soon/coming-soon.component';
import { SearchResultComponent } from './components/search-result/search-result.component';
import { PartnerTreeViewComponent } from './components/partner-tree/view/partner-tree-view.component';
import { PartnerTreeDetailsComponent } from './components/partner-tree/view/details/partner-tree-details.component';
import { PartnerTreeDataComponent } from './components/partner-tree/view/data/partner-tree-data.component';
import { PartnerDataResolver } from './resolvers/partner-data.resolver';
import { PartnerTreeDataResolver } from './resolvers/partner-tree-data.resolver';
import { ContactDataResolver } from './resolvers/contact-data.resolver';
import { UserManagementComponent } from './admin/user-management/user-management.component';
import { EntityManagerComponent } from './admin/entity-manager/entity-manager.component';
import { PartnerTreePageComponent } from './components/partner-tree-page/partner-tree-page.component';
import {
  ContactViewInteractionsComponent
} from '@features/internal/components/contact/view/interactions/contact-view-interactions.component';

const internalRoutes: Routes = [
  {
    path: '',
    component: LayoutComponent,
    data: { routeId: 'main-layout' },
    children: [
      {
        path: '',
        component: HomeComponent,
        // Home is visible to all users - no need for role guard
        canActivate: [authGuard],
        data: { breadcrumb: 'Home', icon: 'pi pi-home', routeId: 'home-route' },
      },
      {
        path: 'search',
        component: SearchResultComponent,
        canActivate: [authGuard],
        data: { breadcrumb: 'Search' }
      },
      {
        path: 'partnerships',
        canActivate: [authGuard, routePermissionGuard],
        data: { breadcrumb: 'Partnerships' },
        children: [
          {
            path: 'contacts',
            canActivate: [authGuard, routePermissionGuard],
            data: { breadcrumb: 'Contacts' },
            children: [
              { path: '', component: ContactListComponent },
              {
                path: ':recordId',
                component: ContactTabsComponent,
                canActivate: [authGuard, routePermissionGuard],
                data: { breadcrumb: 'Contact' },
                resolve: {
                  contactData: ContactDataResolver
                },
                children: [
                  {
                    path: '',
                    component: ContactViewComponent,
                    data: { breadcrumb: 'Details' }
                  },
                  {
                    path: 'interactions',
                    component: ContactViewInteractionsComponent,
                    data: { breadcrumb: 'Interactions' }
                  }
                ]
              },
            ]
          },

          {
            path: 'interactions',
            canActivate: [authGuard, routePermissionGuard],
            data: { breadcrumb: 'Interactions' },
            children: [
              { path: '', component: InteractionListComponent },
              { path: ':id', component: InteractionListComponent, data: { breadcrumb: 'Edit' } }
            ]
          },
          {
            path: 'partner-tree',
            data: { breadcrumb: 'Partner Tree' },
            component: PartnerTreePageComponent,
            canActivate: [authGuard, routePermissionGuard]
          },
          {
            path: 'partners',
            component: PartnerComponent,
            canActivate: [authGuard, routePermissionGuard],
            data: { breadcrumb: 'Partners' }
          },
          {
            path: 'partners/:recordId',
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
                path: 'contacts',
                loadComponent: () => import('./components/partner/contacts/partner-contacts.component').then(m => m.PartnerContactsComponent),
                data: { breadcrumb: 'Contacts' }
              },
              {
                path: 'interactions',
                loadComponent: () => import('./components/partner/view/interactions/partner-view-interactions.component').then(m => m.PartnerViewInteractionsComponent),
                data: { breadcrumb: 'Interactions' }
              },
              {
                path: 'funding-agreements',
                loadComponent: () => import('./components/partner/funding-agreements/partner-funding-agreements.component').then(m => m.PartnerFundingAgreementsComponent),
                data: { breadcrumb: 'Funding & Agreements' }
              }
            ]
          },
          {
            path: 'partnership-agreements',
            component: ComingSoonComponent,
            canActivate: [authGuard, routePermissionGuard],
            data: {
              breadcrumb: 'Partnership Agreements',
              featureName: 'Partnership Agreements'
            }
          }
        ]
      },
      {
        path: 'leads',
        component: ComingSoonComponent,
        data: {
          breadcrumb: 'Leads',
          featureName: 'Leads'
        }
      },
      {
        path: 'initiatives',
        component: ComingSoonComponent,
        data: {
          breadcrumb: 'Initiatives',
          featureName: 'Initiatives'
        }
      },
      // Admin routes
      {
        path: 'admin',
        // canActivate: [authGuard, adminGuard],
        canActivate: [authGuard],
        data: { breadcrumb: 'Admin' },
        children: [
          {
            path: 'partner-tree',
            children: [
              { path: '', component: PartnerTreeComponent, data: { breadcrumb: 'Partner Tree' } },
              {
                path: ':recordId',
                component: PartnerTreeViewComponent,
                data: { breadcrumb: 'Partner Tree View' },
                resolve: {
                  partnerTreeData: PartnerTreeDataResolver
                },
                children: [
                  {
                    path: '',
                    component: PartnerTreeDetailsComponent,
                    data: { breadcrumb: 'Details' }
                  },
                  {
                    path: 'data',
                    component: PartnerTreeDataComponent,
                    data: { breadcrumb: 'Data' }
                  }
                ]
              }
            ]
          },
          {
            path: 'ai-prompt-management',
            loadComponent: () => import('./components/ai-prompt/ai-prompt.component').then(m => m.AiPromptComponent),
            data: {
              breadcrumb: 'AI Prompt Admin'
            }
          },
          {
            path: 'user-management',
            component: UserManagementComponent,
            data: {
              breadcrumb: 'Manage User Permissions'
            }
          },
          {
            path: 'office-management',
            component: ComingSoonComponent,
            data: {
              breadcrumb: 'Manage my Office',
              featureName: 'Manage my Office'
            }
          },
          {
            path: 'entity-manager',
            component: EntityManagerComponent,
            data: {
              breadcrumb: 'Manage Entities'
            }
          },
          {
            path: 'translations',
            component: ComingSoonComponent,
            data: {
              breadcrumb: 'Translation Workbench',
              featureName: 'Translation Workbench'
            }
          }
        ]
      },
      // Legacy routes for backward compatibility - redirect to new structure
      {
        path: 'contacts',
        redirectTo: 'partnerships/contacts',
        pathMatch: 'full'
      },
      {
        path: 'contact/:recordId',
        redirectTo: 'partnerships/contacts/:recordId',
        pathMatch: 'prefix',
      },
      {
        path: 'interactions',
        redirectTo: 'partnerships/interactions',
        pathMatch: 'full'
      },
      {
        path: 'partner-tree',

        redirectTo: 'partnerships/partner-tree',
        pathMatch: 'full'
      },
      {
        path: 'partners',
        redirectTo: 'partnerships/partners',
        pathMatch: 'full'
      },
      {
        path: 'partner/:recordId',
        redirectTo: 'partnerships/partners/:recordId',
        pathMatch: 'prefix'
      }
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(internalRoutes)],
  exports: [RouterModule],
})
export class InternalRoutingModule {
  constructor() {

  }
}
