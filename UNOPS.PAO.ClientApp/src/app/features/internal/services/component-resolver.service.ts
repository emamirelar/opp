import { Injectable, Injector, Type } from '@angular/core';
import { ContactNewComponent  } from '../components/contact/new/contact-new.component';

@Injectable({
  providedIn: 'root',
})
export class ComponentResolverService {
  private componentMap: { [key: string]: Type<any> } = {
    'Contact': ContactNewComponent,
   // 'Component2': Component2,
  };

  constructor(private injector: Injector) {}

  resolveComponent(componentName: string): Type<any> | null {
    return this.componentMap[componentName] || null;
  }

  loadComponent(componentName: string, viewContainerRef: any, response: any): void {
    const component = this.resolveComponent(componentName);
    if (component) {
      viewContainerRef.clear(); // Clear previous content if needed
      const componentRef = viewContainerRef.createComponent(component, { injector: this.injector });
      componentRef.formGroup.patchValue(response);
      componentRef.cdr.detectChanges();
    } else {
      console.error('Component not found!');
    }
  }
}
