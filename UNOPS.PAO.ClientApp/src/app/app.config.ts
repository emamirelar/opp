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
import { FeedbackDialogService } from './common/pages/services/feedback-dialog.service';
import { authInterceptor } from './essentials/interceptors/auth.interceptor';
import { serverErrorInterceptor } from './essentials/interceptors/server-error.interceptor';
import { AuthService } from './essentials/services/auth.service';
import { ConfigurationService } from './essentials/services/configuration.service';

/******* PrimeNG specifc imports *********/
import { providePrimeNG } from 'primeng/config';
//TODO: JW- remove this import
//  import Aura from '@primeng/themes/aura';
import UnopsPreset from './common/themes/unops.preset';
import { routes } from './app.routes';
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
    provideAppInitializer(async () => {
      await inject(ConfigurationService).loadConfig();
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
    DialogService,
    MessageService,
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
