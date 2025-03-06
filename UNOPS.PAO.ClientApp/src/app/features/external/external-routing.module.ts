import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LayoutComponent } from '../../common/layouts/components/layout/layout.component';
import { authGuard } from '../../essentials/guards/auth.guard';

const externalRoutes: Routes = [
  {
    path: '',
    component: LayoutComponent,
    children: [
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(externalRoutes)],
  exports: [RouterModule],
})
export class ExternalRoutingModule {}
