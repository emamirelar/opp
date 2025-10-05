import { AuthService } from '@core/services/auth.service';
import { ChangeDetectionStrategy, Component, EventEmitter, Output } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { PasswordModule } from 'primeng/password';
import { DialogModule } from 'primeng/dialog';
import { FeedbackDialogService } from '../../../../services/feedback-dialog.service';
import { PanelModule } from 'primeng/panel';
import { CommonModule } from '@angular/common';
import { InputIconModule } from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';
import { IconFieldModule } from 'primeng/iconfield';

@Component({
  selector: 'app-sign-up',
  templateUrl: './sign-up.component.html',
  styleUrl: './sign-up.component.scss',
  imports: [
    DialogModule,
    PasswordModule,
    PanelModule,
    ButtonModule,
    CommonModule,
    FormsModule,
    InputIconModule,
    InputTextModule,
    ReactiveFormsModule,
    IconFieldModule
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SignUpComponent {
  @Output() closeSignUp = new EventEmitter<void>();
  
  visible = true;
  loading = false;
  signUpForm: FormGroup = new FormGroup({});

  passwordPolicies = [
    { 
      label: 'Passwords must be at least 6 characters.',
      validator: (password: string) => password.length >= 6 
    },
    { 
      label: 'Passwords must have at least one non alphanumeric character.',
      validator: (password: string) => /[^a-zA-Z0-9]/.test(password)
    },
    { 
      label: 'Passwords must have at least one digit (0-9).',
      validator: (password: string) => /\d/.test(password)
    },
    { 
      label: 'Passwords must have at least one uppercase (A-Z).',
      validator: (password: string) => /[A-Z]/.test(password)
    }
  ];

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private feedbackService: FeedbackDialogService
  ) {
    this.createForm();
  }

  private createForm(): void {
    this.signUpForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [
        Validators.required,
        Validators.pattern(/^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{6,}$/)
      ]],
      confirmPassword: ['', Validators.required]
    }, { validators: this.passwordMatchValidator });
  }

  private passwordMatchValidator(form: FormGroup) {
    const password = form.get('password');
    const confirmPassword = form.get('confirmPassword');

    if (password?.value !== confirmPassword?.value) {
      return { passwordMismatch: true };
    }
    return null;
  }

  get emailControl() { return this.signUpForm.get('email')!; }
  get passwordControl() { return this.signUpForm.get('password')!; }
  get confirmPasswordControl() { return this.signUpForm.get('confirmPassword')!; }

  checkPolicy(validator: (password: string) => boolean): boolean {
    return validator(this.passwordControl.value || '');
  }

  onHide(): void {
    this.closeSignUp.emit();
  }

  onSubmit(): void {
    if (this.signUpForm.valid) {
      this.loading = true;
      const { email, password } = this.signUpForm.value;
      
      this.authService.signUp(email, password).subscribe({
        next: () => {
          this.feedbackService.showSuccessToast({detail: "User registered successfully."});
          this.closeSignUp.emit();
        },
        error: (error) => {
          this.loading = false;
          this.feedbackService.showErrorToast({detail: error.message});
        },
        complete: () => {
          this.loading = false;
        }
      });
    } else {
      Object.keys(this.signUpForm.controls).forEach(key => {
        const control = this.signUpForm.get(key);
        if (control?.invalid) {
          control.markAsTouched();
        }
      });
    }
  }
}