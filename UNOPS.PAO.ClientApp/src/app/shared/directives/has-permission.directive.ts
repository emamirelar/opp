import { Directive, Input, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';
import { AuthService } from '@core/services/auth';

@Directive({
  selector: '[appHasPermission]',
  standalone: true
})
export class HasPermissionDirective implements OnInit {
  @Input() appHasPermission: string | string[] = [];
  private hasView = false;

  constructor(
    private templateRef: TemplateRef<any>,
    private viewContainer: ViewContainerRef,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.updateView();
  }

  private updateView(): void {
    this.authService.currentUser$.subscribe(user => {
      if (!user) {
        this.viewContainer.clear();
        this.hasView = false;
        return;
      }

      const permissions = Array.isArray(this.appHasPermission) 
        ? this.appHasPermission 
        : [this.appHasPermission];  

      const hasPermission = permissions.some(permission => 
        user.roles.includes(permission)
      );

      if (hasPermission && !this.hasView) {
        this.viewContainer.createEmbeddedView(this.templateRef);
        this.hasView = true;
      } else if (!hasPermission && this.hasView) {
        this.viewContainer.clear();
        this.hasView = false;
      }
    });
  }
} 
