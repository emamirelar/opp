import { Routes } from '@angular/router';
import { authGuard, routePermissionGuard } from '@core/guards';
import { OpportunityListComponent } from '@partnerships/opportunities/components/opportunity/list/opportunity-list.component';
import { OpportunityViewComponent } from '@partnerships/opportunities/components/opportunity/view/opportunity-view.component';
import { OpportunityOption1Component } from '@partnerships/opportunities/components/opportunity/option1-unified/opportunity-option1.component';

export const OPPORTUNITIES_ROUTES: Routes = [
  {
    path: '',
    component: OpportunityListComponent,
    canActivate: [authGuard, routePermissionGuard],
  },
  // Demo UI Options
  {
    path: 'demo/option1',
    component: OpportunityOption1Component,
    canActivate: [authGuard],
  },
  // View existing opportunity with Option 1
  {
    path: ':recordId/view-option1',
    component: OpportunityOption1Component,
    canActivate: [authGuard],
    title: 'Opportunity Dashboard'
  },
  // Regular view
  {
    path: ':recordId',
    component: OpportunityViewComponent,
    canActivate: [authGuard, routePermissionGuard],
  },
];
