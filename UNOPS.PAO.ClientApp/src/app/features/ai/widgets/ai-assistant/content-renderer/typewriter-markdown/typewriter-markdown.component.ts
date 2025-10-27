import { Component, Input, OnInit, OnDestroy, ViewEncapsulation, inject, PLATFORM_ID, signal, output } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { MarkdownService } from 'ngx-markdown';

@Component({
  selector: 'app-typewriter-markdown',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './typewriter-markdown.component.html',
  styleUrls: ['./typewriter-markdown.component.scss'],
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

  private async processMermaidDiagrams(content: string): Promise<string> {
    if (!this.isBrowser) {
      return content;
    }

    // Find mermaid code blocks
    const mermaidRegex = /```mermaid\n([\s\S]*?)```/g;
    let processedContent = content;
    let match;
    let diagramId = 0;

    try {
      // Dynamically import mermaid
      const mermaid = await import('mermaid');
      mermaid.default.initialize({ 
        startOnLoad: false, 
        theme: 'default',
        securityLevel: 'loose'
      });

      while ((match = mermaidRegex.exec(content)) !== null) {
        const diagramCode = match[1].trim();
        const uniqueId = `mermaid-diagram-${Date.now()}-${diagramId++}`;
        
        try {
          // Generate SVG from mermaid code
          const { svg } = await mermaid.default.render(uniqueId, diagramCode);
          
          // Replace the mermaid code block with the rendered SVG
          processedContent = processedContent.replace(
            match[0], 
            `<div class="mermaid-diagram">${svg}</div>`
          );
        } catch (mermaidError) {
          console.warn('Failed to render mermaid diagram:', mermaidError);
          // Keep the original code block if rendering fails
          processedContent = processedContent.replace(
            match[0], 
            `<pre><code class="language-mermaid">${diagramCode}</code></pre>`
          );
        }
      }
      
      return processedContent;
    } catch (importError) {
      console.warn('Failed to import mermaid:', importError);
      return content;
    }
  }

  private async convertMarkdownAndStartTyping() {
    try {
      // Process mermaid diagrams before markdown conversion
      let processedContent = await this.processMermaidDiagrams(this.content);
      
      // Convert markdown to HTML
      this.htmlContent = await this.markdownService.parse(processedContent) || processedContent;
      
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
