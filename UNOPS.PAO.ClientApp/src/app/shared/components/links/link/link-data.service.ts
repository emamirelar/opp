import { inject, Injectable, signal, WritableSignal } from '@angular/core';
import { LinkService } from '@shared/services/api/link.service';
import { Link, EntityType, LinkRequest, UpdateLinkRequest } from '../../../models/link.model';


@Injectable({providedIn: 'root'})
export default class LinkDataService {
  private linkService = inject(LinkService);

  // State
  links: WritableSignal<Link[]> = signal([]);
  loading = signal(false);
  saving = signal(false);
  currentPage = signal(-1);
  hasMore = signal(true);
  pageSize = signal(20);

  // Inputs
  entityType = signal<EntityType | undefined>(undefined);
  entityId = signal<number | undefined>(undefined);

  // Methods
  initialize(entityType: EntityType, entityId: number, pageSize: number = 20) {
    this.entityType.set(entityType);
    this.entityId.set(entityId);
    this.pageSize.set(pageSize);
    this.load(true);
  }

  load(reset = false) {
    if (!this.entityType() || !this.entityId()) return;

    this.currentPage.update(page => reset ? 0 : page + 1);
    this.loading.set(true);

    this.linkService.getAll(
      this.entityType()!,
      this.entityId()!,
      this.currentPage(),
      this.pageSize()
    ).subscribe({
      next: (response) => {
        const newLinks = response.body?.records || [];

        this.links.update(links => reset ? newLinks : [...links, ...newLinks]);
        this.hasMore.set(newLinks.length === this.pageSize());
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  createLink(url: string) {
    if (!this.entityType() || !this.entityId()) return;

    const name = this.getNameFromUrl(url);
    const linkRequest: LinkRequest = {
      entity: this.entityType()!,
      entityId: this.entityId()!,
      url,
      name
    };

    this.saving.set(true);
    this.linkService.create(linkRequest).subscribe({
      next: () => {
        this.saving.set(false);
        this.load(true);
      },
      error: (error) => {
        console.error('Error creating link', error);
        this.saving.set(false);
      }
    });
  }

  saveLink(link: Link | UpdateLinkRequest) {
    if (!link.url) return;

    this.saving.set(true);
    if ('id' in link) {
      // Update existing link
      this.linkService.update(link as UpdateLinkRequest).subscribe({
        next: () => {
          this.saving.set(false);
          this.load(true);
        },
        error: (error) => {
          console.error('Error updating link', error);
          this.saving.set(false);
        }
      });
    } else {
      // Create new link
      this.linkService.create(link).subscribe({
        next: () => {
          this.saving.set(false);
          this.load(true);
        },
        error: (error) => {
          console.error('Error creating link', error);
          this.saving.set(false);
        }
      });
    }
  }

  deleteLink(linkId: number) {
    this.loading.set(true);
    this.linkService.delete(linkId).subscribe({
      next: () => {
        this.loading.set(false);
        this.load(true);
      },
      error: (error) => {
        console.error('Error deleting link', error);
        this.loading.set(false);
      }
    });
  }

  createEmptyLink(): Link {
    return {
      entity: this.entityType()!,
      entityId: this.entityId()!,
      url: '',
      name: ''
    };
  }

  private getNameFromUrl(url: string): string {
    try {
      return new URL(url).hostname;
    } catch {
      return url;
    }
  }

  isValidUrl(text: string): boolean {
    try {
      const url = new URL(text);
      return url.protocol === 'http:' || url.protocol === 'https:';
    } catch {
      return false;
    }
  }

  extractUrlFromDrop(event: DragEvent): string | null {
    // Check for webloc file
    const files = event.dataTransfer?.files;
    if (files?.length && files[0].name.endsWith('.webloc')) {
      const reader = new FileReader();
      reader.onload = (e) => {
        try {
          const content = e.target?.result as string;
          const parser = new DOMParser();
          const xmlDoc = parser.parseFromString(content, "text/xml");
          const urlElement = xmlDoc.querySelector("string");
          const url = urlElement?.textContent?.trim();
          if (url) {
            this.createLink(url);
          }
        } catch (error) {
          console.error('Error parsing webloc file', error);
        }
      };
      reader.readAsText(files[0]);
      return null;
    }

    // Check for URL from browser
    const dataTransfer = event.dataTransfer;
    if (dataTransfer) {
      const urlData = dataTransfer.getData('text/uri-list') || dataTransfer.getData('text/plain');
      if (urlData && this.isValidUrl(urlData)) {
        return urlData;
      }
    }

    return null;
  }
}
