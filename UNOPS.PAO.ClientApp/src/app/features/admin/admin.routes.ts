import { Routes } from '@angular/router';
import { authGuard } from '@core/guards';
import { PartnerTreeComponent } from '@partnerships/partners/components/partner-tree/partner-tree.component';
import { PartnerTreeViewComponent } from '@partnerships/partners/components/partner-tree/view/partner-tree-view.component';
import { PartnerTreeDetailsComponent } from '@partnerships/partners/components/partner-tree/view/details/partner-tree-details.component';
import { PartnerTreeDataComponent } from '@partnerships/partners/components/partner-tree/view/data/partner-tree-data.component';
import { PartnerTreeDataResolver } from '@partnerships/partners/resolvers/partner-tree-data.resolver';
import { UserManagementComponent } from '@admin/user-management/components/user-management.component';
import { EntityManagerComponent } from '@admin/entity-manager/components/entity-manager.component';
import { TranslationWorkbenchComponent } from '@admin/translation-workbench/components/translation-workbench.component';
import { ComingSoonComponent } from '@features/static-pages/components/coming-soon/coming-soon.component';

export const ADMIN_ROUTES: Routes = [
  {
    path: 'partner-tree',
    children: [
      { path: '', component: PartnerTreeComponent, data: { breadcrumb: 'Partner Tree' } },
      {
        path: ':recordId',
        component: PartnerTreeViewComponent,
        data: { breadcrumb: 'Partner Tree View' },
        resolve: {
          partnerTreeData: PartnerTreeDataResolver
        },
        children: [
          {
            path: '',
            component: PartnerTreeDetailsComponent,
            data: { breadcrumb: 'Details' }
          },
          {
            path: 'data',
            component: PartnerTreeDataComponent,
            data: { breadcrumb: 'Data' }
          }
        ]
      }
    ]
  },
  {
    path: 'ai-prompt-management',
    loadComponent: () => import('@ai/components/ai-prompt/ai-prompt.component').then(m => m.AiPromptComponent),
    data: {
      breadcrumb: 'AI Prompt Admin'
    }
  },
  {
    path: 'user-management',
    component: UserManagementComponent,
    data: {
      breadcrumb: 'Manage User Permissions'
    }
  },
  {
    path: 'office-management',
    component: ComingSoonComponent,
    data: {
      breadcrumb: 'Manage my Office',
      featureName: 'Manage my Office'
    }
  },
  {
    path: 'entity-manager',
    component: EntityManagerComponent,
    data: {
      breadcrumb: 'Manage Entities'
    }
  },
  {
    path: 'translations',
    component: TranslationWorkbenchComponent,
    data: {
      breadcrumb: 'Translation Workbench'
    }
  }
];

