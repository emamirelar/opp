import { Routes } from '@angular/router';
import { LoginComponent } from './common/pages/components/login/login.component';
import { NotFoundComponent } from './common/pages/components/not-found/not-found.component';
import { InternalRoutingModule } from './features/internal/internal-routing.module';
import { ExternalRoutingModule } from './features/external/external-routing.module';

export const routes: Routes = [
  {
    path: '',
    loadChildren: () => InternalRoutingModule,
  },
  {
    path: '',
    loadChildren: () => ExternalRoutingModule,
  },
  { path: 'login', component: LoginComponent },
  { path: 'not-found', component: NotFoundComponent },
  { path: '', redirectTo: 'home', pathMatch: 'full' },
];
