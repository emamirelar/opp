import { Component, Input, OnInit, OnDestroy, ViewEncapsulation, inject, PLATFORM_ID, signal, output } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { MarkdownService } from 'ngx-markdown';

@Component({
  selector: 'app-typewriter-markdown',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div 
      class="typewriter-content"
      [innerHTML]="displayedContent()"
      [class.is-typing]="isTyping()"
      (click)="skipTypewriting()"
      [title]="isTyping() ? 'Click to skip animation' : ''">
    </div>
  `,
  styles: [`
    .typewriter-content {
      cursor: default;
      transition: opacity 0.3s ease;
    }
    
    .typewriter-content.is-typing {
      cursor: pointer;
    }
    
    .typewriter-content.is-typing:hover {
      opacity: 0.8;
    }
    
    /* Markdown styling */
    .typewriter-content h1,
    .typewriter-content h2,
    .typewriter-content h3 {
      margin-top: 1rem;
      margin-bottom: 0.5rem;
    }
    
    .typewriter-content p {
      margin: 0.5rem 0;
    }
    
    .typewriter-content strong {
      font-weight: 600;
    }
    
    .typewriter-content em {
      font-style: italic;
    }
    
    .typewriter-content code {
      background-color: #f5f5f5;
      padding: 0.2rem 0.4rem;
      border-radius: 3px;
      font-size: 0.9em;
    }
  `],
  encapsulation: ViewEncapsulation.None
})
export class TypewriterMarkdownComponent implements OnInit, OnDestroy {
  @Input() content: string = '';
  @Input() typewriterSpeed: number = 20; // milliseconds per character (faster for better UX)
  @Input() typewriterDelay: number = 200; // initial delay before starting (shorter for responsiveness)
  @Input() enableTypewriter: boolean = true; // New input to control whether typewriter effect is applied
  
  // Output event when typing is complete
  typingComplete = output<void>();

  private platformId = inject(PLATFORM_ID);
  private isBrowser = isPlatformBrowser(this.platformId);
  private markdownService = inject(MarkdownService);

  displayedContent = signal('');
  isTyping = signal(false);
  
  private htmlContent = '';
  private currentIndex = 0;
  private timeoutId?: number;
  private isDestroyed = false;

  ngOnInit() {
    if (this.isBrowser && this.content) {
      this.convertMarkdownAndStartTyping();
    }
  }

  ngOnDestroy() {
    this.isDestroyed = true;
    this.stopTypewriting();
  }

  private async convertMarkdownAndStartTyping() {
    try {
      // Convert markdown to HTML
      this.htmlContent = await this.markdownService.parse(this.content) || this.content;
      
      // If typewriter is disabled, show content immediately
      if (!this.enableTypewriter) {
        this.displayedContent.set(this.htmlContent);
        this.typingComplete.emit();
        return;
      }
      
      this.startTypewriting();
    } catch (error) {
      console.warn('Failed to parse markdown, using plain text:', error);
      this.htmlContent = this.content;
      
      // If typewriter is disabled, show content immediately
      if (!this.enableTypewriter) {
        this.displayedContent.set(this.htmlContent);
        this.typingComplete.emit();
        return;
      }
      
      this.startTypewriting();
    }
  }

  private startTypewriting() {
    if (this.isDestroyed || !this.htmlContent || !this.enableTypewriter) return;

    this.displayedContent.set('');
    this.isTyping.set(true);
    this.currentIndex = 0;

    // Start typing after initial delay
    this.timeoutId = window.setTimeout(() => {
      this.typeNextCharacter();
    }, this.typewriterDelay);
  }

  private typeNextCharacter() {
    if (this.isDestroyed || !this.isTyping() || !this.enableTypewriter) return;

    const html = this.htmlContent;
    
    if (this.currentIndex < html.length) {
      let increment = 1;
      
      // Handle HTML tags - skip entire tag at once
      if (html[this.currentIndex] === '<') {
        const tagEnd = html.indexOf('>', this.currentIndex);
        if (tagEnd !== -1) {
          increment = tagEnd - this.currentIndex + 1;
        }
      }
      // Handle HTML entities
      else if (html[this.currentIndex] === '&') {
        const entityEnd = html.indexOf(';', this.currentIndex);
        if (entityEnd !== -1) {
          increment = entityEnd - this.currentIndex + 1;
        }
      }

      this.currentIndex += increment;
      this.displayedContent.set(html.substring(0, this.currentIndex));

      // Schedule next character
      this.timeoutId = window.setTimeout(() => {
        this.typeNextCharacter();
      }, this.typewriterSpeed);
    } else {
      // Typing complete
      this.isTyping.set(false);
      this.displayedContent.set(html);
      this.typingComplete.emit();
    }
  }

  private stopTypewriting() {
    if (this.timeoutId) {
      clearTimeout(this.timeoutId);
    }
    this.isTyping.set(false);
  }

  skipTypewriting() {
    if (this.isTyping() && this.enableTypewriter) {
      this.stopTypewriting();
      this.displayedContent.set(this.htmlContent);
      this.typingComplete.emit();
    }
  }
} 