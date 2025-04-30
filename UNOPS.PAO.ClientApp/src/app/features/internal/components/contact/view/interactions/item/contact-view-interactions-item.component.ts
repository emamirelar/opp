import { Component, EventEmitter, Input, Output } from '@angular/core';
import { DatePipe, NgClass } from '@angular/common';
import { InteractionViewModel } from '../interaction-view.model';
import { InteractionType } from '../../../../../models/interaction-type.enum';

interface TypeStyle {
  icon: string;
  bgColor: string;
  textColor: string;
}

@Component({
  selector: 'app-contact-view-interactions-item',
  standalone: true,
  imports: [
    NgClass,
    DatePipe
  ],
  templateUrl: './contact-view-interactions-item.component.html'
})
export class ContactViewInteractionsItemComponent {
  @Input() interaction!: InteractionViewModel;
  @Output() itemClick = new EventEmitter<InteractionViewModel>();

  getTypeStyle(type: string): TypeStyle {
    const typeLower = type?.toLowerCase() || '';
    
    switch (typeLower) {
      case InteractionType.Email.toLowerCase():
        return {
          icon: 'pi pi-envelope',
          bgColor: 'bg-purple-50',
          textColor: 'text-purple-800'
        };
      case InteractionType.Phone.toLowerCase():
        return {
          icon: 'pi pi-phone',
          bgColor: 'bg-green-50',
          textColor: 'text-green-800'
        };
      case InteractionType.Chat.toLowerCase():
        return {
          icon: 'pi pi-comments',
          bgColor: 'bg-cyan-50',
          textColor: 'text-cyan-800'
        };
      case InteractionType.VirtualMeeting.toLowerCase():
        return {
          icon: 'pi pi-video',
          bgColor: 'bg-blue-50',
          textColor: 'text-blue-800'
        };
      case InteractionType.InPersonMeeting.toLowerCase():
        return {
          icon: 'pi pi-users',
          bgColor: 'bg-indigo-50',
          textColor: 'text-indigo-800'
        };
      default:
        return {
          icon: 'pi pi-question-circle',
          bgColor: 'bg-gray-50',
          textColor: 'text-gray-800'
        };
    }
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });
  }

  getSenderName(interaction: InteractionViewModel): string {
    return interaction.sender || 'Unknown';
  }

  getRecipientName(interaction: InteractionViewModel): string {
    return interaction.recipients && interaction.recipients.length > 0 ? interaction.recipients[0] : 'Unknown';
  }

  getRemainingRecipientsCount(interaction: InteractionViewModel): number {
    return interaction.recipients ? interaction.recipients.length - 1 : 0;
  }

  hasMultipleRecipients(interaction: InteractionViewModel): boolean {
    return (interaction.recipients?.length || 0) > 1;
  }

  onClick(): void {
    this.itemClick.emit(this.interaction);
  }
}
