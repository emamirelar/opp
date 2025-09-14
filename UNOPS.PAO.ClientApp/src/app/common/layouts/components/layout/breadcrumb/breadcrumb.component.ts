import {
  Component,
  OnInit,
  OnDestroy,
  signal,
  effect,
  inject,
} from '@angular/core';
import {ActivatedRoute, NavigationEnd, Router, Route, RouterLink} from '@angular/router';
import { MenuItem } from 'primeng/api';
import { filter, Subject, takeUntil } from 'rxjs';
import { BreadcrumbModule } from 'primeng/breadcrumb';
import {JsonPipe, NgClass, NgIf} from '@angular/common';
import {Tooltip} from 'primeng/tooltip';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-breadcrumb',
  imports: [BreadcrumbModule, NgClass, NgIf, RouterLink],
  templateUrl: './breadcrumb.component.html',
  standalone: true,
  styleUrl: './breadcrumb.component.scss',
})
export class BreadcrumbComponent implements OnInit, OnDestroy {
  private router = inject(Router);
  private activatedRoute = inject(ActivatedRoute);
  private translateService = inject(TranslateService);
  private destroy$ = new Subject<void>();

  // Use signals for reactive state
  private currentRoute = signal<string>('');
  breadcrumbItems = signal<MenuItem[]>([]);

  homeBreadcrumb: MenuItem = {
    icon: 'material-symbols-outlined material-home',
    routerLink: '/',
  };

  constructor() {
    // Use effect to automatically regenerate breadcrumbs when route changes
    effect(() => {
      const route = this.currentRoute();
      if (route) {
        this.generateBreadcrumbs();
      }
    });
  }

