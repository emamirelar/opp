
import { Injectable, Injector, Type, inject, signal,  } from '@angular/core';
//import { ContactNewComponent  } from '../components/contact/new/contact-new.component';
import { PartnerNewComponent } from '../components/partner/new/partner-new.component';
import { PartnerTreeItemComponent } from '../components/partner-tree/item/partner-tree-item.component';
import { InteractionModalComponent } from '../components/interaction/modal/interaction-modal.component';
import { ContactEditDialogComponent } from '../components/contact/edit-dialog/contact-edit-dialog.component';
import { ContactEditDialogFooterComponent } from '../components/contact/edit-dialog/footer/contact-edit-dialog-footer.component';
import { DialogService } from 'primeng/dynamicdialog';
import { PartnerEditDialogComponent } from '../components/partner/edit-dialog/partner-edit-dialog.component';
import { PartnerEditDialogFooterComponent } from '../components/partner/edit-dialog/footer/partner-edit-dialog-footer.component';

@Injectable({
  providedIn: 'root',
})
export class ComponentResolverService {
  dialogService = inject(DialogService);
  private componentMap: { [key: string]: any } = {
     'Contact': {
        component: ContactEditDialogComponent,
        footer: ContactEditDialogFooterComponent,
     },
     'Partner': {
        component: PartnerEditDialogComponent,
        footer: PartnerEditDialogFooterComponent,
     },
     /*'PartnerTree': PartnerTreeItemComponent,
     'Interaction': InteractionModalComponent,*/
   };

  constructor(private injector: Injector) {}

  resolveComponent(record: any, componentName: string, isNew: boolean = true): void {
    var componentData = this.componentMap[componentName];

    if (componentData) {
      this.dialogService.open(componentData.component, {
        header: isNew ? ' New' + componentName : 'Edit ' + componentName,
        width: '40vw',
        breakpoints: { '960px': '95vw' },
        closable: true,
        templates: {
          footer: componentData.footer
        },
        data: {
          mode: isNew ? 'new' : 'edit',
          record,
          requestingSaveSignal: signal<boolean>(false)
        }
      });
    }
  }

  loadComponent(componentName: any, viewContainerRef: any, response: any): void {
    this.resolveComponent(response, componentName);
  }
}