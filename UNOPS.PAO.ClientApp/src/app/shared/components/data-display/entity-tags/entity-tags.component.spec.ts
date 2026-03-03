import { ComponentFixture, TestBed } from '@angular/core/testing';
import { EntityTagsComponent } from './entity-tags.component';
import { TranslateModule, TranslateLoader, TranslateFakeLoader } from '@ngx-translate/core';

describe('EntityTagsComponent', () => {
  let component: EntityTagsComponent;
  let fixture: ComponentFixture<EntityTagsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        EntityTagsComponent,
        TranslateModule.forRoot({
          loader: { provide: TranslateLoader, useClass: TranslateFakeLoader }
        })
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EntityTagsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should handle empty tags array', () => {
    component.tags = [];
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should display tags when provided', () => {
    const tags = [{ tag: 'Test Tag', color: 'bg-blue-500' }];
    fixture.componentRef.setInput('tags', tags);
    fixture.detectChanges();
    const compiled = fixture.nativeElement;
    expect(compiled.textContent).toContain('Test Tag');
  });
});

