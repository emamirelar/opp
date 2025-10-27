import { ComponentFixture, TestBed } from '@angular/core/testing';
import { GDriveDocumentComponent } from './document-gdrive.component';

describe('GDriveDocumentComponent', () => {
  let component: GDriveDocumentComponent;
  let fixture: ComponentFixture<GDriveDocumentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GDriveDocumentComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GDriveDocumentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

