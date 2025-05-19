import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LayoutComponent } from '../../common/layouts/components/layout/layout.component';
import { HomeComponent } from '../../common/pages/components/home/home.component';
import { authGuard } from '../../essentials/guards/auth.guard';
import { InteractionListComponent } from './components/interaction/list/interaction-list.component';
import { PartnerTreeComponent } from './components/partner-tree/partner-tree.component';
import { PartnerComponent } from './components/partner/partner.component';
import { PartnerViewComponent } from './components/partner/view/partner-view.component';
import { PartnerDataComponent } from './components/partner/data/partner-data.component';
import { ContactListComponent } from './components/contact/list/contact-list.component';
import { ContactViewComponent } from './components/contact/view/contact-view.component';
import { PartnerTabsComponent } from './components/partner/tabs/partner-tabs.component';
import { ComingSoonComponent } from '../../common/pages/components/coming-soon/coming-soon.component';
import { adminGuard } from '../../essentials/guards/admin.guard';
import { SearchResultComponent } from './components/search-result/search-result.component';
import { PartnerTreeViewComponent } from './components/partner-tree/view/partner-tree-view.component';
import { PartnerDataResolver } from './resolvers/partner-data.resolver';
import { PartnerTreeDataResolver } from './resolvers/partner-tree-data.resolver';

const internalRoutes: Routes = [
  {
    path: '',
    component: LayoutComponent,
    children: [
      {
        path: '',
        component: HomeComponent,
        canActivate: [authGuard],
        data: { breadcrumb: 'Home', icon: 'pi pi-home' },
      },
      {
        path: 'search',
        component: SearchResultComponent,
        canActivate: [authGuard],
        data: { breadcrumb: 'Search' }
      },
      {
        path: 'partnerships',
        canActivate: [authGuard],
        data: { breadcrumb: 'Partnerships' },
        children: [
          {
            path: 'contacts',
            component: ContactListComponent,
            canActivate: [authGuard],
            data: { breadcrumb: 'Contacts' }
          },
          {
            path: 'contacts/:recordId',
            data: { breadcrumb: 'Contact' },
            component: ContactViewComponent,
            canActivate: [authGuard],
          },
          {
            path: 'interactions',
            canActivate: [authGuard],
            data: { breadcrumb: 'Interactions' },
            children: [
              { path: '', component: InteractionListComponent },
              { path: ':id', component: InteractionListComponent, data: { breadcrumb: 'Edit' } }
            ]
          },
          {
            path: 'partners',
            component: PartnerComponent,
            canActivate: [authGuard],
            data: { breadcrumb: 'Partners' }
          },
          {
            path: 'partners/:recordId',
            component: PartnerTabsComponent,
            canActivate: [authGuard],
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
              }
            ]
          },
          {
            path: 'partnership-agreements',
            component: ComingSoonComponent,
            canActivate: [authGuard],
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
        canActivate: [authGuard],
        data: { 
          breadcrumb: 'Leads',
          featureName: 'Leads'
        }
      },
      {
        path: 'initiatives',
        component: ComingSoonComponent,
        canActivate: [authGuard],
        data: { 
          breadcrumb: 'Initiatives',
          featureName: 'Initiatives'
        }
      },
      // Admin routes
      {
        path: 'admin',
        canActivate: [authGuard, adminGuard],
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
                }
              }
            ]
          },
          {
            path: 'ai-prompts',
            component: ComingSoonComponent,
            data: { 
              breadcrumb: 'AI Prompts Admin',
              featureName: 'AI Prompts Admin'
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
export class InternalRoutingModule {}
