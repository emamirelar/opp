import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { LayoutService } from '../../services/layout.service';
import { LanguageSelectorComponent } from './language-selector/language-selector.component';
import { ProfileMenubarComponent } from './profile-menubar/profile-menubar.component';
import { ThemeTogglerComponent } from './theme-toggler/theme-toggler.component';
import { StyleClassModule } from 'primeng/styleclass';

@Component({
  selector: 'app-topbar',
  imports: [LanguageSelectorComponent, ProfileMenubarComponent, ThemeTogglerComponent, StyleClassModule],
  templateUrl: './topbar.component.html',
  styleUrl: './topbar.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TopbarComponent {
  items!: MenuItem[];

  constructor(
      public layoutService: LayoutService) { }
}