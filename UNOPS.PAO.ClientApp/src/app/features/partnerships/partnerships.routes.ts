import { Routes } from '@angular/router';
import { authGuard, routePermissionGuard } from '@core/guards';
import { ComingSoonComponent } from '@features/static-pages/components/coming-soon/coming-soon.component';

export const PARTNERSHIPS_ROUTES: Routes = [
  {
    path: 'contacts',
    loadChildren: () => import('@partnerships/contacts/contacts.routes').then(m => m.CONTACTS_ROUTES),
    canActivate: [authGuard, routePermissionGuard],
    data: { breadcrumb: 'Contacts' }
  },
  {
    path: 'interactions',
    loadChildren: () => import('@partnerships/interactions/interactions.routes').then(m => m.INTERACTIONS_ROUTES),
    canActivate: [authGuard, routePermissionGuard],
    data: { breadcrumb: 'Interactions' }
  },
  {
    path: 'partners',
    loadChildren: () => import('@partnerships/partners/partners.routes').then(m => m.PARTNERS_ROUTES),
    canActivate: [authGuard, routePermissionGuard],
    data: { breadcrumb: 'Partners' }
  },
  {
    path: 'opportunities-alt',
    loadChildren: () => import('@partnerships/opportunities-alt/opportunities-alt.routes').then(m => m.OPPORTUNITIES_ALT_ROUTES),
    canActivate: [authGuard],
    data: { breadcrumb: 'Opportunities Alt' }
  },
  {
    path: 'opportunities',
    loadChildren: () => import('@partnerships/opportunities/opportunities.routes').then(m => m.OPPORTUNITIES_ROUTES),
    canActivate: [authGuard, routePermissionGuard],
    data: { breadcrumb: 'Opportunities' }
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
];

