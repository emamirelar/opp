/*import { Injectable } from "@angular/core";
import { AbstractListViewConfigurator } from "../../../../common/pages/services/listview/abstractListviewConfigurator.service";

@Injectable({
  providedIn: 'root'
})
export class ContactListViewConfigurator extends AbstractListViewConfigurator {

  constructor(){
    super();
  }

  override getIconName(): string {
    return 'money_bag';
  }

  override getTitle(): string {
    return 'title.contacts';
  }

  override getColumns(){
    return [{
      label: 'label.contact.contactId',
      field: 'id',
      type: 'string',
      sortable: true
    },{
      label: 'label.fundingOpportunity.description',
      field: 'description',
      type: 'string',
      sortable: true
    },{
      label: 'label.fundingOpportunity.dateFrom',
      field: 'dateFrom',
      type: 'date',
      sortable: true
    },{
      label: 'label.fundingOpportunity.dateTo',
      field: 'dateTo',
      type: 'date',
      sortable: true
    }];
  }

  override getUrl() {

    return '/api/funding-opportunity';
  }

}
*/