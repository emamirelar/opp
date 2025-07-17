import { Injectable, inject } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { HttpClient } from '@angular/common/http';
import { MenuItem } from 'primeng/api';
import { AuthService } from '../../essentials/services/auth.service';
import { switchMap, catchError, of, Observable, tap } from 'rxjs';

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
  currentLanguage: Language;
  private http = inject(HttpClient);
  private authService = inject(AuthService);

  constructor(public translationService: TranslateService) {
    this.translationService.addLangs(['en', 'fr', 'span', 'pt']);
    this.translationService.setDefaultLang('en');
    this.currentLanguage = { code: 'en', name: 'EN' }; // Temporary until server responds
  }

  initializeLanguage(): Promise<void> {
    return new Promise((resolve) => {
      this.authService.user().pipe(
        switchMap(() => this.http.get<{ language: string }>('/api/global/preferred-language')),
        catchError((error) => {
          console.log('Could not load preferred language, falling back to localStorage:', error);
          // Fall back to localStorage if server call fails
          const savedLanguage = this.getCurrentLanguage();
          return of({ language: savedLanguage.code });
        })
      ).subscribe({
        next: (response) => {
          const preferredLanguage = this.getLanguages().find(lang => lang.code === response.language) 
            || { code: 'en', name: 'EN' };
          
          // Set the language from server (or localStorage fallback)
          localStorage.setItem(this.languageKey, JSON.stringify(preferredLanguage));
          this.currentLanguage = preferredLanguage;
          this.translationService.use(preferredLanguage.code).subscribe(() => {
            console.log('Language initialized to:', preferredLanguage.code);
            resolve();
          });
        },
        error: (error) => {
          console.log('Error loading preferred language:', error);
          // Final fallback to localStorage
          const fallbackLanguage = this.getCurrentLanguage();
          this.currentLanguage = fallbackLanguage;
          this.translationService.use(fallbackLanguage.code).subscribe(() => {
            console.log('Language initialized to fallback:', fallbackLanguage.code);
            resolve();
          });
        }
      });
    });
  }

  getCurrentLanguage(): Language {
    const saved = localStorage.getItem(this.languageKey);
    return saved ? JSON.parse(saved) : { code: 'en', name: 'EN' };
  }

  switchLanguage(language: Language) {
    localStorage.setItem(this.languageKey, JSON.stringify(language));
    this.currentLanguage = language;
    this.translationService.use(language.code);
    
    // Update user language preference in the database
    this.updatePreferredLanguage(language.code).subscribe({
      next: () => {
        console.log('User language preference updated successfully');
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
    return this.translationService
      .getLangs()
      .map((lang) => ({ name: lang.toUpperCase(), code: lang,  }));
  }
}
