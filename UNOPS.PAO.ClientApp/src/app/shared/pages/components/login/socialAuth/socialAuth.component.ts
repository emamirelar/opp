import { Component, OnInit } from '@angular/core';

import { HttpClient } from '@angular/common/http';
import {
  SocialAuthService,
  GoogleSigninButtonModule,
} from '@abacritt/angularx-social-login';

import { AuthService } from '@core/services/auth';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-social-auth',
  templateUrl: './socialAuth.component.html',
  styleUrl: './socialAuth.component.css',
  imports: [GoogleSigninButtonModule],
})
export class SociaAuth implements OnInit {
  constructor(
    private socialAuthService: SocialAuthService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.socialAuthService.authState.subscribe(async (user) => {
      try {
        await firstValueFrom(this.authService.googleSignIn(user));
        window.location.href = '/';
      } catch (err) {
        console.error('Google sign-in error:', err);
      }
    });
  }
}
