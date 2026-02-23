import { Component, ViewChild, ViewContainerRef, AfterViewInit, inject } from '@angular/core';
import { RouterModule, RouterOutlet, Router } from '@angular/router';
import { AuthService, IapSessionRefreshService } from '@core/services/auth';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { FeedbackDialogComponent } from '@shared/components/feedback/feedback-dialog/feedback-dialog.component';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet,
    RouterModule,
    ToastModule,
    ConfirmDialogModule,
    FeedbackDialogComponent
  ],
  template: `
  <p-confirmDialog></p-confirmDialog>
  <app-feedback-dialog></app-feedback-dialog>
  <div #dynamicComponent></div>
  <router-outlet></router-outlet>`,
  standalone: true,
})
export class AppComponent implements AfterViewInit {
  public isExpanded: Boolean = false;
  public isLoggedIn: Boolean = false;
  @ViewChild('dynamicComponent', { read: ViewContainerRef, static: false }) dynamicComponent!: ViewContainerRef;
  viewContainerRef!: ViewContainerRef;

  private iapSessionRefresh = inject(IapSessionRefreshService);

  constructor(
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit() {
    const cookies = document.cookie.split(';').map(c => c.trim());
    const devCookie = cookies.find(c => c.startsWith('dev-user-email='));
    const hasCookie = !!devCookie;

    // Fast path for dev cookie - skip all API checks
    if (hasCookie) {
      this.isLoggedIn = true;
      if (window.location.href.includes('/login')) {
        window.location.href = '/';
      }
      return;
    }

    this.authService.isLogedIn().subscribe((res) => {
      this.isLoggedIn = res;
      if (res) {
        console.log('[IAP-SESSION] User logged in - starting proactive refresh');
        this.iapSessionRefresh.startProactiveRefresh();
      }
    });
  }

  private isProduction(): boolean {
    const hostname = window.location.hostname;
    return hostname !== 'localhost' &&
           hostname !== '127.0.0.1' &&
           !hostname.includes('dev') &&
           !hostname.includes('staging');
  }

  ngAfterViewInit() {
    this.viewContainerRef = this.dynamicComponent;
  }
}
