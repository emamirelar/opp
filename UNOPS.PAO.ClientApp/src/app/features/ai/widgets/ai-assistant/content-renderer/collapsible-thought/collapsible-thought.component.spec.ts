import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CollapsibleThoughtComponent } from './collapsible-thought.component';
import { MarkdownService } from 'ngx-markdown';
import { of } from 'rxjs';

describe('CollapsibleThoughtComponent', () => {
  let component: CollapsibleThoughtComponent;
  let fixture: ComponentFixture<CollapsibleThoughtComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CollapsibleThoughtComponent],
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

    fixture = TestBed.createComponent(CollapsibleThoughtComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

