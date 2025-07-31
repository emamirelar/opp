import { Component, Input, OnInit, ViewEncapsulation, inject, PLATFORM_ID, signal, output, ViewChild, ElementRef, AfterViewInit, Output, EventEmitter } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { MarkdownModule } from 'ngx-markdown';
import { ResultItem } from '../ai-assistant.model';
import { EntityGridComponent } from './entity-grid/entity-grid.component';
import { TypewriterMarkdownComponent } from './typewriter-markdown/typewriter-markdown.component';

@Component({
  selector: 'app-content-renderer',
  standalone: true,
  imports: [CommonModule, MarkdownModule, EntityGridComponent, TypewriterMarkdownComponent],
  templateUrl: './content-renderer.component.html',
  styleUrls: ['./content-renderer.component.css'],
  encapsulation: ViewEncapsulation.None
})
export class ContentRendererComponent implements OnInit, AfterViewInit {
  @Input() item!: ResultItem;
  @Input() shouldShow: boolean = true; // Controls when this item should be visible
  @Input() isSequential: boolean = false; // Whether this is part of sequential display
  @Input() isNewMessage: boolean = true; // Whether this is a new message (for typewriter effect)
  
  // Output when this content item is done displaying
  contentComplete = output<void>();
  @Output() cardClicked = new EventEmitter<any>();
  
  private platformId = inject(PLATFORM_ID);
  private isBrowser = isPlatformBrowser(this.platformId);

  @ViewChild('mermaidElement') mermaidElement?: ElementRef<HTMLDivElement>;

  async ngOnInit() {
    console.log('🎨 ContentRenderer - Rendering item:', this.item);
    console.log('🎨 ContentRenderer - Type:', this.item.type);
    console.log('🎨 ContentRenderer - Entity:', this.item.entity);
    console.log('🎨 ContentRenderer - Message type:', typeof this.item.message);
    console.log('🎨 ContentRenderer - Message length:', Array.isArray(this.item.message) ? this.item.message.length : 'N/A');
    console.log('🎨 ContentRenderer - Is new message:', this.isNewMessage);
    
    // For non-text content types, signal completion after a brief delay for animation
    if (this.isNonTextContent() && this.shouldShow) {
      setTimeout(() => {
        this.contentComplete.emit();
      }, 400); // Wait for fade-in animation to complete
    }
  }

  async ngAfterViewInit() {
    if (this.item.type === 'mermaid' && this.isBrowser) {
      // Small delay to ensure element is ready
      setTimeout(async () => {
        if (!this.mermaidElement) {
          console.warn('🎨 Mermaid element not ready');
          return;
        }

        try {
          const mermaid = await import('mermaid');
          
          // Configure mermaid
          mermaid.default.initialize({ 
            startOnLoad: false,
            theme: 'default',
            securityLevel: 'loose',
            fontFamily: 'arial'
          });

          // Generate unique ID for this diagram
          const diagramId = `mermaid-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
          
          // Render the diagram
          const diagramCode = this.getStringMessage();
          console.log('🎨 Rendering mermaid diagram:', diagramCode);
          
          const { svg } = await mermaid.default.render(diagramId, diagramCode);
          
          // Insert the rendered SVG
          if (this.mermaidElement) {
            this.mermaidElement.nativeElement.innerHTML = svg;
          }
          
          console.log('🎨 Mermaid diagram rendered successfully');
        } catch (error) {
          console.error('🎨 Failed to render mermaid diagram:', error);
          // Fallback: show the raw mermaid code
          if (this.mermaidElement) {
            this.mermaidElement.nativeElement.innerHTML = `<pre><code>${this.getStringMessage()}</code></pre>`;
          }
        }
      }, 100);
    }
  }

  getStringMessage(): string {
    if (typeof this.item.message === 'string') {
      return this.item.message;
    }
    // Fallback for array or other types
    return JSON.stringify(this.item.message);
  }

  getContentTypeLabel(): string {
    switch (this.item.type) {
      case 'mermaid':
        return 'Mermaid Diagram';
      case 'code':
        return this.item.language ? this.item.language.toUpperCase() : 'Code';
      case 'grid':
        return this.item.entity ? `${this.item.entity} Table` : 'Data Table';
      case 'card':
        return this.item.entity ? `${this.item.entity} Cards` : 'Data Cards';
      default:
        return this.item.type?.charAt(0).toUpperCase() + this.item.type?.slice(1) || 'Content';
    }
  }

  getArrayMessage(): any[] {
    console.log('🎨 ContentRenderer - getArrayMessage called for type:', this.item.type);
    console.log('🎨 ContentRenderer - message is array:', Array.isArray(this.item.message));
    console.log('🎨 ContentRenderer - message value:', this.item.message);
    
    // Handle array of objects (multiple cards/items) - return as-is
    if (Array.isArray(this.item.message)) {
      console.log('🎨 ContentRenderer - Multiple items: returning array with', this.item.message.length, 'items');
      return this.item.message;
    }
    
    // Handle single object (single card/item) - wrap in array for consistent display
    if (this.item.message && typeof this.item.message === 'object') {
      console.log('🎨 ContentRenderer - Single item: converting object to array');
      return [this.item.message];
    }
    
    // Fallback for string or other types - return empty array
    console.log('🎨 ContentRenderer - Returning empty array (fallback)');
    return [];
  }

  onTypingComplete(): void {
    this.contentComplete.emit();
  }

  private isNonTextContent(): boolean {
    return ['grid', 'card', 'mermaid', 'code'].includes(this.item.type);
  }
} 