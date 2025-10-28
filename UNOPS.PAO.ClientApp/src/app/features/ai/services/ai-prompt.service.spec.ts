import { TestBed } from '@angular/core/testing';
import { AiPromptService } from './ai-prompt.service';
import { HttpClientTestingModule } from '@angular/common/http/testing';

describe('AiPromptService', () => {
  let service: AiPromptService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AiPromptService]
    });
    service = TestBed.inject(AiPromptService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // TODO: Add tests for prompt management
  // TODO: Add tests for prompt templates
});

