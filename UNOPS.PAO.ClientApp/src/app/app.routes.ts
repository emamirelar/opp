import { Routes } from '@angular/router';
import { LoginComponent } from '@features/auth/components/login/login.component';
import { NotFoundComponent } from '@features/static-pages/components/not-found/not-found.component';
import { AccessDeniedComponent } from '@features/static-pages/components/access-denied/access-denied.component';
import { LayoutComponent } from '@layouts/components/layout/layout.component';
import { HomeComponent } from '@features/home/components/home/home.component';
import { authGuard, adminGuard, routePermissionGuard } from '@core/guards';
import { InteractionListComponent } from '@partnerships/interactions/components/interaction/list/interaction-list.component';
import { InteractionDetailComponent } from '@partnerships/interactions/components/interaction/detail/interaction-detail.component';
import { PartnerTreeComponent } from '@partnerships/partners/components/partner-tree/partner-tree.component';
import { PartnerComponent } from '@partnerships/partners/components/partner/partner.component';
import { PartnerViewComponent } from '@partnerships/partners/components/partner/view/partner-view.component';
import { PartnerDataComponent } from '@partnerships/partners/components/partner/data/partner-data.component';
import { ContactListComponent } from '@partnerships/contacts/components/contact/list/contact-list.component';
import { ContactViewComponent } from '@partnerships/contacts/components/contact/view/contact-view.component';
import { ContactTabsComponent } from '@partnerships/contacts/components/contact/tabs/contact-tabs.component';
import { PartnerTabsComponent } from '@partnerships/partners/components/partner/tabs/partner-tabs.component';
import { ComingSoonComponent } from '@features/static-pages/components/coming-soon/coming-soon.component';
import { SearchResultComponent } from '@search/components/search-result/search-result.component';
import { PartnerTreeViewComponent } from '@partnerships/partners/components/partner-tree/view/partner-tree-view.component';
import { PartnerTreeDetailsComponent } from '@partnerships/partners/components/partner-tree/view/details/partner-tree-details.component';
import { PartnerTreeDataComponent } from '@partnerships/partners/components/partner-tree/view/data/partner-tree-data.component';
import { PartnerDataResolver } from '@partnerships/partners/resolvers/partner-data.resolver';
import { PartnerTreeDataResolver } from '@partnerships/partners/resolvers/partner-tree-data.resolver';
import { ContactDataResolver } from '@partnerships/contacts/resolvers/contact-data.resolver';
import { UserManagementComponent } from '@admin/user-management/user-management.component';
import { EntityManagerComponent } from '@admin/entity-manager/entity-manager.component';
import { TranslationWorkbenchComponent } from './features/admin/translation-workbench/translation-workbench.component';
import { PartnerTreePageComponent } from '@partnerships/partners/components/partner-tree-page/partner-tree-page.component';
import {
  ContactViewInteractionsComponent
} from '@partnerships/contacts/components/contact/view/interactions/contact-view-interactions.component';

export const routes: Routes = [
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
              { path: ':id', component: InteractionDetailComponent, data: { breadcrumb: 'Interaction Details' } }
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
            loadComponent: () => import('@ai/components/ai-prompt/ai-prompt.component').then(m => m.AiPromptComponent),
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
            component: TranslationWorkbenchComponent,
            data: {
              breadcrumb: 'Translation Workbench'
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
      },
      {
        path: 'ai',
        loadComponent: () => import('@ai/ai-content.component').then(m => m.AiContentComponent),
        canActivate: [authGuard],
        data: { breadcrumb: 'AI Assistant', icon: 'pi pi-sparkles', routeId: 'ai-route' }
      },
      {
        path: 'ai/:sessionId',
        loadComponent: () => import('@ai/ai-content.component').then(m => m.AiContentComponent),
        canActivate: [authGuard],
        data: { breadcrumb: 'AI Assistant', icon: 'pi pi-sparkles', routeId: 'ai-session-route' }
      }
    ],
  },

  { path: 'login', component: LoginComponent },
  { path: 'not-found', component: NotFoundComponent },
  { path: 'access-denied', component: AccessDeniedComponent },
  { path: '', redirectTo: 'home', pathMatch: 'full' },
];
