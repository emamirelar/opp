import { Component, ViewChild, ViewContainerRef, AfterViewInit } from '@angular/core';
import { RouterModule, RouterOutlet, Router } from '@angular/router';
import { AuthService } from './essentials/services/auth.service';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { FeedbackDialogComponent } from './common/reusables/widgets/feedback-dialog/feedback-dialog.component';
import { AiAssistantComponent } from './common/reusables/widgets/ai-assistant/ai-assistant.component';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet, 
    RouterModule, 
    ToastModule, 
    ConfirmDialogModule,
    FeedbackDialogComponent, 
    AiAssistantComponent
  ],
  template: `
  <p-confirmDialog></p-confirmDialog>
  <app-feedback-dialog></app-feedback-dialog>
  @if(isLoggedIn) {
    <app-ai-assistant [viewContainerRef]="viewContainerRef"></app-ai-assistant>
  }
    <div #dynamicComponent></div>
  <router-outlet></router-outlet>`,
  standalone: true,
})
export class AppComponent implements AfterViewInit {
  public isExpanded: Boolean = false;
  public isLoggedIn: Boolean = false;
  @ViewChild('dynamicComponent', { read: ViewContainerRef, static: false }) dynamicComponent!: ViewContainerRef;
  viewContainerRef!: ViewContainerRef;
  
  constructor(
    private authService: AuthService,
    private router: Router
  ) { }
  
  ngOnInit() {
    console.log('[APP] Initializing app component');
    
    const cookies = document.cookie.split(';').map(c => c.trim());
    const devCookie = cookies.find(c => c.startsWith('dev-user-email='));
    const hasCookie = !!devCookie;
    
    console.log('[APP] Current cookies:', {
      allCookies: document.cookie,
      cookies: cookies,
      devCookie: devCookie,
      hasCookie: hasCookie
    });
    
    // Fast path for dev cookie - skip all API checks
    if (hasCookie) {
      console.log('[APP] Dev cookie found, setting isLoggedIn=true without API calls');
      this.isLoggedIn = true;
      // If on login page with dev cookie, redirect to home
      if (window.location.href.includes('/login')) {
        console.log('[APP] On login page with dev cookie - redirecting to home');
        window.location.href = '/';
      }
      return;
    }
    
    // If no dev cookie, proceed with normal auth check
    console.log('[APP] No dev cookie, checking login status via API');
    
    this.authService.isLogedIn().subscribe((res) => {
      this.isLoggedIn = res;
      console.log('[APP] isLoggedIn result:', res);
    });
  }
  
  ngAfterViewInit() {
    this.viewContainerRef = this.dynamicComponent;
  }
}
