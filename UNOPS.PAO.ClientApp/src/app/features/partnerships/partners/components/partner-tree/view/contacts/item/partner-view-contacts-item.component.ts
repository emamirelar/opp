import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ContactViewModel } from '../../../../../models/contact-view.model';

@Component({
  selector: 'app-partner-view-contacts-item',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './partner-view-contacts-item.component.html'
})
export class PartnerViewContactsItemComponent {
  @Input() contact!: ContactViewModel;
  @Output() itemClick = new EventEmitter<ContactViewModel>();

  getInitials(): string {
    if (!this.contact) return '';
    const firstInitial = this.contact.firstName ? this.contact.firstName.charAt(0) : '';
    const lastInitial = this.contact.lastName ? this.contact.lastName.charAt(0) : '';
    return (firstInitial + lastInitial).toUpperCase();
  }

  onContactClick(): void {
    this.itemClick.emit(this.contact);
  }

  /**
   * Handle image load error by replacing with default Contact placeholder
   */
  onImageError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.src = 'assets/images/Contact.png';
  }
} 
