import { TypewriterDirective } from './typewriter.directive';
import { ElementRef } from '@angular/core';

describe('TypewriterDirective', () => {
  it('should create an instance', () => {
    const elementRef = {} as ElementRef;
    const directive = new TypewriterDirective(elementRef);
    expect(directive).toBeTruthy();
  });

  // TODO: Add tests for typewriter effect
  // TODO: Add tests for text animation
  // TODO: Add tests for speed control
});

