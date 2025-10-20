import { TestBed } from '@angular/core/testing';
import { EntityPanelService } from './entity-panel.service';

describe('EntityPanelService', () => {
  let service: EntityPanelService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(EntityPanelService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // TODO: Add tests for panel open/close
  // TODO: Add tests for entity panel state management
});

