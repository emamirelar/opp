import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PartnerViewComponent } from './partner-view.component';

describe('PartnerViewComponent', () => {
  let component: PartnerViewComponent;
  let fixture: ComponentFixture<PartnerViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PartnerViewComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(PartnerViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
}); 
