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
import { MarkdownModule } from 'ngx-markdown';
import { SecurityContext } from '@angular/core';

/******* Services *********/

import { MessageService } from 'primeng/api';
import { DialogService } from 'primeng/dynamicdialog';
import { ConfirmationService } from 'primeng/api';
import { FeedbackDialogService } from '@shared/services/feedback-dialog.service';
import { authInterceptor } from '@core/interceptors/auth.interceptor';
import { serverErrorInterceptor } from '@core/interceptors/server-error.interceptor';
import { AuthService } from '@core/services/auth.service';
import { ConfigurationService } from '@core/services/configuration.service';
import { HasPermissionDirective } from '@core/directives/has-permission.directive';
import { PermissionService } from '@core/services/permission.service';
import { LanguageService } from '@shared/services/language.service';

/******* PrimeNG specifc imports *********/
import { providePrimeNG } from 'primeng/config';
//TODO: JW- remove this import
//  import Aura from '@primeng/themes/aura';
import UnopsPreset from '../styles/themes/unops.preset';
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
    // Simple config loading initializer
    provideAppInitializer(() => {
      const configService = inject(ConfigurationService);
      return configService.loadConfig();
    }),
    // Language initialization - load preferred language before app starts
    provideAppInitializer(() => {
      const languageService = inject(LanguageService);
      return languageService.initializeLanguage();
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
      MarkdownModule.forRoot({
        sanitize: SecurityContext.HTML,
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
          colorScheme: 'light',
          darkModeSelector: '.fake-dark-mode',
        },
      },
    }),
    FeedbackDialogService
  ],
};
