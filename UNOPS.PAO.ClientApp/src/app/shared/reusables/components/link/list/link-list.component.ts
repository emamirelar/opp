import { Component, computed, effect, inject, input, model, output, signal, ViewChild, ElementRef, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Link, EntityType } from '../../../../models/link.model';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { TranslateModule } from '@ngx-translate/core';
import { LinkEditDialogComponent } from '../edit-dialog/link-edit-dialog.component';
import LinkDataService from '../link-data.service';
import {InfiniteScrollDirective} from 'ngx-infinite-scroll';

@Component({
  selector: 'app-link-list',
  standalone: true,
  imports: [
    CommonModule,
    ButtonModule,
    TableModule,
    TooltipModule,
    TranslateModule,
    LinkEditDialogComponent,
    InfiniteScrollDirective,
  ],
  templateUrl: './link-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LinkListComponent {
  linkDataService = inject(LinkDataService);
  @ViewChild('scrollContainer') scrollContainer!: ElementRef;

  // Inputs
  entityType = input.required<EntityType>();
  entityId = input.required<number>();
  title = input<string>('');
  showAddButton = input<boolean>(true);
  pageSize = input<number>(20);
  disabled = input<boolean>(false);

  // Outputs
  onAddClick = output<void>();

  // State
  showEditDialog = signal(false);
  selectedLink = signal<Link | undefined>(undefined);
  isDragging = signal(false);

  constructor() {
    // Effect to reinitialize when input parameters change
    effect(() => {
      const entityType = this.entityType();
      const entityId = this.entityId();
      const pageSize = this.pageSize();
      
      if (entityType && entityId) {
        this.linkDataService.initialize(entityType, entityId, pageSize);
      }
    });
  }

  loadMore() {
    this.linkDataService.load();
  }

  openUrl(url: string) {
    window.open(url, '_blank');
  }

  openEditDialog(link?: Link, event?: Event) {
    event?.stopPropagation();
    this.selectedLink.set(link);
    this.showEditDialog.set(true);
  }

  onDialogSaved() {
    this.selectedLink.set(undefined);
  }

  onDialogClosed() {
    this.selectedLink.set(undefined);
  }

  // Drag and drop handlers
  onDragEnter(event: DragEvent) {
    if (this.disabled()) return;
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(true);
  }

  onDragOver(event: DragEvent) {
    if (this.disabled()) return;
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(true);
  }

  onDragLeave(event: DragEvent) {
    if (this.disabled()) return;
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(false);
  }

  onDrop(event: DragEvent) {
    if (this.disabled()) return;
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(false);

    const url = this.linkDataService.extractUrlFromDrop(event);
    if (url) {
      this.linkDataService.createLink(url);
    }
  }

  displayUrl(url: string) {
    return url.substring(0, 256)
  }
}
