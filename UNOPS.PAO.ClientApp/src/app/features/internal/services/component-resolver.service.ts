
import { Injectable, Injector, Type } from '@angular/core';
import { ContactNewComponent  } from '../components/contact/new/contact-new.component';
import { PartnerNewComponent } from '../components/partner/new/partner-new.component';
import { PartnerTreeItemComponent } from '../components/partner-tree/item/partner-tree-item.component';
import { InteractionModalComponent } from '../components/interaction/modal/interaction-modal.component';

@Injectable({
  providedIn: 'root',
})
export class ComponentResolverService {
  private componentMap: { [key: string]: Type<any> } = {
    'Contact': ContactNewComponent,
    'Partner': PartnerNewComponent,
    'PartnerTree': PartnerTreeItemComponent,
    'Interaction': InteractionModalComponent,
  };

  constructor(private injector: Injector) {}

  resolveComponent(componentName: string): Type<any> | null {
    return this.componentMap[componentName] || null;
  }

  loadComponent(componentName: any, viewContainerRef: any, response: any): void {
    const component = this.resolveComponent(componentName);
    if (component) {
      viewContainerRef.clear(); // Clear previous content if needed
      const componentRef = viewContainerRef.createComponent(component, { injector: this.injector });
      componentRef.setInput('record', response);
      componentRef.cdr?.detectChanges();
    } else {
      console.error('Component not found!');
    }
  }
}