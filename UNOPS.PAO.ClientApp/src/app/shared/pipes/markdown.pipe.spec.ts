import { DomSanitizer } from '@angular/platform-browser';
import { MarkdownPipe } from './markdown.pipe';

describe('MarkdownPipe', () => {
  let pipe: MarkdownPipe;
  let sanitizer: jasmine.SpyObj<DomSanitizer>;

  beforeEach(() => {
    sanitizer = jasmine.createSpyObj('DomSanitizer', ['bypassSecurityTrustHtml']);
    sanitizer.bypassSecurityTrustHtml.and.returnValue('safe-html' as any);
    pipe = new MarkdownPipe(sanitizer);
  });

  it('create an instance', () => {
    expect(pipe).toBeTruthy();
  });

  // TODO: Add tests for markdown transformation
  // TODO: Add tests for edge cases (null, empty, etc.)
  // TODO: Add tests for various markdown syntax
});

