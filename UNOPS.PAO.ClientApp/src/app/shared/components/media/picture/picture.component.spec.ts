import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule, TranslateService, TranslateLoader, TranslateFakeLoader } from '@ngx-translate/core';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { of } from 'rxjs';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { PictureComponent } from './picture.component';

describe('PictureComponent', () => {
  let component: PictureComponent;
  let fixture: ComponentFixture<PictureComponent>;
  let mockDialogService: jasmine.SpyObj<DialogService>;
  let translateService: TranslateService;
  let mockDialogRef: { onClose: ReturnType<typeof of>; close: jasmine.Spy; destroy: jasmine.Spy };

  beforeEach(async () => {
    const ref = jasmine.createSpyObj('DynamicDialogRef', ['close', 'destroy']);
    (ref as any).onClose = of('new-image-url.jpg');
    mockDialogRef = ref as any;

    mockDialogService = jasmine.createSpyObj('DialogService', ['open']);
    mockDialogService.open.and.returnValue(mockDialogRef as any);

    await TestBed.configureTestingModule({
      imports: [
        PictureComponent,
        TranslateModule.forRoot({
          loader: { provide: TranslateLoader, useClass: TranslateFakeLoader }
        })
      ],
      providers: [
        { provide: DialogService, useValue: mockDialogService },
        provideNoopAnimations()
      ]
    })
      .overrideComponent(PictureComponent, {
        set: {
          providers: [{ provide: DialogService, useValue: mockDialogService }]
        }
      })
      .compileComponents();

    fixture = TestBed.createComponent(PictureComponent);
    component = fixture.componentInstance;
    translateService = TestBed.inject(TranslateService);
    spyOn(translateService, 'instant').and.returnValue('Edit Picture');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('input properties', () => {
    it('should have default imageUrl as null', () => {
      expect(component.imageUrl).toBeNull();
    });

    it('should have default altText', () => {
      expect(component.altText).toBe('Profile picture');
    });

    it('should have default size as medium', () => {
      expect(component.size).toBe('medium');
    });

    it('should have default uploadUrl as null', () => {
      expect(component.uploadUrl).toBeNull();
    });

    it('should have default disabled as false', () => {
      expect(component.disabled).toBeFalse();
    });

    it('should accept custom imageUrl', () => {
      component.imageUrl = 'test-image.jpg';
      expect(component.imageUrl).toBe('test-image.jpg');
    });

    it('should accept custom size', () => {
      component.size = 'large';
      expect(component.size).toBe('large');
    });
  });

  describe('getSizeClass', () => {
    it('should return correct class for extra-small', () => {
      component.size = 'extra-small';
      expect(component.getSizeClass()).toBe('w-10 h-10');
    });

    it('should return correct class for small', () => {
      component.size = 'small';
      expect(component.getSizeClass()).toBe('w-16 h-16');
    });

    it('should return correct class for medium', () => {
      component.size = 'medium';
      expect(component.getSizeClass()).toBe('w-24 h-24');
    });

    it('should return correct class for large', () => {
      component.size = 'large';
      expect(component.getSizeClass()).toBe('w-32 h-32');
    });

    it('should return default class for invalid size', () => {
      component.size = 'invalid' as any;
      expect(component.getSizeClass()).toBe('w-24 h-24');
    });
  });

  describe('openPictureEditor', () => {
    it('should open picture editor dialog', () => {
      component.uploadUrl = '/api/upload';
      
      component.openPictureEditor();

      expect(mockDialogService.open).toHaveBeenCalled();
      const callArgs = mockDialogService.open.calls.mostRecent().args;
      const dialogConfig = callArgs[1] as any;
      expect(dialogConfig.width).toBe('40vw');
      expect(dialogConfig.data.uploadUrl).toBe('/api/upload');
    });

    it('should update imageUrl when dialog closes with result', () => {
      const newImageUrl = 'new-image.jpg';
      (mockDialogRef as any).onClose = of(newImageUrl);

      component.openPictureEditor();

      expect(component.imageUrl).toBe(newImageUrl);
    });

    it('should emit imageChanged event when dialog closes with result', () => {
      const newImageUrl = 'new-image.jpg';
      (mockDialogRef as any).onClose = of(newImageUrl);

      spyOn(component.imageChanged, 'emit');

      component.openPictureEditor();

      expect(component.imageChanged.emit).toHaveBeenCalledWith(newImageUrl);
    });

    it('should emit imageChanged event even when dialog closes without result', () => {
      (mockDialogRef as any).onClose = of(undefined);

      spyOn(component.imageChanged, 'emit');

      component.openPictureEditor();

      expect(component.imageChanged.emit).toHaveBeenCalledWith(undefined);
    });

    it('should not update imageUrl when dialog closes without result', () => {
      const originalUrl = 'original.jpg';
      component.imageUrl = originalUrl;
      (mockDialogRef as any).onClose = of(undefined);

      component.openPictureEditor();

      expect(component.imageUrl).toBe(originalUrl);
    });

    it('should translate dialog header', () => {
      component.openPictureEditor();

      expect(translateService.instant).toHaveBeenCalledWith('title.editPicture');
    });
  });

  describe('template rendering', () => {
    it('should render image when imageUrl is provided', () => {
      component.imageUrl = 'test-image.jpg';
      fixture.detectChanges();

      const img = fixture.nativeElement.querySelector('img');
      expect(img).toBeTruthy();
    });

    it('should apply correct size class', () => {
      fixture.componentRef.setInput('size', 'large');
      fixture.componentRef.setInput('imageUrl', 'test.jpg');
      fixture.detectChanges();

      const container = fixture.nativeElement.querySelector('.rounded-full.overflow-hidden');
      expect(container).toBeTruthy();
      expect(container.className).toContain('w-32');
      expect(container.className).toContain('h-32');
    });

    it('should disable edit button when disabled is true', () => {
      fixture.componentRef.setInput('disabled', true);
      fixture.detectChanges();

      // When disabled, the edit button overlay is not rendered (@if (!disabled))
      const button = fixture.nativeElement.querySelector('button');
      expect(button).toBeFalsy();
    });
  });
});


