import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { LayoutService } from '../../services/layout.service';
import { LanguageSelectorComponent } from './language-selector/language-selector.component';
import { ProfileMenubarComponent } from './profile-menubar/profile-menubar.component';
import { ThemeTogglerComponent } from './theme-toggler/theme-toggler.component';
import { StyleClassModule } from 'primeng/styleclass';
import {AiAssistantComponent} from '../../../reusables/widgets/ai-assistant/ai-assistant.component';
import {Menu} from 'primeng/menu';
import {NgOptimizedImage} from '@angular/common';

@Component({
  selector: 'app-topbar',
  imports: [LanguageSelectorComponent, ProfileMenubarComponent, ThemeTogglerComponent, StyleClassModule, AiAssistantComponent, Menu, NgOptimizedImage],
  templateUrl: './topbar.component.html',
  styleUrl: './topbar.component.scss',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TopbarComponent {
  items!: MenuItem[];

  constructor(
      public layoutService: LayoutService) { }


}
