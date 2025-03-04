import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LayoutComponent } from '../../common/layouts/components/layout/layout.component';
import { HomeComponent } from '../../common/pages/components/home/home.component';
import { authGuard } from '../../essentials/guards/auth.guard';
import { FundingOpportunityItemComponent } from '../../features/internal/components/fundingOpportunity/fundingOpportunityItem/fundingOpportunityItem.component';
import { ContactComponent } from './components/contact/contact.component';
import { ContactItemComponent } from './components/contact/contactItem/contact-item/contact-item.component';
import { PartnerComponent } from './components/partner/partner.component';
import { PartnerItemComponent } from './components/partner/partnerItem/partner-item.component';

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
        path: 'partners',
        component: PartnerComponent,
        canActivate: [authGuard],
        data: { Breadcrumb: 'Partners' }
      },
      {
        path: 'partner/:recordId',
        data: { breadcrumb: 'Details' },
        component: PartnerItemComponent,
        canActivate: [authGuard],
      }
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(internalRoutes)],
  exports: [RouterModule],
})
export class InternalRoutingModule {}
