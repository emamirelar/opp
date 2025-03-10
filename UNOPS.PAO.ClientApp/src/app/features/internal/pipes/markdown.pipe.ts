import { Pipe, PipeTransform } from '@angular/core';
import { Marked } from 'marked';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Pipe({
  name: 'markdown',
  standalone: true
})
export class MarkdownPipe implements PipeTransform {
  private marked = new Marked();

  constructor(private sanitizer: DomSanitizer) {}

  transform(value: string): SafeHtml {
    if (!value) return '';
    
    const html = this.marked.parse(value) as string;
    return this.sanitizer.bypassSecurityTrustHtml(html);
  }
}