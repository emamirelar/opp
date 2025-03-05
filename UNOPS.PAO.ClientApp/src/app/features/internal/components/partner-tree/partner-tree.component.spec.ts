import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PartnerTreeComponent } from './partner-tree.component';

describe('PartnerTreeComponent', () => {
  let component: PartnerTreeComponent;
  let fixture: ComponentFixture<PartnerTreeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PartnerTreeComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PartnerTreeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
