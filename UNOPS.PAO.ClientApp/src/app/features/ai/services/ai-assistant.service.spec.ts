import { TestBed } from '@angular/core/testing';
import { AiAssistantService } from './ai-assistant.service';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { DialogService } from 'primeng/dynamicdialog';

describe('AiAssistantService', () => {
  let service: AiAssistantService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        AiAssistantService,
        { provide: DialogService, useValue: {} }
      ]
    });
    service = TestBed.inject(AiAssistantService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // TODO: Add tests for AI assistant interactions
  // TODO: Add tests for streaming responses
});

