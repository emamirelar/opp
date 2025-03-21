import { Component } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { MenuItem } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { MenuModule } from 'primeng/menu';
import { MenubarModule } from 'primeng/menubar';
import { Language, LanguageService } from '../../../../services/language.service';

@Component({
  selector: 'app-language-selector',
  imports: [MenubarModule, MenuModule, ButtonModule, TranslateModule],
  templateUrl: './language-selector.component.html',
  standalone: true,
  styleUrl: './language-selector.component.scss'
})

export class LanguageSelectorComponent {
  languages: Language[] = [];
  currentLanguage: Language;
  languageItems: MenuItem[];

  constructor(private languageService: LanguageService) {
    this.languages = this.languageService.getLanguages();
    this.currentLanguage = this.languageService.getCurrentLanguage();

    this.languageItems = this.languages.map(lang => ({
      label: lang.name,
      icon: `pi pi-globe`,
      command: () => this.languageService.switchLanguage(lang)
    }));
  }
}
