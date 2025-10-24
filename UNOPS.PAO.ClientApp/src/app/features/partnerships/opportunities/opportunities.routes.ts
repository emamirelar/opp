import { Routes } from '@angular/router';
import { authGuard, routePermissionGuard } from '@core/guards';
import { OpportunityListComponent } from '@partnerships/opportunities/components/opportunity/list/opportunity-list.component';
import { OpportunityViewComponent } from '@partnerships/opportunities/components/opportunity/view/opportunity-view.component';
import { OpportunityOption1Component } from '@partnerships/opportunities/components/opportunity/option1-unified/opportunity-option1.component';
import { OpportunityOption2Component } from '@partnerships/opportunities/components/opportunity/option2-tabbed/opportunity-option2.component';
import { OpportunityOption3Component } from '@partnerships/opportunities/components/opportunity/option3-wizard/opportunity-option3.component';

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
  {
    path: 'demo/option2',
    component: OpportunityOption2Component,
    canActivate: [authGuard],
  },
  {
    path: 'demo/option3',
    component: OpportunityOption3Component,
    canActivate: [authGuard],
  },
  // Regular view
  {
    path: ':recordId',
    component: OpportunityViewComponent,
    canActivate: [authGuard, routePermissionGuard],
  },
];
