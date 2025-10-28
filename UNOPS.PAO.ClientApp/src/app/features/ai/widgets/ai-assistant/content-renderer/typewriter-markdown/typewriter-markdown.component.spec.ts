import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TypewriterMarkdownComponent } from './typewriter-markdown.component';

describe('TypewriterMarkdownComponent', () => {
  let component: TypewriterMarkdownComponent;
  let fixture: ComponentFixture<TypewriterMarkdownComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TypewriterMarkdownComponent]
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

