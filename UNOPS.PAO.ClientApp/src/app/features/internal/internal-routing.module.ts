import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LayoutComponent } from '../../common/layouts/components/layout/layout.component';
import { HomeComponent } from '../../common/pages/components/home/home.component';
import { authGuard } from '../../essentials/guards/auth.guard';
import { ContactComponent } from './components/contact/contact.component';
import { ContactItemComponent } from './components/contact/contactItem/contact-item/contact-item.component';
import {InteractionListComponent} from './components/interaction/list/interaction-list.component';
import { PartnerTreeComponent } from './components/partner-tree/partner-tree.component';

const internalRoutes: Routes = [
  {
    path: '',
    component: LayoutComponent,
    children: [
      {
        path: '',
        component: HomeComponent,
        canActivate: [authGuard],
        data: { breadcrumb: 'Home', icon: 'pi pi-home' },
      },
      {
        path: 'contacts',
        component: ContactComponent,
        canActivate: [authGuard],
        data: {Breadcrumb: 'Contacts'}
      },
      {
        path: 'contact/:recordId',
        data: { breadcrumb: 'Details' },
        component: ContactItemComponent,
        canActivate: [authGuard],
      },
      {
        path: 'interactions',
        canActivate: [authGuard],
        children: [
          { path: '', component: InteractionListComponent},
          { path: ':id', component: InteractionListComponent, data: { breadcrumb: 'Edit' } }
        ]
      },
      {
        path: 'partner-tree',
        data: { breadcrumb: 'Details'},
        component: PartnerTreeComponent,
        canActivate: [authGuard]
      }
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(internalRoutes)],
  exports: [RouterModule],
})
export class InternalRoutingModule {}
