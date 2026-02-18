import { Routes } from '@angular/router';
import { LoginComponent } from '@features/auth/components/login/login.component';
import { NotFoundComponent } from '@features/static-pages/components/not-found/not-found.component';
import { AccessDeniedComponent } from '@features/static-pages/components/access-denied/access-denied.component';
import { LayoutComponent } from '@layouts/components/layout/layout.component';
import { HomeComponent } from '@features/home/components/home/home.component';
import { authGuard, routePermissionGuard } from '@core/guards';
import { ComingSoonComponent } from '@features/static-pages/components/coming-soon/coming-soon.component';
import { SearchResultComponent } from '@search/components/search-result/search-result.component';

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
        loadChildren: () => import('@partnerships/partnerships.routes').then(m => m.PARTNERSHIPS_ROUTES),
        canActivate: [authGuard, routePermissionGuard],
        data: { breadcrumb: 'Partnerships' }
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
        loadChildren: () => import('@admin/admin.routes').then(m => m.ADMIN_ROUTES),
        canActivate: [authGuard, routePermissionGuard],
        data: { breadcrumb: 'Admin' }
      },
      {
        path: 'ai',
        loadComponent: () => import('@ai/components/ai-content/ai-content.component').then(m => m.AiContentComponent),
        canActivate: [authGuard],
        data: { breadcrumb: 'AI Assistant', icon: 'pi pi-sparkles', routeId: 'ai-route' }
      },
      {
        path: 'ai/:sessionId',
        loadComponent: () => import('@ai/components/ai-content/ai-content.component').then(m => m.AiContentComponent),
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
