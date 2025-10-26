import { Routes } from '@angular/router';
import { authGuard, routePermissionGuard } from '@core/guards';
import { OpportunityListComponent } from '@partnerships/opportunities/components/opportunity/list/opportunity-list.component';
import { OpportunityViewComponent } from '@partnerships/opportunities/components/opportunity/view/opportunity-view.component';
import { OpportunityOption1Component } from '@partnerships/opportunities/components/opportunity/option1-unified/opportunity-option1.component';
import { OpportunityOption2Component } from '@partnerships/opportunities/components/opportunity/option2-tabbed/opportunity-option2.component';
import { OpportunityOption3Component } from '@partnerships/opportunities/components/opportunity/option3-wizard/opportunity-option3.component';
import { OpportunityUnifiedCreateComponent } from '@partnerships/opportunities/components/opportunity/unified-create/opportunity-unified-create.component';

export const OPPORTUNITIES_ROUTES: Routes = [
  {
    path: '',
    component: OpportunityListComponent,
    canActivate: [authGuard, routePermissionGuard],
  },
  // Create new opportunity
  {
    path: 'create',
    component: OpportunityUnifiedCreateComponent,
    canActivate: [authGuard],
    title: 'Create New Opportunity'
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
