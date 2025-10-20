import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ChartJsComponent } from './chart-js.component';

describe('ChartJsComponent', () => {
  let component: ChartJsComponent;
  let fixture: ComponentFixture<ChartJsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ChartJsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ChartJsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

