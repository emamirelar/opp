import { Injectable } from '@angular/core';
import { InteractionType } from '@features/internal/models/interaction-type.enum';

export interface InteractionIconInfo {
  icon: string;
  materialIcon: string;
  materialIconFilled: string;
  color: string;
  bgColor: string;
  textColor: string;
  gradient: string;
  shadowColor: string;
}

@Injectable({
  providedIn: 'root'
})
export class InteractionIconService {

  /**
   * Get comprehensive icon information for an interaction type
   */
  getInteractionIconInfo(type: string | null | undefined): InteractionIconInfo {
    const typeLower = (type && typeof type === 'string') ? type.toLowerCase() : '';
    
    switch (typeLower) {
      case InteractionType.Email.toLowerCase():
        return {
          icon: 'pi pi-envelope',
          materialIcon: 'mail',
          materialIconFilled: 'mail',
          color: '#8b5cf6',
          bgColor: 'bg-purple-50',
          textColor: 'text-purple-800',
          gradient: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          shadowColor: 'rgba(102, 126, 234, 0.3)'
        };
      case InteractionType.Phone.toLowerCase():
      case 'call': // Fallback for legacy data
        return {
          icon: 'pi pi-phone',
          materialIcon: 'phone',
          materialIconFilled: 'phone',
          color: '#10b981',
          bgColor: 'bg-green-50',
          textColor: 'text-green-800',
          gradient: 'linear-gradient(135deg, #11998e 0%, #38ef7d 100%)',
          shadowColor: 'rgba(17, 153, 142, 0.3)'
        };
      case InteractionType.Chat.toLowerCase():
        return {
          icon: 'pi pi-comments',
          materialIcon: 'chat',
          materialIconFilled: 'chat_bubble',
          color: '#06b6d4',
          bgColor: 'bg-cyan-50',
          textColor: 'text-cyan-800',
          gradient: 'linear-gradient(135deg, #74b9ff 0%, #0984e3 100%)',
          shadowColor: 'rgba(116, 185, 255, 0.3)'
        };
      case InteractionType.VirtualMeeting.toLowerCase():
      case 'video call': // Fallback for legacy data
        return {
          icon: 'pi pi-video',
          materialIcon: 'videocam',
          materialIconFilled: 'videocam',
          color: '#3b82f6',
          bgColor: 'bg-blue-50',
          textColor: 'text-blue-800',
          gradient: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          shadowColor: 'rgba(102, 126, 234, 0.3)'
        };
      case InteractionType.InPersonMeeting.toLowerCase():
      case 'meeting': // Fallback for legacy data
        return {
          icon: 'pi pi-users',
          materialIcon: 'group',
          materialIconFilled: 'group',
          color: '#6366f1',
          bgColor: 'bg-indigo-50',
          textColor: 'text-indigo-800',
          gradient: 'linear-gradient(135deg, #a29bfe 0%, #6c5ce7 100%)',
          shadowColor: 'rgba(162, 155, 254, 0.3)'
        };
      case 'note':
        return {
          icon: 'pi pi-file-edit',
          materialIcon: 'note',
          materialIconFilled: 'sticky_note_2',
          color: '#f59e0b',
          bgColor: 'bg-amber-50',
          textColor: 'text-amber-800',
          gradient: 'linear-gradient(135deg, #fdcb6e 0%, #e17055 100%)',
          shadowColor: 'rgba(253, 203, 110, 0.3)'
        };
      case 'task':
        return {
          icon: 'pi pi-check-square',
          materialIcon: 'task_alt',
          materialIconFilled: 'check_circle',
          color: '#8b5cf6',
          bgColor: 'bg-purple-50',
          textColor: 'text-purple-800',
          gradient: 'linear-gradient(135deg, #a29bfe 0%, #6c5ce7 100%)',
          shadowColor: 'rgba(162, 155, 254, 0.3)'
        };
      case 'appointment':
        return {
          icon: 'pi pi-calendar',
          materialIcon: 'event',
          materialIconFilled: 'event',
          color: '#ef4444',
          bgColor: 'bg-red-50',
          textColor: 'text-red-800',
          gradient: 'linear-gradient(135deg, #fd79a8 0%, #e84393 100%)',
          shadowColor: 'rgba(253, 121, 168, 0.3)'
        };
      case 'other':
      default:
        return {
          icon: 'pi pi-question-circle',
          materialIcon: 'help',
          materialIconFilled: 'help',
          color: '#6b7280',
          bgColor: 'bg-gray-50',
          textColor: 'text-gray-800',
          gradient: 'linear-gradient(135deg, #ddd6fe 0%, #8b5cf6 100%)',
          shadowColor: 'rgba(139, 92, 246, 0.2)'
        };
    }
  }

  /**
   * Get just the icon class for an interaction type
   */
  getInteractionIcon(type: string | null | undefined): string {
    return this.getInteractionIconInfo(type).icon;
  }

  /**
   * Get the Material Design icon name for an interaction type
   */
  getInteractionMaterialIcon(type: string | null | undefined): string {
    return this.getInteractionIconInfo(type).materialIcon;
  }

  /**
   * Get the Material Design filled icon name for an interaction type
   */
  getInteractionMaterialIconFilled(type: string | null | undefined): string {
    return this.getInteractionIconInfo(type).materialIconFilled;
  }

  /**
   * Get the gradient background for an interaction type
   */
  getInteractionGradient(type: string | null | undefined): string {
    return this.getInteractionIconInfo(type).gradient;
  }

  /**
   * Get the shadow color for an interaction type
   */
  getInteractionShadowColor(type: string | null | undefined): string {
    return this.getInteractionIconInfo(type).shadowColor;
  }

  /**
   * Get the color for an interaction type
   */
  getInteractionColor(type: string | null | undefined): string {
    return this.getInteractionIconInfo(type).color;
  }

  /**
   * Get background color class for an interaction type
   */
  getInteractionBgColor(type: string): string {
    return this.getInteractionIconInfo(type).bgColor;
  }

  /**
   * Get text color class for an interaction type
   */
  getInteractionTextColor(type: string): string {
    return this.getInteractionIconInfo(type).textColor;
  }

  /**
   * Generate HTML for displaying interaction type with icon
   */
  getInteractionIconHtml(type: string): string {
    const info = this.getInteractionIconInfo(type);
    return `<div class="flex items-center gap-2">
      <span class="material-symbols-outlined" style="color: ${info.color}">${info.materialIcon}</span>
      <span>${type}</span>
    </div>`;
  }

  /**
   * Get all supported interaction types with their icons
   */
  getAllInteractionTypes(): Array<{type: string, info: InteractionIconInfo}> {
    return [
      { type: InteractionType.Email, info: this.getInteractionIconInfo(InteractionType.Email) },
      { type: InteractionType.Phone, info: this.getInteractionIconInfo(InteractionType.Phone) },
      { type: InteractionType.Chat, info: this.getInteractionIconInfo(InteractionType.Chat) },
      { type: InteractionType.VirtualMeeting, info: this.getInteractionIconInfo(InteractionType.VirtualMeeting) },
      { type: InteractionType.InPersonMeeting, info: this.getInteractionIconInfo(InteractionType.InPersonMeeting) }
    ];
  }
}