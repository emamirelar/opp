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
  template: `
    <div class="content-item" [ngSwitch]="item.type" *ngIf="shouldShow">
      <!-- Markdown content with typewriter effect -->
      <div *ngSwitchCase="'markdown'">
        <app-typewriter-markdown 
          [content]="getStringMessage()"
          [enableTypewriter]="isNewMessage"
          (typingComplete)="onTypingComplete()">
        </app-typewriter-markdown>
      </div>
      
      <!-- Grid/Table content -->
      <div *ngSwitchCase="'grid'" class="fade-in-content">
        <app-entity-grid 
          [entityType]="item.entity || 'Item'"
          [gridData]="getArrayMessage()"
          (cardClicked)="cardClicked.emit($event)">
        </app-entity-grid>
      </div>
      
      <!-- Card content (using same grid component) -->
      <div *ngSwitchCase="'card'" class="fade-in-content">
        <app-entity-grid 
          [entityType]="item.entity || 'Item'"
          [gridData]="getArrayMessage()"
          (cardClicked)="cardClicked.emit($event)">
        </app-entity-grid>
      </div>
      
      <!-- Mermaid diagram -->
      <div *ngSwitchCase="'mermaid'" class="mermaid-container fade-in-content">
        <div #mermaidElement class="mermaid">{{ getStringMessage() }}</div>
      </div>
      
      <!-- Code block -->
      <div *ngSwitchCase="'code'" class="code-container fade-in-content">
        <pre><code [class]="'language-' + (item.language || 'javascript')">{{ getStringMessage() }}</code></pre>
      </div>
      
      <!-- Plain text fallback with typewriter effect -->
      <div *ngSwitchDefault class="text-content">
        <app-typewriter-markdown 
          [content]="getStringMessage()"
          [enableTypewriter]="isNewMessage"
          (typingComplete)="onTypingComplete()">
        </app-typewriter-markdown>
      </div>
    </div>
  `,
  styles: [`
    .content-item {
      margin: 0.5rem 0;
    }
    
    .fade-in-content {
      animation: fadeInSlideUp 0.5s ease-out;
    }
    
    @keyframes fadeInSlideUp {
      from {
        opacity: 0;
        transform: translateY(12px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }
    
    .mermaid-container {
      margin: 1rem 0;
      text-align: center;
    }
    
    .code-container {
      margin: 1rem 0;
    }
    
    .code-container pre {
      background-color: #f5f5f5;
      border: 1px solid #ddd;
      border-radius: 4px;
      padding: 1rem;
      overflow-x: auto;
    }
    
    .text-content {
      margin: 0.5rem 0;
    }
    
    /* Markdown styling */
    ::ng-deep .content-item markdown {
      font-family: inherit;
    }
    
    ::ng-deep .content-item markdown h1,
    ::ng-deep .content-item markdown h2,
    ::ng-deep .content-item markdown h3 {
      margin-top: 1rem;
      margin-bottom: 0.5rem;
    }
    
    ::ng-deep .content-item markdown p {
      margin: 0.5rem 0;
    }
    
    ::ng-deep .content-item markdown code {
      background-color: #f5f5f5;
      padding: 0.2rem 0.4rem;
      border-radius: 3px;
      font-size: 0.9em;
    }
    
    ::ng-deep .content-item markdown pre {
      background-color: #f5f5f5;
      border: 1px solid #ddd;
      border-radius: 4px;
      padding: 1rem;
      overflow-x: auto;
    }
  `],
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
    
    if (this.item.type === 'mermaid' && this.isBrowser) {
      // Dynamically import mermaid only in browser
      const mermaid = await import('mermaid');
      mermaid.default.initialize({ startOnLoad: true });
    }
    
    // For non-text content types, signal completion after a brief delay for animation
    if (this.isNonTextContent() && this.shouldShow) {
      setTimeout(() => {
        this.contentComplete.emit();
      }, 400); // Wait for fade-in animation to complete
    }
  }

  async ngAfterViewInit() {
    if (this.item.type === 'mermaid' && this.isBrowser && this.mermaidElement) {
      const mermaid = await import('mermaid');
      // Render the diagram in the element
      mermaid.default.init(undefined, this.mermaidElement.nativeElement);
    }
  }

  getStringMessage(): string {
    if (typeof this.item.message === 'string') {
      return this.item.message;
    }
    // Fallback for array or other types
    return JSON.stringify(this.item.message);
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