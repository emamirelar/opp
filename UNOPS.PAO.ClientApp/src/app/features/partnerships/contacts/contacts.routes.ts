import { Routes } from '@angular/router';
import { authGuard, routePermissionGuard } from '@core/guards';
import { ContactListComponent } from '@partnerships/contacts/components/contact/list/contact-list.component';
import { ContactViewComponent } from '@partnerships/contacts/components/contact/view/contact-view.component';
import { ContactTabsComponent } from '@partnerships/contacts/components/contact/tabs/contact-tabs.component';
import { ContactDataResolver } from '@partnerships/contacts/resolvers/contact-data.resolver';
import {
  ContactViewInteractionsComponent
} from '@partnerships/contacts/components/contact/view/interactions/contact-view-interactions.component';

export const CONTACTS_ROUTES: Routes = [
  { path: '', component: ContactListComponent },
  {
    path: ':recordId',
    component: ContactTabsComponent,
    canActivate: [authGuard, routePermissionGuard],
    data: { breadcrumb: 'Contact' },
    resolve: {
      contactData: ContactDataResolver
    },
    children: [
      {
        path: '',
        component: ContactViewComponent,
        data: { breadcrumb: 'Details' }
      },
      {
        path: 'interactions',
        component: ContactViewInteractionsComponent,
        data: { breadcrumb: 'Interactions' }
      }
    ]
  }
];

