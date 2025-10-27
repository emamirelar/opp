import { Component, Input, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MarkdownModule } from 'ngx-markdown';

@Component({
  selector: 'app-collapsible-thought',
  standalone: true,
  imports: [CommonModule, MarkdownModule],
  templateUrl: './collapsible-thought.component.html',
  styleUrls: ['./collapsible-thought.component.scss']
})
export class CollapsibleThoughtComponent {
  @Input() content: string = '';
  
  isExpanded = signal(false);
  
  // Maximum characters to show in preview
  private readonly PREVIEW_MAX_LENGTH = 80;
  
  // Extract first line/sentence for preview
  firstLine = computed(() => {
    if (!this.content) return '';
    
    const trimmed = this.content.trim();
    
    // 1. First, try to find bolded text **text**
    const boldMatch = trimmed.match(/\*\*([^*]+)\*\*/);
    if (boldMatch) {
      const boldText = boldMatch[0]; // Keep the ** markers for markdown rendering
      // If bold text is too long, truncate it but preserve markdown syntax
      if (boldText.length > this.PREVIEW_MAX_LENGTH) {
        const innerText = boldMatch[1];
        return '**' + innerText.substring(0, this.PREVIEW_MAX_LENGTH - 6) + '...**';
      }
      return boldText;
    }
    
    // 2. Try to find the first sentence (ending with . ! or ?)
    const firstSentenceMatch = trimmed.match(/^[^.!?]+[.!?]/);
    if (firstSentenceMatch) {
      const firstSentence = firstSentenceMatch[0];
      // If first sentence is too long, truncate it
      if (firstSentence.length > this.PREVIEW_MAX_LENGTH) {
        return firstSentence.substring(0, this.PREVIEW_MAX_LENGTH) + '...';
      }
      return firstSentence;
    }
    
    // 3. No sentence ending found, check for newlines
    const lines = trimmed.split('\n');
    const firstLine = lines[0];
    
    // If first line is too long, truncate it
    if (firstLine.length > this.PREVIEW_MAX_LENGTH) {
      return firstLine.substring(0, this.PREVIEW_MAX_LENGTH) + '...';
    }
    
    return firstLine;
  });
  
  // Check if content is long enough to need collapsing
  shouldCollapse = computed(() => {
    if (!this.content) return false;
    
    const trimmed = this.content.trim();
    const firstLineText = this.firstLine();
    
    // If content is longer than preview, show collapse
    return trimmed.length > firstLineText.length || 
           trimmed.split('\n').filter(line => line.trim().length > 0).length > 1;
  });
  
  toggleExpanded() {
    this.isExpanded.update(value => !value);
  }
}
