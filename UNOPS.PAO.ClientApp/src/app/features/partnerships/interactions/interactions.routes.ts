import { Routes } from '@angular/router';
import { InteractionListComponent } from '@partnerships/interactions/components/interaction/list/interaction-list.component';
import { InteractionDetailComponent } from '@partnerships/interactions/components/interaction/detail/interaction-detail.component';

export const INTERACTIONS_ROUTES: Routes = [
  { path: '', component: InteractionListComponent },
  { path: ':id', component: InteractionDetailComponent, data: { breadcrumb: 'Interaction Details' } }
];

