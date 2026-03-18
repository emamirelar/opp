import { Routes } from '@angular/router';
import { authGuard } from '@core/guards';
import { OpportunityAltComponent } from './opportunity-alt.component';

export const OPPORTUNITIES_ALT_ROUTES: Routes = [
  {
    path: '',
    component: OpportunityAltComponent,
    canActivate: [authGuard],
  },
  {
    path: ':recordId',
    component: OpportunityAltComponent,
    canActivate: [authGuard],
  },
  {
    path: ':recordId/:section',
    component: OpportunityAltComponent,
    canActivate: [authGuard],
  },
];
