import { Component, inject, signal } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { PasswordModule } from 'primeng/password';
import { DialogModule } from 'primeng/dialog';

import { SociaAuth } from './socialAuth/socialAuth.component';
import { AuthService } from '../../../../essentials/services/auth.service';
import { SignUpComponent } from './sign-up/sign-up.component';
import { NgIf } from '@angular/common';

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
    NgIf
],
  providers: [DialogService],
})
export class LoginComponent {
  canShowPrimitiveLoginOption: boolean = true;
  canShowOAuthLoginComponent: boolean = true;
  canDoSignUp: boolean = true;
  displaySignUpDialog: boolean = false;

  userName: string = '';
  password: string = '';
  hidePassword = signal(true);

  private dialogService = inject(DialogService);
  private ref: DynamicDialogRef | undefined;

  constructor(private authService: AuthService) {}

  handleOnPasswordIconClick(event: MouseEvent) {
    this.hidePassword.set(!this.hidePassword());
    event.stopPropagation();
  }

  handleOnLogin(loginForm: NgForm): void {
    let loginFormValues = loginForm?.form.value;
    if (
      loginFormValues.userEmail?.trim() == '' ||
      loginFormValues.password?.trim() == ''
    ) {
      return;
    }

    this.authService
      .logIn(loginFormValues.userEmail, loginFormValues.password)
      .subscribe({
        next: (res) => {
          window.location.href = '/';
        },
        error: (err) => {},
      });
  }

  onOpenSignUpDialog() {
    this.displaySignUpDialog = true;
  }

  onCloseSignUp() {
      this.displaySignUpDialog = false;
  }
}
