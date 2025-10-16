import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { MenuItem } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { MenuModule } from 'primeng/menu';
import { MenubarModule } from 'primeng/menubar';
import { Language, LanguageService } from '@shared/services/utils';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-language-selector',
  imports: [MenubarModule, MenuModule, ButtonModule, TranslateModule],
  templateUrl: './language-selector.component.html',
  standalone: true,
  styleUrl: './language-selector.component.scss'
})

export class LanguageSelectorComponent implements OnInit, OnDestroy {
  languages: Language[] = [];
  languageItems: MenuItem[];
  private languageSubscription?: Subscription;

  constructor(
    private languageService: LanguageService,
    private translateService: TranslateService,
    private cdr: ChangeDetectorRef
  ) {
    this.languages = this.languageService.getLanguages();
    this.languageItems = this.buildLanguageItems();
  }

  get currentLanguage(): Language {
    return this.languageService.currentLanguage();
  }

  private buildLanguageItems(): MenuItem[] {
    const currentLang = this.currentLanguage;
    return this.languages.map(lang => ({
      label: lang.name,
      icon: lang.code === currentLang.code ? 'pi pi-check' : 'pi pi-globe',
      command: () => this.languageService.switchLanguage(lang)
    }));
  }

  ngOnInit() {
    // Subscribe to language changes to update menu items and trigger change detection
    this.languageSubscription = this.translateService.onLangChange.subscribe((langChangeEvent) => {
      console.log('Language changed to:', langChangeEvent.lang);
      // Rebuild menu items to update checkmarks
      this.languageItems = this.buildLanguageItems();
      // Trigger change detection to update the UI
      this.cdr.detectChanges();
    });
  }

  ngOnDestroy() {
    if (this.languageSubscription) {
      this.languageSubscription.unsubscribe();
    }
  }
}
