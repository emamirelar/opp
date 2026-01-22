import { DomSanitizer } from '@angular/platform-browser';
import { FilePipe } from './file.pipe';

describe('FilePipe', () => {
  let pipe: FilePipe;
  let sanitizer: jasmine.SpyObj<DomSanitizer>;

  beforeEach(() => {
    sanitizer = jasmine.createSpyObj('DomSanitizer', ['bypassSecurityTrustUrl']);
    sanitizer.bypassSecurityTrustUrl.and.returnValue('safe-url' as any);
    pipe = new FilePipe(sanitizer);
  });

  it('create an instance', () => {
    expect(pipe).toBeTruthy();
  });

  // TODO: Add tests for file transformation
  // TODO: Add tests for file size formatting
  // TODO: Add tests for file name handling
});

