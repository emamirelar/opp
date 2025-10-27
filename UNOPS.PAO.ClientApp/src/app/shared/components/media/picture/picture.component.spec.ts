import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { of } from 'rxjs';
import { PictureComponent } from './picture.component';

describe('PictureComponent', () => {
  let component: PictureComponent;
  let fixture: ComponentFixture<PictureComponent>;
  let mockDialogService: jasmine.SpyObj<DialogService>;
  let mockTranslateService: jasmine.SpyObj<TranslateService>;
  let mockDialogRef: jasmine.SpyObj<DynamicDialogRef>;

  beforeEach(async () => {
    mockDialogRef = jasmine.createSpyObj('DynamicDialogRef', ['close', 'destroy']);
    mockDialogRef.onClose = of('new-image-url.jpg');
    
    mockDialogService = jasmine.createSpyObj('DialogService', ['open']);
    mockDialogService.open.and.returnValue(mockDialogRef);
    
    mockTranslateService = jasmine.createSpyObj('TranslateService', ['instant']);
    mockTranslateService.instant.and.returnValue('Edit Picture');

    await TestBed.configureTestingModule({
      imports: [
        PictureComponent,
        TranslateModule.forRoot()
      ],
      providers: [
        { provide: DialogService, useValue: mockDialogService },
        { provide: TranslateService, useValue: mockTranslateService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(PictureComponent);
    component = fixture.componentInstance;
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
      expect(callArgs[1].width).toBe('40vw');
      expect(callArgs[1].data.uploadUrl).toBe('/api/upload');
    });

    it('should update imageUrl when dialog closes with result', (done) => {
      const newImageUrl = 'new-image.jpg';
      mockDialogRef.onClose = of(newImageUrl);
      
      component.openPictureEditor();

      setTimeout(() => {
        expect(component.imageUrl).toBe(newImageUrl);
        done();
      }, 100);
    });

    it('should emit imageChanged event when dialog closes with result', (done) => {
      const newImageUrl = 'new-image.jpg';
      mockDialogRef.onClose = of(newImageUrl);
      
      spyOn(component.imageChanged, 'emit');
      
      component.openPictureEditor();

      setTimeout(() => {
        expect(component.imageChanged.emit).toHaveBeenCalledWith(newImageUrl);
        done();
      }, 100);
    });

    it('should emit imageChanged event even when dialog closes without result', (done) => {
      mockDialogRef.onClose = of(undefined);
      
      spyOn(component.imageChanged, 'emit');
      
      component.openPictureEditor();

      setTimeout(() => {
        expect(component.imageChanged.emit).toHaveBeenCalledWith(undefined);
        done();
      }, 100);
    });

    it('should not update imageUrl when dialog closes without result', (done) => {
      const originalUrl = 'original.jpg';
      component.imageUrl = originalUrl;
      mockDialogRef.onClose = of(undefined);
      
      component.openPictureEditor();

      setTimeout(() => {
        expect(component.imageUrl).toBe(originalUrl);
        done();
      }, 100);
    });

    it('should translate dialog header', () => {
      component.openPictureEditor();

      expect(mockTranslateService.instant).toHaveBeenCalledWith('title.editPicture');
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
      component.size = 'large';
      component.imageUrl = 'test.jpg';
      fixture.detectChanges();

      const img = fixture.nativeElement.querySelector('img');
      expect(img.className).toContain('w-32');
      expect(img.className).toContain('h-32');
    });

    it('should disable edit button when disabled is true', () => {
      component.disabled = true;
      fixture.detectChanges();

      const button = fixture.nativeElement.querySelector('button');
      if (button) {
        expect(button.disabled || button.className.includes('disabled')).toBeTruthy();
      }
    });
  });
});


