import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { MenuItem } from 'primeng/api';
import { animate, state, style, transition, trigger } from '@angular/animations';

@Component({
  selector: 'app-menu-item',
  imports: [CommonModule, RouterModule, TranslateModule],
  templateUrl: './menu-item.component.html',
  styleUrl: './menu-item.component.scss',
  standalone: true,
  animations: [
    trigger('submenu', [
      state('hidden', style({
        height: '0',
        overflow: 'hidden',
        opacity: 0,
      })),
      state('visible', style({
        height: '*',
        opacity: 1,
      })),
      transition('hidden <=> visible', animate('300ms cubic-bezier(0.86, 0, 0.07, 1)')),
    ]),
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MenuItemComponent {
  @Input() item!: MenuItem;

  active: boolean = true;

  constructor(private router: Router) {

  }

  itemClick(event: Event) {
    // Prevent default action
    event.preventDefault();
    
    if (this.item.disabled) {
      return;
    }

    // Execute command if present
    if (this.item.command) {
      this.item.command({ originalEvent: event, item: this.item });
    }

    // Toggle active state for items with children
    if (this.item.items) {
      this.active = !this.active;
    }

    // Navigate if routerLink is present
    if (this.item.routerLink) {
      this.router.navigate(this.item.routerLink);
    }
  }
}
