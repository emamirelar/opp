import { Injectable, inject } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { MenuItem } from 'primeng/api';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../essentials/services/auth.service';
import { catchError, switchMap } from 'rxjs/operators';
import { of } from 'rxjs';

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
    this.currentLanguage = this.getCurrentLanguage();
    this.translationService.setDefaultLang(this.currentLanguage.code);

    this.translationService.use(this.currentLanguage.code);
  }

  getCurrentLanguage(): Language {
    const defaultLanguage = {code: 'en', name: 'en'};
    const cookieLanguage = localStorage.getItem(this.languageKey);
    return cookieLanguage ? JSON.parse(cookieLanguage) : defaultLanguage;
  }

  switchLanguage(language: Language) {
    localStorage.setItem(this.languageKey, JSON.stringify(language));
    this.currentLanguage = language;
    this.translationService.use(language.code);
    
    // Update user language preference in the database
    this.updateUserLanguagePreference(language.code).subscribe({
      next: () => {
        console.log('User language preference updated successfully');
      },
      error: (error) => {
        console.error('Error updating user language preference:', error);
        // Continue with local language switching even if API call fails
      }
    });
  }

  private updateUserLanguagePreference(languageCode: string) {
    // First get the current user's email to fetch their userId
    return this.authService.user().pipe(
      switchMap((claims) => {
        const emailClaim = claims.find(c => c.type === 'email' || 
                                     c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress');
        
        const email = emailClaim?.value;
        if (!email) {
          throw new Error('User email not found in claims');
        }

        // Get current user info to extract userId
        const apiUrl = `/api/user-info/current?email=${encodeURIComponent(email)}`;
        return this.http.get<any>(apiUrl);
      }),
      switchMap((userInfoResponse) => {
        // Extract userId from the response
        const userInfo = userInfoResponse.userInfoWithOrgSettings || userInfoResponse;
        const userId = userInfo.userId;
        
        if (!userId) {
          throw new Error('User ID not found in user info response');
        }

        // Update the user's language preference
        const updatePayload = {
          userId: userId,
          language: languageCode
        };

        return this.http.put('/api/user-info/update', updatePayload);
      }),
      catchError((error) => {
        console.error('Error in updateUserLanguagePreference:', error);
        return of(null); // Return observable that completes without error to prevent breaking the UI
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
