import { TestBed } from '@angular/core/testing';
import { DialogService } from 'primeng/dynamicdialog';
import { ComponentResolverService } from './component-resolver.service';

describe('ComponentResolverService', () => {
  let service: ComponentResolverService;
  let mockDialogService: jasmine.SpyObj<DialogService>;

  beforeEach(() => {
    mockDialogService = jasmine.createSpyObj('DialogService', ['open']);

    TestBed.configureTestingModule({
      providers: [
        ComponentResolverService,
        { provide: DialogService, useValue: mockDialogService }
      ]
    });

    service = TestBed.inject(ComponentResolverService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // TODO: Add tests for dynamic component resolution
  // TODO: Add tests for component factory creation
  // TODO: Add tests for component injection
});

