import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LayoutComponent } from '../../common/layouts/components/layout/layout.component';
import { HomeComponent } from '../../common/pages/components/home/home.component';
import { authGuard } from '../../essentials/guards/auth.guard';
import { FundingOpportunityComponent } from '../../features/internal/components/fundingOpportunity/fundingOpportunity.component';
import { FundingOpportunityItemComponent } from '../../features/internal/components/fundingOpportunity/fundingOpportunityItem/fundingOpportunityItem.component';
import { ProposalComponent } from '../../features/internal/components/proposal/proposal.component';
import { ProposalItemComponent } from '../../features/internal/components/proposal/proposalItem/proposalItem.component';

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
        path: 'funding-opportunity',
        component: FundingOpportunityComponent,
        canActivate: [authGuard],
        data: { breadcrumb: 'Funding Opportunity' },
      },
      {
        path: 'funding-opportunity/:recordId',
        data: { breadcrumb: 'Details' },
        component: FundingOpportunityItemComponent,
        canActivate: [authGuard],
      },
      {
        path: 'proposal',
        data: { breadcrumb: 'Proposal' },
        component: ProposalComponent,
        canActivate: [authGuard],
      },
      {
          path: 'proposal/:recordId',
          data: { breadcrumb: 'Details' },
          component: ProposalItemComponent,
          canActivate: [authGuard],
      },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(internalRoutes)],
  exports: [RouterModule],
})
export class InternalRoutingModule {}
