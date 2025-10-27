import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EntityTag } from '../../../models/entity-tag.model';

/**
 * Generic component for displaying entity tags
 * Can be used across all entities (Partner, Contact, Interaction, etc.)
 */
@Component({
  selector: 'app-entity-tags',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './entity-tags.component.html',
  styleUrls: ['./entity-tags.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EntityTagsComponent {
  @Input() tags: EntityTag[] | null | undefined = null;
}
