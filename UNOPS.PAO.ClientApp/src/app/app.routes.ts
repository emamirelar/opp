import { Routes } from '@angular/router';
import { LoginComponent } from './common/pages/components/login/login.component';
import { NotFoundComponent } from './common/pages/components/not-found/not-found.component';
import { AccessDeniedComponent } from './common/pages/components/access-denied/access-denied.component';
import { InternalRoutingModule } from './features/internal/internal-routing.module';

export const routes: Routes = [
  {
    path: '',
    loadChildren: () => InternalRoutingModule,
  },
  { path: 'login', component: LoginComponent },
  { path: 'not-found', component: NotFoundComponent },
  { path: 'access-denied', component: AccessDeniedComponent },
  { path: '', redirectTo: 'home', pathMatch: 'full' },
];
