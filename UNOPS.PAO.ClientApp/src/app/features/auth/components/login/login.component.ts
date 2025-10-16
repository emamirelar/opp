import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { PasswordModule } from 'primeng/password';
import { DialogModule } from 'primeng/dialog';
import { Router } from '@angular/router';

import { SociaAuth } from './socialAuth/socialAuth.component';
import { AuthService } from '@core/services/auth';
import { SignUpComponent } from './sign-up/sign-up.component';
import { NgIf } from '@angular/common';
import { firstValueFrom } from 'rxjs';
import { IapStatusComponent } from '@app/shared/components/feedback/iap-status/iap-status.component';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
  imports: [
    FormsModule,
    InputTextModule,
    ButtonModule,
    PasswordModule,
    DialogModule,
    SociaAuth,
    SignUpComponent,
    NgIf,
    IapStatusComponent
  ],
  providers: [DialogService],
})
export class LoginComponent implements OnInit {
  // Hide traditional login form by default if IAP is intended to be the primary login method
  canShowPrimitiveLoginOption: boolean = false;
  canShowOAuthLoginComponent: boolean = false;
  canDoSignUp: boolean = false;
  displaySignUpDialog: boolean = false;
  isIapAuthenticated: boolean = false;
  checkingAuthentication: boolean = true;

  userName: string = '';
  password: string = '';
  hidePassword = signal(true);

  private dialogService = inject(DialogService);
  private router = inject(Router);
  private ref: DynamicDialogRef | undefined;

  constructor(
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    // Make sure the auth service has a clean state
    if (this.authService.resetAuthenticationState) {
      this.authService.resetAuthenticationState();
    }
    
    // Direct cookie check - fastest and most reliable for dev mode
    const hasCookie = document.cookie.split(';').some(c => c.trim().startsWith('dev-user-email='));
    
    if (hasCookie) {
      // Use window.location instead of router to ensure a full page reload
      window.location.href = '/';
      return;
    }
    
    // Only check IPA authentication if no cookie is found
    this.checkIapAuthentication();
  }

  private checkIapAuthentication(): void {
    this.checkingAuthentication = true;
    
    // Prevent multiple simultaneous checks
    if (sessionStorage.getItem('login_iap_check_active') === 'true') {
      this.checkingAuthentication = false;
      this.canShowPrimitiveLoginOption = true;
      this.canShowOAuthLoginComponent = true;
      this.canDoSignUp = true;
      return;
    }
    
    // Mark that we're checking
    sessionStorage.setItem('login_iap_check_active', 'true');
    
    // Check if we've already redirected too many times
    const redirectCount = parseInt(sessionStorage.getItem('login_redirect_count') || '0', 10);
    
    if (redirectCount > 1) {
      // Just show the login form and stop trying to redirect
      this.checkingAuthentication = false;
      this.canShowPrimitiveLoginOption = true;
      this.canShowOAuthLoginComponent = true;
      this.canDoSignUp = true;
      
      // Mark that we're done checking
      sessionStorage.setItem('login_iap_check_active', 'false');
      return;
    }
    
    // Direct check for IAP simulation headers with simple timeout
    this.authService.getAuthInfo().subscribe({
      next: (authInfo) => {
        // Check if we have IAP headers
        const hasIapHeaders = authInfo && authInfo.hasIapEmailHeader;
        
        if (hasIapHeaders) {
          this.isIapAuthenticated = true;
          
          // Record this redirection
          sessionStorage.setItem('login_redirect_count', (redirectCount + 1).toString());
          sessionStorage.setItem('login_iap_check_active', 'false');
          
          // Use window.location instead of router for a full page reload to avoid Angular state issues
          window.location.href = '/';
        } else {
          // Fall back to showing login options
          this.isIapAuthenticated = false;
          this.canShowPrimitiveLoginOption = true;
          this.canShowOAuthLoginComponent = true;
          this.canDoSignUp = true;
          this.checkingAuthentication = false;
          
          // Mark that we're done checking
          sessionStorage.setItem('login_iap_check_active', 'false');
        }
      },
      error: (error) => {
        this.checkingAuthentication = false;
        this.canShowPrimitiveLoginOption = true;
        this.canShowOAuthLoginComponent = true;
        this.canDoSignUp = true;
        
        // Mark that we're done checking
        sessionStorage.setItem('login_iap_check_active', 'false');
      },
      complete: () => {
        // Always mark as done to prevent hanging state
        sessionStorage.setItem('login_iap_check_active', 'false');
      }
    });
  }

  handleOnPasswordIconClick(event: MouseEvent) {
    this.hidePassword.set(!this.hidePassword());
    event.stopPropagation();
  }

  async handleOnLogin(loginForm: NgForm): Promise<void> {
    let loginFormValues = loginForm?.form.value;
    if (
      loginFormValues.userEmail?.trim() == '' ||
      loginFormValues.password?.trim() == ''
    ) {
      return;
    }

    try {
      await firstValueFrom(this.authService.logIn(loginFormValues.userEmail, loginFormValues.password));
      window.location.href = '/';
    } catch (err) {
      console.error('Login error:', err);
    }
  }

  onOpenSignUpDialog() {
    this.displaySignUpDialog = true;
  }

  onCloseSignUp() {
      this.displaySignUpDialog = false;
  }
}
