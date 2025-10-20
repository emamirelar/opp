import { Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';

@Pipe({
  name: 'file',
  standalone: true
})
export class FilePipe implements PipeTransform {
  constructor(private sanitizer: DomSanitizer) {}

  transform(file: File | null | undefined): SafeUrl | string {
    if (!file) return '';
    
    const url = URL.createObjectURL(file);
    return this.sanitizer.bypassSecurityTrustUrl(url);
  }
} 
