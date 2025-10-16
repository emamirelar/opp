import { Directive, ElementRef, Input, OnInit, OnDestroy, inject, signal } from '@angular/core';

@Directive({
  selector: '[appTypewriter]',
  standalone: true
})
export class TypewriterDirective implements OnInit, OnDestroy {
  @Input() appTypewriter: string = '';
  @Input() typewriterSpeed: number = 30; // milliseconds per character
  @Input() typewriterDelay: number = 100; // initial delay before starting
  @Input() autoStart: boolean = true;

  private elementRef = inject(ElementRef);
  private isTyping = signal(false);
  private currentIndex = signal(0);
  private timeoutId?: number;
  private intervalId?: number;
  private isDestroyed = false;

  ngOnInit() {
    if (this.autoStart && this.appTypewriter) {
      this.startTypewriting();
    }
  }

  ngOnDestroy() {
    this.isDestroyed = true;
    this.stopTypewriting();
  }

  private startTypewriting() {
    if (this.isDestroyed || !this.appTypewriter) return;

    // Clear the element initially
    this.elementRef.nativeElement.innerHTML = '';
    this.isTyping.set(true);
    this.currentIndex.set(0);

    // Start typing after initial delay
    this.timeoutId = window.setTimeout(() => {
      this.typeNextCharacter();
    }, this.typewriterDelay);
  }

  private typeNextCharacter() {
    if (this.isDestroyed || !this.isTyping()) return;

    const text = this.appTypewriter;
    const currentIdx = this.currentIndex();

    if (currentIdx < text.length) {
      // Get the next character or HTML entity
      let nextChar = text[currentIdx];
      let increment = 1;

      // Handle HTML entities (like &nbsp;, &amp;, etc.)
      if (nextChar === '&') {
        const remainingText = text.substring(currentIdx);
        const entityMatch = remainingText.match(/^&[a-zA-Z0-9#]+;/);
        if (entityMatch) {
          nextChar = entityMatch[0];
          increment = nextChar.length;
        }
      }

      // Handle basic HTML tags (like <br>, <b>, </b>, etc.)
      if (nextChar === '<') {
        const remainingText = text.substring(currentIdx);
        const tagMatch = remainingText.match(/^<[^>]*>/);
        if (tagMatch) {
          nextChar = tagMatch[0];
          increment = nextChar.length;
        }
      }

      // Add the character(s) to the display
      this.elementRef.nativeElement.innerHTML = text.substring(0, currentIdx + increment);
      this.currentIndex.set(currentIdx + increment);

      // Schedule next character
      this.intervalId = window.setTimeout(() => {
        this.typeNextCharacter();
      }, this.typewriterSpeed);
    } else {
      // Typing complete
      this.isTyping.set(false);
      this.elementRef.nativeElement.innerHTML = text;
    }
  }

  private stopTypewriting() {
    if (this.timeoutId) {
      clearTimeout(this.timeoutId);
    }
    if (this.intervalId) {
      clearTimeout(this.intervalId);
    }
    this.isTyping.set(false);
  }

  // Public method to skip animation and show full text immediately
  public skipAnimation() {
    this.stopTypewriting();
    this.elementRef.nativeElement.innerHTML = this.appTypewriter;
    this.isTyping.set(false);
  }

  // Public method to check if currently typing
  public getIsTyping() {
    return this.isTyping();
  }
} 
