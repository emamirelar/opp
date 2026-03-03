import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TypewriterMarkdownComponent } from './typewriter-markdown.component';
import { MarkdownService } from 'ngx-markdown';
import { of } from 'rxjs';

describe('TypewriterMarkdownComponent', () => {
  let component: TypewriterMarkdownComponent;
  let fixture: ComponentFixture<TypewriterMarkdownComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TypewriterMarkdownComponent],
      providers: [
        {
          provide: MarkdownService,
          useValue: {
            parse: () => '',
            compile: () => '',
            reload$: of(null) // MarkdownModule uses reload$.pipe()
          }
        }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TypewriterMarkdownComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

