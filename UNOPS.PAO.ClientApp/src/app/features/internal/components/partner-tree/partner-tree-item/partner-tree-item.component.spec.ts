import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PartnerTreeItemComponent } from './partner-tree-item.component';

describe('PartnerTreeItemComponent', () => {
  let component: PartnerTreeItemComponent;
  let fixture: ComponentFixture<PartnerTreeItemComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PartnerTreeItemComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PartnerTreeItemComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
