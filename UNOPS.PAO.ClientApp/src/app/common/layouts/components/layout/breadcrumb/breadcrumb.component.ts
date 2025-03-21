import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { filter } from 'rxjs';
import { BreadcrumbModule } from 'primeng/breadcrumb';

@Component({
  selector: 'app-breadcrumb',
  imports: [BreadcrumbModule],
  templateUrl: './breadcrumb.component.html',
  standalone: true,
  styleUrl: './breadcrumb.component.scss'
})
export class BreadcrumbComponent implements OnInit {
  breadcrumbItems: MenuItem[] = [];
  homeBreadcrumb: MenuItem = { icon: 'material-symbols-outlined material-home', routerLink: '/' };

  constructor(
    private router: Router,
    private activatedRoute: ActivatedRoute
  ) {}

  ngOnInit() {
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => {
        this.generateBreadcrumbs();
      });
  }

  generateBreadcrumbs() {
    let route = this.activatedRoute.root;
    const breadcrumbs: MenuItem[] = [];

    while (route.firstChild) {
      route = route.firstChild;

      const routeSnapshot = route.snapshot;
      const url = routeSnapshot.url.map(segment => segment.path).join('/');
      const label = routeSnapshot.data['breadcrumb'] ||
                    this.formatLabel(routeSnapshot.routeConfig?.path || '');

      if (label) {
        breadcrumbs.push({
          label: label,
          routerLink: `/${url}`
        });
      }
    }

    this.breadcrumbItems = breadcrumbs;
  }

  private formatLabel(path: string): string {
    return path
      .split('-')
      .map(word => word.charAt(0).toUpperCase() + word.slice(1))
      .join(' ');
  }
}
