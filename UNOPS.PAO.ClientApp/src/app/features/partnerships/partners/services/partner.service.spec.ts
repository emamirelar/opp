import { TestBed } from '@angular/core/testing';
import { PartnerService } from './partner.service';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { DialogService } from 'primeng/dynamicdialog';
import { ConfirmationService } from 'primeng/api';
import { ImportDialogService } from '@features/import-export/components/import/dialog/import-dialog.service';

describe('PartnerService', () => {
  let service: PartnerService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        PartnerService,
        { provide: DialogService, useValue: {} },
        { provide: ConfirmationService, useValue: { confirm: () => {} } },
        { provide: ImportDialogService, useValue: {} }
      ]
    });
    service = TestBed.inject(PartnerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // TODO: Add tests for partner CRUD operations
  // TODO: Add tests for partner search
  // TODO: Add tests for partner filtering
});

