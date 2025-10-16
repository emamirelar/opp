import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { AuthService } from '@core/services/auth';

@Component({
  selector: 'app-iap-status',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './iap-status.component.html',
  styleUrls: ['./iap-status.component.scss']
})
export class IapStatusComponent implements OnInit {
  isIapAuthenticated = false;
  isLoggedIn = false;
  allCookies = '';
  devCookie: string | null = null;
  authInfo: any = null;

  constructor(private authService: AuthService) {}

  ngOnInit() {
    // Get cookie information
    this.allCookies = document.cookie;
    const cookies = document.cookie.split(';').map(c => c.trim());
    this.devCookie = cookies.find(c => c.startsWith('dev-user-email=')) || null;
    
    // Check IAP authentication
    this.authService.isIapAuthenticated().subscribe(isAuth => {
      this.isIapAuthenticated = isAuth;
    });
    
    // Check regular authentication
    this.authService.isLogedIn().subscribe(isAuth => {
      this.isLoggedIn = isAuth;
    });
    
    // Get auth info
    this.authService.getAuthInfo().subscribe(
      info => this.authInfo = info,
      err => console.error('Error getting auth info:', err)
    );
  }
  
  getObjectKeys(obj: any): string[] {
    return obj ? Object.keys(obj) : [];
  }
} 
