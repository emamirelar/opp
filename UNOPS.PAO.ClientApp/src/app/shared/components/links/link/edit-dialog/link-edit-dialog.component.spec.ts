import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LinkEditDialogComponent } from './link-edit-dialog.component';
import { TranslateModule } from '@ngx-translate/core';
import { EntityType } from '../../../../models/link.model';
import LinkDataService from '../link-data.service';

const mockLinkDataService = jasmine.createSpyObj('LinkDataService', ['saveLink', 'deleteLink']);

describe('LinkEditDialogComponent', () => {
  let component: LinkEditDialogComponent;
  let fixture: ComponentFixture<LinkEditDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        LinkEditDialogComponent,
        TranslateModule.forRoot()
      ],
      providers: [
        { provide: LinkDataService, useValue: mockLinkDataService }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LinkEditDialogComponent);
    component = fixture.componentInstance;
    component.entityType = 'Partner' as EntityType;
    component.entityId = 1;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize form on init', () => {
    expect(component.form).toBeDefined();
    expect(component.form.get('url')).toBeDefined();
    expect(component.form.get('name')).toBeDefined();
  });

  it('should close dialog and reset form', () => {
    component.close();
    expect(component.form.value.url).toBeFalsy();
  });
});

