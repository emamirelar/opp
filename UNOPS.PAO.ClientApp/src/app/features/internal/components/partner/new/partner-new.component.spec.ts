import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PartnerNewComponent } from './partner-new.component';

describe('NewPartnerComponent', () => {
  let component: PartnerNewComponent;
  let fixture: ComponentFixture<PartnerNewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PartnerNewComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PartnerNewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
