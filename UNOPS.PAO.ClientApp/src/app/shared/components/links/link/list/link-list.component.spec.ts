import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { LinkListComponent } from './link-list.component';
import { TranslateModule } from '@ngx-translate/core';
import { EntityType } from '../../../../models/link.model';
import LinkDataService from '../link-data.service';

const mockLinkDataService = jasmine.createSpyObj('LinkDataService', ['initialize', 'load', 'createLink', 'saveLink', 'deleteLink', 'createEmptyLink']);
mockLinkDataService.links = signal([]);
mockLinkDataService.loading = signal(false);
mockLinkDataService.saving = signal(false);
mockLinkDataService.hasMore = signal(true);
mockLinkDataService.currentPage = signal(0);
mockLinkDataService.pageSize = signal(20);
mockLinkDataService.entityType = signal(undefined);
mockLinkDataService.entityId = signal(undefined);

describe('LinkListComponent', () => {
  let component: LinkListComponent;
  let fixture: ComponentFixture<LinkListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        LinkListComponent,
        TranslateModule.forRoot()
      ],
      providers: [
        { provide: LinkDataService, useValue: mockLinkDataService }
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

