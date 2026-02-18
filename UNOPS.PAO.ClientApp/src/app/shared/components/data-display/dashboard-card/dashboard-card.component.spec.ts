import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DashboardCardComponent } from './dashboard-card.component';
import { TranslateModule } from '@ngx-translate/core';
import { DashboardCardConfig } from './dashboard-card.models';

describe('DashboardCardComponent', () => {
  let component: DashboardCardComponent;
  let fixture: ComponentFixture<DashboardCardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        DashboardCardComponent,
        TranslateModule.forRoot()
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DashboardCardComponent);
    component = fixture.componentInstance;
    
    // Set required input
    component.config = {
      title: 'Test Card',
      subtitle: 'Test Subtitle',
      icon: 'dashboard',
      iconColor: 'bg-unops-primary/10'
    } as DashboardCardConfig;
    
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display card configuration', () => {
    const compiled = fixture.nativeElement;
    expect(compiled.textContent).toContain('Test Card');
  });

  it('should handle card size', () => {
    expect(component.cardSize).toBeDefined();
  });
});

