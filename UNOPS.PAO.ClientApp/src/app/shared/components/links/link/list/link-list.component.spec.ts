import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LinkListComponent } from './link-list.component';
import { TranslateModule } from '@ngx-translate/core';
import { createMockTranslateService, createMockDialogService, createMockMarkdownService } from '@shared/testing/test-utilities';
import { EntityType } from '../../../../models/link.model';

describe('LinkListComponent', () => {
  let component: LinkListComponent;
  let fixture: ComponentFixture<LinkListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        LinkListComponent,
        TranslateModule.forRoot()
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LinkListComponent);
    component = fixture.componentInstance;
    
    // Set required inputs using signal setters
    fixture.componentRef.setInput('entityType', 'Partner' as EntityType);
    fixture.componentRef.setInput('entityId', 1);
    
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have linkDataService', () => {
    expect(component.linkDataService).toBeDefined();
  });

  it('should open edit dialog when openEditDialog is called', () => {
    component.openEditDialog();
    expect(component.showEditDialog()).toBe(true);
  });
});

