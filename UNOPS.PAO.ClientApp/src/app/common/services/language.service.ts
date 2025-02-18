import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { MenuItem } from 'primeng/api';

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
      .map((lang) => ({ name: lang.toUpperCase(), code: lang }));
  }
  
}