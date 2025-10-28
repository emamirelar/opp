import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ContentRendererComponent } from './content-renderer.component';

describe('ContentRendererComponent', () => {
  let component: ContentRendererComponent;
  let fixture: ComponentFixture<ContentRendererComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ContentRendererComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ContentRendererComponent);
    component = fixture.componentInstance;
    // Note: item is required input, so we need to provide it
    component.item = { type: 'text', value: 'test' } as any;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

