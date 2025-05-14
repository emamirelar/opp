import {
  ApplicationConfig,
  importProvidersFrom,
  inject,
  provideAppInitializer,
  provideZoneChangeDetection,
} from '@angular/core';
import {
  provideRouter,
  withComponentInputBinding,
  withHashLocation,
  withInMemoryScrolling,
  Router,
} from '@angular/router';

import {
  HttpClient,
  provideHttpClient,
  withInterceptors,
} from '@angular/common/http';

import { GoogleLoginProvider } from '@abacritt/angularx-social-login';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';

/******* Services *********/

import { MessageService } from 'primeng/api';
import { DialogService } from 'primeng/dynamicdialog';
import { ConfirmationService } from 'primeng/api';
import { FeedbackDialogService } from './common/pages/services/feedback-dialog.service';
import { authInterceptor } from './essentials/interceptors/auth.interceptor';
import { serverErrorInterceptor } from './essentials/interceptors/server-error.interceptor';
import { AuthService } from './essentials/services/auth.service';
import { ConfigurationService } from './essentials/services/configuration.service';
import { HasPermissionDirective } from './essentials/directives/has-permission.directive';
import { PermissionService } from './essentials/services/permission.service';

/******* PrimeNG specifc imports *********/
import { providePrimeNG } from 'primeng/config';
//TODO: JW- remove this import
//  import Aura from '@primeng/themes/aura';
import UnopsPreset from './common/themes/unops.preset';
import { routes } from './app.routes';
import { firstValueFrom } from 'rxjs';
/********************************/
const httpLoaderFactory: (http: HttpClient) => TranslateHttpLoader = (
  http: HttpClient,
) => new TranslateHttpLoader(http);

const socialAuthConfigFactory = (configService: ConfigurationService) => {
  return {
    autoLogin: false,
    providers: [
      {
        id: GoogleLoginProvider.PROVIDER_ID,
        provider: new GoogleLoginProvider(
          configService.getConfig().googleClientId,
        ),
      },
    ],
  };
};

/**
 * Role-Based Access Control (RBAC) Implementation
 * 
 * The application uses a centralized permission configuration in permissions.json 
 * that is shared between frontend and backend.
 * 
 * The roles in the system are:
 * - Administrator: Can access all sections and features (admin@unops.org)
 * - Internal: UNOPS staff with access to Partnerships and Initiatives (anushas@unops.org)
 * - Partner: External partners with access to Partnerships (devuser@partner.com)
 * - External: External users with access only to Leads (devuser@example.com)
 * 
 * Access control is implemented at both levels:
 * 1. Both frontend and backend check the same permission configuration
 * 2. Backend enforces permissions at the API level
 * 3. Frontend adapts UI based on permissions
 */

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(
      routes, 
      withHashLocation(), 
      withInMemoryScrolling({ anchorScrolling: 'enabled', scrollPositionRestoration: 'enabled' }),
      withComponentInputBinding()
    ),
    // Config loading initializer only - removed IAP check to prevent repeated calls
    provideAppInitializer(async () => {
      await inject(ConfigurationService).loadConfig();
      console.log('[DEBUG-INIT] Config loaded');
      
      // Also load permissions during initialization
      try {
        const permissionService = inject(PermissionService);
        await firstValueFrom(permissionService.loadConfig());
        console.log('[DEBUG-INIT] Permissions loaded');
      } catch (error) {
        console.error('[DEBUG-INIT] Error loading permissions', error);
      }
    }),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideHttpClient(
      withInterceptors([authInterceptor, serverErrorInterceptor]),
    ),
    ConfigurationService,
    {
      provide: 'SocialAuthServiceConfig',
      useFactory: socialAuthConfigFactory,
      deps: [ConfigurationService],
    },
    importProvidersFrom(
      [
      TranslateModule.forRoot({
        loader: {
          provide: TranslateLoader,
          useFactory: httpLoaderFactory,
          deps: [HttpClient],
        },
      }),
    ]),
    provideAnimationsAsync(),
    AuthService,
    PermissionService,
    DialogService,
    MessageService,
    ConfirmationService,
    HasPermissionDirective,
    providePrimeNG({
      theme: {
        preset: UnopsPreset,
        options: {
          darkModeSelector: '.app-dark',
          ripple: true,
          animations: true,
          typography: true,
          colors: true,
          shape: true,
          spacing: true,
          elevation: true,
          transitions: true,
          breakpoints: true,
          zIndex: true,
          rtl: false,
          ltr: true,
        },
      },
    }),
    FeedbackDialogService
  ],
};
