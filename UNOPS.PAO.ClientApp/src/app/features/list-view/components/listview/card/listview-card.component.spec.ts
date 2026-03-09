import { ComponentFixture, TestBed } from '@angular/core/testing';
import { fakeAsync, tick } from '@angular/core/testing';
import { ListviewCardComponent } from './listview-card.component';
import { TranslateModule, TranslateLoader, TranslateFakeLoader } from '@ngx-translate/core';
import { SimpleChanges } from '@angular/core';
import { InteractionIconService } from '@shared/services/domain/interaction-icon.service';

describe('ListviewCardComponent', () => {
  let component: ListviewCardComponent;
  let fixture: ComponentFixture<ListviewCardComponent>;

  beforeEach(async () => {
    const mockInteractionIconService = jasmine.createSpyObj('InteractionIconService', [
      'getInteractionIcon', 'getInteractionColor', 'getInteractionMaterialIcon', 'getInteractionMaterialIconFilled'
    ]);
    mockInteractionIconService.getInteractionIcon.and.returnValue('pi pi-comments');
    mockInteractionIconService.getInteractionColor.and.returnValue('#3B82F6');
    mockInteractionIconService.getInteractionMaterialIcon.and.returnValue('chat');
    mockInteractionIconService.getInteractionMaterialIconFilled.and.returnValue('chat');

    await TestBed.configureTestingModule({
      imports: [
        ListviewCardComponent,
        TranslateModule.forRoot({ loader: { provide: TranslateLoader, useClass: TranslateFakeLoader } })
      ],
      providers: [
        { provide: InteractionIconService, useValue: mockInteractionIconService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ListviewCardComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('config', { pageSize: 20 });
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnChanges', () => {
    it('should not throw errors when called with data changes', () => {
      const changes: SimpleChanges = {
        data: {
          currentValue: [{ id: 1, name: 'Test' }],
          previousValue: [],
          firstChange: false,
          isFirstChange: () => false
        }
      };

      expect(() => component.ngOnChanges(changes)).not.toThrow();
    });

    it('should schedule sentinel observation when columns change after view init', () => {
      component['hasViewInitialized'] = true;
      const spy = spyOn<any>(component, 'scheduleObserveSentinel');

      const changes: SimpleChanges = {
        columns: {
          currentValue: [{ field: 'name', label: 'Name' }],
          previousValue: [],
          firstChange: false,
          isFirstChange: () => false
        }
      };

      component.ngOnChanges(changes);
      expect(spy).toHaveBeenCalled();
    });

    it('should not schedule sentinel observation on first columns change', () => {
      component['hasViewInitialized'] = true;
      const spy = spyOn<any>(component, 'scheduleObserveSentinel');

      const changes: SimpleChanges = {
        columns: {
          currentValue: [{ field: 'name', label: 'Name' }],
          previousValue: [],
          firstChange: true,
          isFirstChange: () => true
        }
      };

      component.ngOnChanges(changes);
      expect(spy).not.toHaveBeenCalled();
    });
  });

  describe('data setter', () => {
    it('should update internal data and schedule sentinel observation after view init', () => {
      component['hasViewInitialized'] = true;
      const spy = spyOn<any>(component, 'scheduleObserveSentinel');
      const testData = [{ id: 1, name: 'Test' }];

      fixture.componentRef.setInput('data', testData);
      fixture.detectChanges();

      expect(component.data()).toEqual(testData);
      expect(spy).toHaveBeenCalled();
    });

    it('should not schedule sentinel observation before view init', () => {
      component['hasViewInitialized'] = false;
      const spy = spyOn<any>(component, 'scheduleObserveSentinel');
      const testData = [{ id: 1, name: 'Test' }];

      fixture.componentRef.setInput('data', testData);

      expect(component.data()).toEqual(testData);
      expect(spy).not.toHaveBeenCalled();
    });
  });

  describe('scheduleObserveSentinel', () => {
    it('should use requestAnimationFrame to observe sentinel', fakeAsync(() => {
      const observeSpy = spyOn<any>(component, 'observeLoadMoreSentinel');
      const rafSpy = spyOn(window, 'requestAnimationFrame').and.callFake((cb: FrameRequestCallback) => {
        setTimeout(() => cb(0), 0);
        return 0;
      });
      
      component['scheduleObserveSentinel']();
      
      expect(component['observeSentinelScheduled']).toBe(true);
      expect(rafSpy).toHaveBeenCalled();
      
      tick();
      
      expect(observeSpy).toHaveBeenCalled();
      expect(component['observeSentinelScheduled']).toBe(false);
    }));

    it('should not schedule multiple observations', () => {
      component['observeSentinelScheduled'] = true;
      const spy = spyOn(window, 'requestAnimationFrame');
      
      component['scheduleObserveSentinel']();
      
      expect(spy).not.toHaveBeenCalled();
    });
  });
});
