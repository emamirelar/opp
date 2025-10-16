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
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
      line-height: 1.6;
      color: #374151;
    }
    
    .typewriter-content.is-typing {
      cursor: pointer;
    }
    
    .typewriter-content.is-typing:hover {
      opacity: 0.8;
    }
    
    /* Headings with professional styling */
    .typewriter-content h1 {
      font-size: 2rem;
      font-weight: 700;
      margin-top: 1.5rem;
      margin-bottom: 1rem;
      color: #1f2937;
      border-bottom: 3px solid transparent;
      background: linear-gradient(90deg, #667eea 0%, #764ba2 100%);
      background-clip: text;
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      position: relative;
    }
    
    .typewriter-content h1::after {
      content: '';
      position: absolute;
      bottom: -3px;
      left: 0;
      width: 60px;
      height: 3px;
      background: linear-gradient(90deg, #667eea 0%, #764ba2 100%);
      border-radius: 2px;
    }
    
    .typewriter-content h2 {
      font-size: 1.5rem;
      font-weight: 600;
      margin-top: 1.25rem;
      margin-bottom: 0.75rem;
      color: #374151;
      position: relative;
      padding-left: 1rem;
    }
    
    .typewriter-content h2::before {
      content: '';
      position: absolute;
      left: 0;
      top: 0.5rem;
      width: 4px;
      height: 70%;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      border-radius: 2px;
    }
    
    .typewriter-content h3 {
      font-size: 1.25rem;
      font-weight: 600;
      margin-top: 1rem;
      margin-bottom: 0.5rem;
      color: #4b5563;
      position: relative;
    }
    
    .typewriter-content h4, 
    .typewriter-content h5, 
    .typewriter-content h6 {
      font-size: 1.1rem;
      font-weight: 600;
      margin-top: 0.75rem;
      margin-bottom: 0.5rem;
      color: #6b7280;
    }
    
    /* Paragraphs with better spacing */
    .typewriter-content p {
      margin: 0.75rem 0;
      line-height: 1.7;
    }
    
    /* Links with professional styling */
    .typewriter-content a {
      color: #2563eb;
      text-decoration: none;
      font-weight: 500;
      border-bottom: 1px solid transparent;
      transition: all 0.2s ease;
      position: relative;
    }
    
    .typewriter-content a:hover {
      color: #1d4ed8;
      border-bottom-color: #2563eb;
      background: linear-gradient(90deg, rgba(37, 99, 235, 0.1) 0%, transparent 100%);
      padding: 0 4px;
      margin: 0 -4px;
      border-radius: 4px;
    }
    
    .typewriter-content a:visited {
      color: #7c3aed;
    }
    
    /* Lists with proper spacing and styling */
    .typewriter-content ul, 
    .typewriter-content ol {
      margin: 0.75rem 0;
      padding-left: 1.5rem;
    }
    
    .typewriter-content li {
      margin-bottom: 0.5rem;
      line-height: 1.6;
      position: relative;
      list-style: none;
    }
    
    /* Unordered lists (ul) - use bullets */
    .typewriter-content ul li::before {
      content: '•';
      color: #667eea;
      font-weight: bold;
      position: absolute;
      left: -1.2rem;
      font-size: 1.2em;
    }
    
    /* Ordered lists (ol) - use numbers */
    .typewriter-content ol {
      counter-reset: list-counter;
    }
    
    .typewriter-content ol li {
      counter-increment: list-counter;
    }
    
    .typewriter-content ol li::before {
      content: counter(list-counter) '.';
      color: #667eea;
      font-weight: 600;
      position: absolute;
      left: -1.5rem;
      min-width: 1.2rem;
      text-align: right;
    }
    
    /* Ensure ul li doesn't get counter styling */
    .typewriter-content ul li {
      counter-increment: none;
    }
    
    .typewriter-content ul li::before {
      content: '•';
    }
    
    /* Nested lists */
    .typewriter-content li ul, 
    .typewriter-content li ol {
      margin: 0.25rem 0;
    }
    
    /* Text formatting */
    .typewriter-content strong {
      font-weight: 600;
      color: #1f2937;
    }
    
    .typewriter-content em {
      font-style: italic;
      color: #4b5563;
    }
    
    /* Code styling */
    .typewriter-content code {
      background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
      color: #e11d48;
      padding: 0.25rem 0.5rem;
      border-radius: 6px;
      font-size: 0.9em;
      font-family: 'Monaco', 'Menlo', 'Ubuntu Mono', monospace;
      border: 1px solid #e2e8f0;
      font-weight: 500;
    }
    
    .typewriter-content pre {
      background: #1e293b;
      color: #e2e8f0;
      padding: 1.25rem;
      border-radius: 8px;
      overflow-x: auto;
      margin: 1rem 0;
      border: 1px solid #334155;
      position: relative;
    }
    
    .typewriter-content pre::before {
      content: '';
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      height: 3px;
      background: linear-gradient(90deg, #667eea 0%, #764ba2 100%);
      border-radius: 8px 8px 0 0;
    }
    
    .typewriter-content pre code {
      background: transparent;
      color: inherit;
      padding: 0;
      border: none;
      font-size: 0.9rem;
      border-radius: 0;
    }
    
    /* Blockquotes */
    .typewriter-content blockquote {
      margin: 1rem 0;
      padding: 1rem 1.25rem;
      background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
      border-left: 4px solid #667eea;
      border-radius: 0 8px 8px 0;
      font-style: italic;
      color: #4b5563;
      position: relative;
    }
    
    .typewriter-content blockquote::before {
      content: '"';
      font-size: 3rem;
      color: #667eea;
      position: absolute;
      top: -0.5rem;
      left: 0.5rem;
      opacity: 0.3;
      font-family: Georgia, serif;
    }
    
    /* Tables */
    .typewriter-content table {
      border-collapse: collapse;
      width: 100%;
      margin: 1rem 0;
      border-radius: 8px;
      overflow: hidden;
      box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);
    }
    
    .typewriter-content th, 
    .typewriter-content td {
      padding: 0.75rem 1rem;
      text-align: left;
      border-bottom: 1px solid #e5e7eb;
    }
    
    .typewriter-content th {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      font-weight: 600;
      text-transform: uppercase;
      font-size: 0.85rem;
      letter-spacing: 0.5px;
    }
    
    .typewriter-content tr:nth-child(even) {
      background-color: #f9fafb;
    }
    
    .typewriter-content tr:hover {
      background-color: #f3f4f6;
    }
    
    /* Horizontal rules */
    .typewriter-content hr {
      border: none;
      height: 2px;
      background: linear-gradient(90deg, transparent 0%, #667eea 50%, transparent 100%);
      margin: 2rem 0;
      border-radius: 1px;
    }
    
    /* Mermaid diagrams */
    .typewriter-content .mermaid-diagram {
      margin: 1.5rem 0;
      text-align: center;
      background: white;
      border-radius: 12px;
      padding: 1.5rem;
      box-shadow: 0 4px 12px rgba(0,0,0,0.1);
      border: 1px solid #e5e7eb;
      position: relative;
      overflow: hidden;
    }
    
    .typewriter-content .mermaid-diagram::before {
      content: '';
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      height: 4px;
      background: linear-gradient(90deg, #4facfe 0%, #00f2fe 100%);
    }
    
    .typewriter-content .mermaid-diagram svg {
      max-width: 100%;
      height: auto;
      filter: drop-shadow(0 2px 4px rgba(0,0,0,0.05));
    }
    
    /* Responsive design */
    @media (max-width: 768px) {
      .typewriter-content h1 {
        font-size: 1.75rem;
      }
      
      .typewriter-content h2 {
        font-size: 1.35rem;
      }
      
      .typewriter-content ul, 
      .typewriter-content ol {
        padding-left: 1.25rem;
      }
      
      .typewriter-content pre {
        padding: 1rem;
        margin: 0.75rem -0.25rem;
        border-radius: 6px;
      }
      
      .typewriter-content blockquote {
        margin: 0.75rem 0;
        padding: 0.75rem 1rem;
      }
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
