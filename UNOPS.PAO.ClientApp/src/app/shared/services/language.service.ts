import { Injectable, inject, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { HttpClient } from '@angular/common/http';
import { MenuItem } from 'primeng/api';
import { AuthService } from '@core/services/auth';
import { switchMap, catchError, of, Observable } from 'rxjs';

export interface Language {
  code: string;
  name: string;
}

@Injectable({
  providedIn: 'root'
})
export class LanguageService {
  private languageKey = 'selected_language_cookie';
  languages: Language[] = [];
  private currentLanguageSignal = signal<Language>({ code: 'en', name: 'English' });
  currentLanguage = this.currentLanguageSignal.asReadonly();
  private http = inject(HttpClient);
  private authService = inject(AuthService);

  constructor(public translationService: TranslateService) {
    this.translationService.addLangs(['en', 'fr', 'span', 'pt']);
    this.translationService.setDefaultLang('en');
  }

  initializeLanguage(): Promise<void> {
    return new Promise((resolve) => {
      this.authService.user().pipe(
        switchMap(() => this.http.get<{ language: string }>('/api/global/preferred-language')),
        catchError((error) => {
          // Fall back to localStorage if server call fails
          const savedLanguage = this.getCurrentLanguage();
          return of({ language: savedLanguage.code });
        })
      ).subscribe({
        next: (response) => {
          const preferredLanguage = this.getLanguages().find(lang => lang.code === response.language)
            || { code: 'en', name: 'English' };

          localStorage.setItem(this.languageKey, JSON.stringify(preferredLanguage));
          this.currentLanguageSignal.set(preferredLanguage);
          this.translationService.use(preferredLanguage.code).subscribe(() => {
            resolve();
          });
        },
        error: (error) => {
          const fallbackLanguage = this.getCurrentLanguage();
          this.currentLanguageSignal.set(fallbackLanguage);
          this.translationService.use(fallbackLanguage.code).subscribe(() => {
            resolve();
          });
        }
      });
    });
  }

  getCurrentLanguage(): Language {
    const saved = localStorage.getItem(this.languageKey);
    return saved ? JSON.parse(saved) : { code: 'en', name: 'English' };
  }

  switchLanguage(language: Language) {
    localStorage.setItem(this.languageKey, JSON.stringify(language));
    this.currentLanguageSignal.set(language);
    this.translationService.use(language.code);
    
    // Update user language preference in the database
    this.updatePreferredLanguage(language.code).subscribe({
      next: () => {
      },
      error: (error) => {
        console.error('Error updating user language preference:', error);
        // Continue with local language switching even if API call fails
      }
    });
  }



  private updatePreferredLanguage(languageCode: string): Observable<any> {
    return this.http.put('/api/global/preferred-language', `"${languageCode}"`, {
      headers: { 'Content-Type': 'application/json' }
    }).pipe(
      catchError((error) => {
        console.error('Error updating preferred language:', error);
        return of(null);
      })
    );
  }

  public translateMenuItems(menuItems: MenuItem[]) {
    return menuItems.map(item => ({
      ...item,
      label: item.label ? this.translationService.instant(item.label) : item.label
    }));
  }

  getLanguages(): Language[] {
    const languageNames: { [key: string]: string } = {
      'en': 'English',
      'fr': 'Français',
      'span': 'Español',
      'pt': 'Português'
    };

    return this.translationService
      .getLangs()
      .map((lang) => ({
        name: languageNames[lang] || lang.toUpperCase(),
        code: lang
      }));
  }
}
