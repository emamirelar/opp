import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LayoutComponent } from '../../common/layouts/components/layout/layout.component';
import { authGuard } from '../../essentials/guards/auth.guard';
import { FundingOpportunityComponent } from '../../features/external/components/fundingOpportunity/fundingOpportunity.component';
import { FundingOpportunityItemComponent } from '../../features/external/components/fundingOpportunity/fundingOpportunityItem/fundingOpportunityItem.component';
import { ProposalComponent } from '../../features/external/components/proposal/proposal.component';
import { ProposalItemComponent } from '../../features/external/components/proposal/proposalItem/proposalItem.component';

const externalRoutes: Routes = [
  {
    path: '',
    component: LayoutComponent,
    children: [
      {
        path: 'external/funding-opportunity',
        component: FundingOpportunityComponent,
        data: { breadcrumb: 'Funding Opportunities' },
      },
      {
        path: 'external/funding-opportunity/:recordId',
        data: { breadcrumb: 'Details' },
        component: FundingOpportunityItemComponent,
        canActivate: [authGuard],
      },
      {
        path: 'external/proposal',
        component: ProposalComponent,
        data: { breadcrumb: 'My Proposals' },
      },
      {
        path: 'external/proposal/:recordId',
        data: { breadcrumb: 'Details' },
        component: ProposalItemComponent,
        canActivate: [authGuard],
      },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(externalRoutes)],
  exports: [RouterModule],
})
export class ExternalRoutingModule {}
