import { TestBed } from '@angular/core/testing';
import { InteractionService } from './interaction.service';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { ConfirmationService } from 'primeng/api';
import { ImportDialogService } from '@features/import-export/components/import/dialog/import-dialog.service';

describe('InteractionService', () => {
  let service: InteractionService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        InteractionService,
        { provide: ConfirmationService, useValue: { confirm: () => {} } },
        { provide: ImportDialogService, useValue: {} }
      ]
    });
    service = TestBed.inject(InteractionService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // TODO: Add tests for interaction CRUD operations
  // TODO: Add tests for interaction filtering
});

