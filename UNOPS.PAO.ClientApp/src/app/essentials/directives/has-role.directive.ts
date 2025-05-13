import { Directive, Input, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { PermissionService } from '../services/permission.service';
import { Subscription } from 'rxjs';

@Directive({
  selector: '[appHasRole]',
  standalone: true
})
export class HasRoleDirective implements OnInit {
  @Input() appHasRole: string | string[] = [];
  private hasView = false;
  private subscription: Subscription | null = null;

  constructor(
    private templateRef: TemplateRef<any>,
    private viewContainer: ViewContainerRef,
    private authService: AuthService,
    private permissionService: PermissionService
  ) {}

  ngOnInit(): void {
    this.updateView();
  }

  ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
      this.subscription = null;
    }
  }

  private updateView(): void {
    const roles = Array.isArray(this.appHasRole) ? this.appHasRole : [this.appHasRole];
    
    // Special case - if 'ALL' is included, everyone has access
    if (roles.includes('ALL')) {
      this.showContent();
      return;
    }
    
    // Use centralized permission service for entity permissions
    // This is for directive expressions like [appHasRole]="'Entity:Action'"
    const entityAction = roles.find(r => r.includes(':'));
    if (entityAction) {
      const [entity, action] = entityAction.split(':');
      this.subscription = this.permissionService.canPerformEntityAction(entity, action)
        .subscribe(hasPermission => {
          if (hasPermission) {
            this.showContent();
          } else {
            this.hideContent();
          }
        });
      return;
    }
    
    // Fast path for dev cookies
    if (this.authService.hasDevCookie()) {
      const cookies = document.cookie.split(';').map(c => c.trim());
      const devCookie = cookies.find(c => c.startsWith('dev-user-email='));
      if (devCookie) {
        const email = devCookie.substring('dev-user-email='.length);
        
        let hasRole = false;
        
        // Check if user has any of the specified roles
        for (const role of roles) {
          if (role === 'Administrator' && email.toLowerCase().includes('admin')) {
            hasRole = true;
            break;
          } else if (role === 'Internal' && email.endsWith('@unops.org')) {
            hasRole = true;
            break;
          } else if (role === 'Partner' && email.includes('partner')) {
            hasRole = true;
            break;
          } else if (role === 'External' && email.includes('example.com')) {
            hasRole = true;
            break;
          }
        }
        
        if (hasRole) {
          this.showContent();
        } else {
          this.hideContent();
        }
        
        return;
      }
    }
    
    // Regular path - use auth service
    this.subscription = this.authService.getUserRoles().subscribe(userRoles => {
      if (!userRoles?.length) {
        this.hideContent();
        return;
      }
      
      // Administrators always have access
      if (userRoles.includes('Administrator')) {
        this.showContent();
        return;
      }
      
      // Check if user has any of the required roles
      const hasRole = roles.some(role => userRoles.includes(role));
      
      if (hasRole) {
        this.showContent();
      } else {
        this.hideContent();
      }
    });
  }
  
  private showContent(): void {
    if (!this.hasView) {
      this.viewContainer.createEmbeddedView(this.templateRef);
      this.hasView = true;
    }
  }
  
  private hideContent(): void {
    if (this.hasView) {
      this.viewContainer.clear();
      this.hasView = false;
    }
  }
} 