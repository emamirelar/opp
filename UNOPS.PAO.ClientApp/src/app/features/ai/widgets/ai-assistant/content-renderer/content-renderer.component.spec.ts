import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ContentRendererComponent } from './content-renderer.component';
import { MarkdownService } from 'ngx-markdown';
import { of } from 'rxjs';

describe('ContentRendererComponent', () => {
  let component: ContentRendererComponent;
  let fixture: ComponentFixture<ContentRendererComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ContentRendererComponent],
      providers: [
        {
          provide: MarkdownService,
          useValue: {
            parse: () => '',
            compile: () => '',
            reload$: of(null) // MarkdownModule/MarkdownComponent uses reload$.pipe()
          }
        }
      ]
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

