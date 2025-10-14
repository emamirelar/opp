import { Pipe, PipeTransform } from '@angular/core';
import { Marked } from 'marked';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Pipe({
  name: 'markdown',
  standalone: true
})
export class MarkdownPipe implements PipeTransform {
  private marked: Marked;

  constructor(private sanitizer: DomSanitizer) {
    // Configure marked to open all links in new tabs
    this.marked = new Marked({
      renderer: {
        link: (token) => {
          const href = token.href || '';
          const title = token.title ? ` title="${token.title}"` : '';
          const text = token.text || '';
          return `<a href="${href}"${title} target="_blank" rel="noopener noreferrer">${text}</a>`;
        }
      }
    });
  }

  transform(value: string): SafeHtml {
    if (!value) return '';
    
    const html = this.marked.parse(value) as string;
    return this.sanitizer.bypassSecurityTrustHtml(html);
  }
}
