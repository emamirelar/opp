import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AiAssistantScanComponent } from './ai-assistant-scan.component';

describe('AiAssistantScanComponent', () => {
  let component: AiAssistantScanComponent;
  let fixture: ComponentFixture<AiAssistantScanComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AiAssistantScanComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AiAssistantScanComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

