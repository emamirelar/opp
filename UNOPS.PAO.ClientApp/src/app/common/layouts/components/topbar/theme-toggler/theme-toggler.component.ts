import { Component } from '@angular/core';
import { LayoutService } from '../../../services/layout.service';
import { ButtonModule } from 'primeng/button';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-theme-toggler',
  imports: [ButtonModule, CommonModule],
  templateUrl: './theme-toggler.component.html',
  standalone: true,
  styleUrl: './theme-toggler.component.scss'
})
export class ThemeTogglerComponent {
  constructor(public layoutService: LayoutService) { }

  toggleDarkMode() {
    this.layoutService.layoutConfig.update((state) => ({
      ...state,
      darkTheme: !state.darkTheme,
    }));
  }
}