  ngOnInit() {
    // Initial load
    this.currentRoute.set(this.router.url);

    // Subscribe to router events
    this.router.events
      .pipe(
        filter((event) => event instanceof NavigationEnd),
        takeUntil(this.destroy$),
      )
      .subscribe((event: NavigationEnd) => {
        this.currentRoute.set(event.url);
      });

    // Subscribe to language changes to refresh breadcrumbs
    this.translateService.onLangChange
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.generateBreadcrumbs();
      });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  generateBreadcrumbs() {
    let route = this.activatedRoute.root;
    const breadcrumbs: MenuItem[] = [];
    const urlSegments: string[] = [];

    while (route.firstChild) {
      route = route.firstChild;

      const routeSnapshot = route.snapshot;
      const currentSegments = routeSnapshot.url.map((segment) => segment.path);

      // Only add non-empty segments to avoid duplication
      if (currentSegments.length > 0 && currentSegments[0] !== '') {
        urlSegments.push(...currentSegments);
      }

      let label =
        routeSnapshot.data['breadcrumb'] ||
        this.formatLabel(routeSnapshot.routeConfig?.path || '');

      // Translate the label
      label = this.translateLabel(label);

      // Only add breadcrumb if we have a meaningful label and it's not a duplicate
      if (label && label !== '') {
        const isDuplicate = breadcrumbs.some((item) => item.label === label);
        if (!isDuplicate) {
          const routePath =
            urlSegments.length > 0 ? `/${urlSegments.join('/')}` : '/';

          // Check if this route is navigable (has a component or redirect)
          const isNavigable = this.isRouteNavigable(routePath);

          // For routes with numeric parameters, try to find parent route
          const parentRoute = this.getParentRouteIfApplicable(routePath);
          const finalRoute = parentRoute || routePath;
          const finalIsNavigable = parentRoute ? this.isRouteNavigable(parentRoute) : isNavigable;

          const breadcrumbItem: MenuItem = {
            label: label,
          };

          // Only add routerLink if the route is navigable, not the current route, and not "Details"
          if (finalIsNavigable && finalRoute !== this.currentRoute() && label !== 'Details') {
            breadcrumbItem.routerLink = finalRoute;
          }

          breadcrumbs.push(breadcrumbItem);
        }
      }
    }

    // Update the signal with new breadcrumbs
    this.breadcrumbItems.set(breadcrumbs);
  }

  private formatLabel(path: string): string {
    return path
      .split('-')
      .map((word) => word.charAt(0).toUpperCase() + word.slice(1))
      .join(' ');
  }

  private translateLabel(label: string): string {
    // Map common breadcrumb labels to translation keys
    const labelMap: { [key: string]: string } = {
      'Home': 'title.home',
      'Admin': 'title.admin',
      'Partnerships': 'title.partnerships',
      'Partners': 'title.partners',
      'Contacts': 'title.contacts',
      'Interactions': 'title.interactions',
      'Partner Tree': 'title.partnerTree',
      'Partner Tree View': 'title.partnerTree',
      'Details': 'title.details',
      'Data': 'title.data',
      'Search': 'title.search',
      'Leads': 'title.leads',
      'Initiatives': 'title.initiatives',
      'Partnership Agreements': 'title.partnershipAgreements',
      'AI Prompt Admin': 'title.aiPromptsAdmin',
      'Manage User Permissions': 'title.userManagement',
      'Manage my Office': 'title.manageOffice',
      'Manage Entities': 'title.managerEntities',
      'Translation Workbench': 'title.translationWorkbench'
    };

    const translationKey = labelMap[label];
    if (translationKey) {
      return this.translateService.instant(translationKey);
    }

    // Return original label if no translation found
    return label;
  }

  private isRouteNavigable(routePath: string): boolean {
    try {
      const normalizedPath = routePath.startsWith('/')
        ? routePath.substring(1)
        : routePath;

      const segments = normalizedPath.split('/').filter((s) => s.length > 0);
      const hasNumericParams = segments.some((segment) => this.isNumeric(segment));

      if (hasNumericParams) {
        return false;
      }

      return this.routeExistsInConfig(normalizedPath);
    } catch {
      return false;
    }
  }

  private routeExistsInConfig(path: string): boolean {
    const segments = path.split('/').filter((s) => s.length > 0);

    // Get the loaded internal routes from the first route's _loadedRoutes
    const mainRoute = this.router.config.find(route => route.path === '' && (route as any)._loadedRoutes);
    if (!mainRoute || !(mainRoute as any)._loadedRoutes) {
      return false;
    }

    const loadedRoutes = (mainRoute as any)._loadedRoutes;
    const layoutRoute = loadedRoutes.find((route: any) => route.path === '' && route.children);

    if (!layoutRoute?.children) {
      return false;
    }

    return this.findInRouteConfig(segments, layoutRoute.children);
  }

  private findInRouteConfig(segments: string[], routes: Route[]): boolean {
    if (segments.length === 0) {
      return routes.some(route => route.path === '' && this.hasValidEndpoint(route));
    }

    const [firstSegment, ...remainingSegments] = segments;

    // Find matching route
    const matchingRoute = routes.find(route => {
      if (route.path === firstSegment) {
        return true;
      }
      // Handle parameter routes like :recordId
      if (route.path?.startsWith(':') && this.isNumeric(firstSegment)) {
        return false; // Don't allow navigation to parameter routes with actual IDs
      }
      return false;
    });

    if (!matchingRoute) {
      return false;
    }

    // If this is the last segment
    if (remainingSegments.length === 0) {
      return this.hasValidEndpoint(matchingRoute);
    }

    // Continue searching in children
    if (matchingRoute.children) {
      return this.findInRouteConfig(remainingSegments, matchingRoute.children);
    }

    return false;
  }

  private hasValidEndpoint(route: Route): boolean {
    return !!(route.component || route.loadComponent || route.redirectTo ||
              (route.children && route.children.some(child =>
                child.path === '' && (child.component || child.loadComponent)
              )));
  }

  private isNumeric(str: string): boolean {
    return /^\d+$/.test(str);
  }

  private getParentRouteIfApplicable(routePath: string): string | null {
    const normalizedPath = routePath.startsWith('/')
      ? routePath.substring(1)
      : routePath;

    const segments = normalizedPath.split('/').filter((s) => s.length > 0);

    // If the last segment is numeric (ID), try to get parent route
    if (segments.length > 0 && this.isNumeric(segments[segments.length - 1])) {
      const parentSegments = segments.slice(0, -1);
      const parentPath = parentSegments.join('/');

      // Check if parent route exists in the actual route configuration
      if (this.routeExistsInConfig(parentPath)) {
        return `/${parentPath}`;
      }
    }

    return null;
  }
}
