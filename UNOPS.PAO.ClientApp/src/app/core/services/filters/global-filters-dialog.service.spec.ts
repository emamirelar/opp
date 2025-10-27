import { TestBed } from '@angular/core/testing';
import { GlobalFiltersDialogService } from './global-filters-dialog.service';

describe('GlobalFiltersDialogService', () => {
  let service: GlobalFiltersDialogService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [GlobalFiltersDialogService]
    });

    service = TestBed.inject(GlobalFiltersDialogService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should have openDialog$ observable', () => {
    expect(service.openDialog$).toBeDefined();
  });

  it('should emit event when openDialog is called', (done) => {
    let emissionCount = 0;

    service.openDialog$.subscribe(() => {
      emissionCount++;
      expect(emissionCount).toBe(1);
      done();
    });

    service.openDialog();
  });

  it('should emit multiple times when openDialog is called multiple times', () => {
    let emissionCount = 0;

    service.openDialog$.subscribe(() => {
      emissionCount++;
    });

    service.openDialog();
    service.openDialog();
    service.openDialog();
    
    expect(emissionCount).toBe(3);
  });

  it('should allow multiple subscribers', () => {
    let subscriber1Called = false;
    let subscriber2Called = false;

    service.openDialog$.subscribe(() => {
      subscriber1Called = true;
    });

    service.openDialog$.subscribe(() => {
      subscriber2Called = true;
    });

    service.openDialog();

    expect(subscriber1Called).toBe(true);
    expect(subscriber2Called).toBe(true);
  });
});

