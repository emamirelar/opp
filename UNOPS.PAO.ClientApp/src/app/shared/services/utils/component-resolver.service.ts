import { Injectable, Injector, inject, signal,  } from '@angular/core';
import { InteractionModalComponent } from '@partnerships/interactions/components/interaction/modal/interaction-modal.component';
import { ContactEditDialogComponent } from '@partnerships/contacts/components/contact/edit-dialog/contact-edit-dialog.component';
import { ContactEditDialogFooterComponent } from '@partnerships/contacts/components/contact/edit-dialog/footer/contact-edit-dialog-footer.component';
import { DialogService } from 'primeng/dynamicdialog';
import { PartnerEditDialogComponent } from '@partnerships/partners/components/partner/edit-dialog/partner-edit-dialog.component';
import { PartnerEditDialogFooterComponent } from '@partnerships/partners/components/partner/edit-dialog/footer/partner-edit-dialog-footer.component';
import { InteractionModalFooterComponent } from '@partnerships/interactions/components/interaction/modal/footer/interaction-modal-footer.component';

@Injectable({
  providedIn: 'root',
})
export class ComponentResolverService {
  dialogService = inject(DialogService);
  private componentMap: { [key: string]: any } = {
     'Contact': {
        header: 'Contact',
        component: ContactEditDialogComponent,
        footer: ContactEditDialogFooterComponent,
     },
     'Partner': {
        header: 'Partner',
        component: PartnerEditDialogComponent,
        footer: PartnerEditDialogFooterComponent,
     },
     'Interaction': {
        header: 'Interaction',
        component: InteractionModalComponent,
        footer: InteractionModalFooterComponent,
     },
     /*'PartnerTree': PartnerTreeItemComponent,
     'Interaction': InteractionModalComponent,*/
   };

  constructor(private injector: Injector) {}

  resolveComponent(record: any, componentName: string, isNew: boolean = true): void {
    var componentData = this.componentMap[componentName];

    if (componentData) {
      this.dialogService.open(componentData.component, {
        header: (componentName.startsWith('bulk') ? componentData.header : (isNew ? ' New' + componentName : 'Edit ' + componentName)),
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
