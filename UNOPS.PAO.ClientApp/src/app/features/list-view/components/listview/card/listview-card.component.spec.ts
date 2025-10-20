import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ListviewCardComponent } from './listview-card.component';
import { TranslateModule } from '@ngx-translate/core';
import { SimpleChanges } from '@angular/core';

describe('ListviewCardComponent', () => {
  let component: ListviewCardComponent;
  let fixture: ComponentFixture<ListviewCardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ListviewCardComponent, TranslateModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(ListviewCardComponent);
    component = fixture.componentInstance;
    component.config = { pageSize: 20 };
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

      component.data = testData;

      expect(component.data).toEqual(testData);
      expect(spy).toHaveBeenCalled();
    });

    it('should not schedule sentinel observation before view init', () => {
      component['hasViewInitialized'] = false;
      const spy = spyOn<any>(component, 'scheduleObserveSentinel');
      const testData = [{ id: 1, name: 'Test' }];

      component.data = testData;

      expect(component.data).toEqual(testData);
      expect(spy).not.toHaveBeenCalled();
    });
  });

  describe('scheduleObserveSentinel', () => {
    it('should use requestAnimationFrame to observe sentinel', (done) => {
      const spy = spyOn<any>(component, 'observeLoadMoreSentinel');
      
      component['scheduleObserveSentinel']();
      
      expect(component['observeSentinelScheduled']).toBe(true);
      
      // Wait for requestAnimationFrame
      requestAnimationFrame(() => {
        expect(spy).toHaveBeenCalled();
        expect(component['observeSentinelScheduled']).toBe(false);
        done();
      });
    });

    it('should not schedule multiple observations', () => {
      component['observeSentinelScheduled'] = true;
      const spy = spyOn(window, 'requestAnimationFrame');
      
      component['scheduleObserveSentinel']();
      
      expect(spy).not.toHaveBeenCalled();
    });
  });
});
