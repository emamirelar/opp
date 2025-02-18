import { Component, OnInit } from '@angular/core';

import { HttpClient } from '@angular/common/http';
import {
  SocialAuthService,
  GoogleSigninButtonModule,
} from '@abacritt/angularx-social-login';

import { AuthService } from '../../../../../essentials/services/auth.service';

@Component({
  selector: 'app-social-auth',
  templateUrl: './socialAuth.component.html',
  styleUrl: './socialAuth.component.css',
  imports: [GoogleSigninButtonModule],
})
export class SociaAuth implements OnInit {
  constructor(
    private socialAuthService: SocialAuthService,
    private authService: AuthService,
  ) {}

  ngOnInit(): void {
    this.socialAuthService.authState.subscribe((user) => {
      this.authService.googleSignIn(user).subscribe({
        next: (res) => {
          window.location.href = '/';
        },
        error: (err) => {},
      });
    });
  }
}
