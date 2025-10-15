import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EntityTag } from '../../models/entity-tag.model';

/**
 * Generic component for displaying entity tags
 * Can be used across all entities (Partner, Contact, Interaction, etc.)
 */
@Component({
  selector: 'app-entity-tags',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (tags && tags.length > 0) {
      @for (tag of tags; track tag.tag) {
        <span class="px-2 py-1 text-xs rounded-full {{ tag.color }} whitespace-nowrap">{{ tag.tag }}</span>
      }
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EntityTagsComponent {
  @Input() tags: EntityTag[] | null | undefined = null;
}
