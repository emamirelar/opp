import { Component, Input, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { TagModule } from 'primeng/tag';
import { ChipModule } from 'primeng/chip';
import { DividerModule } from 'primeng/divider';

import { Interaction } from '../../../models/interaction.model';
import { InteractionType } from '../../../models/interaction-type.enum';
import { InteractionIconService } from '../../../../../common/services/interaction-icon.service';

@Component({
  selector: 'app-interaction-preview',
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    TagModule,
    ChipModule,
    DividerModule
  ],
  templateUrl: './interaction-preview.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InteractionPreviewComponent {
  @Input({ required: true }) interaction!: Interaction;

  public interactionIconService = inject(InteractionIconService);

  getInteractionIcon(type: InteractionType): string {
    return this.interactionIconService.getInteractionIcon(type);
  }

  formatDate(date: string | Date): string {
    const d = new Date(date);
    return d.toLocaleDateString() + ' ' + d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

  truncateText(text: string, maxLength: number = 100): string {
    if (!text || text.length <= maxLength) {
      return text;
    }
    return text.substring(0, maxLength) + '...';
  }
}