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
