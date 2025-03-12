import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ContactNewComponent } from './contact-new.component';

describe('NewContactComponent', () => {
  let component: ContactNewComponent;
  let fixture: ComponentFixture<ContactNewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ContactNewComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ContactNewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
