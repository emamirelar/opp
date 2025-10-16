import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CollapsibleThoughtComponent } from './collapsible-thought.component';

describe('CollapsibleThoughtComponent', () => {
  let component: CollapsibleThoughtComponent;
  let fixture: ComponentFixture<CollapsibleThoughtComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CollapsibleThoughtComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CollapsibleThoughtComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

