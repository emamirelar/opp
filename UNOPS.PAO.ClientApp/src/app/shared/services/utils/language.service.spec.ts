import { TestBed } from '@angular/core/testing';
import { LanguageService } from './language.service';
import { TranslateModule } from '@ngx-translate/core';

describe('LanguageService', () => {
  let service: LanguageService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [TranslateModule.forRoot()],
      providers: [LanguageService]
    });
    service = TestBed.inject(LanguageService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // TODO: Add tests for language switching
  // TODO: Add tests for language persistence
  // TODO: Add tests for default language
});

