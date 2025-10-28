import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AiAssistantPanelComponent } from './ai-assistant-panel.component';

describe('AiAssistantPanelComponent', () => {
  let component: AiAssistantPanelComponent;
  let fixture: ComponentFixture<AiAssistantPanelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AiAssistantPanelComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AiAssistantPanelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

