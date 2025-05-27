import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../essentials/services/auth.service';

@Component({
  selector: 'app-iap-status',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="iap-status">
      <div class="status-section">
        <h3>Authentication Status</h3>
        <div><strong>IAP Authenticated:</strong> {{isIapAuthenticated}}</div>
        <div><strong>Regular Auth:</strong> {{isLoggedIn}}</div>
      </div>
      
      <div class="status-section">
        <h3>Cookie Information</h3>
        <div *ngIf="devCookie"><strong>Dev Cookie:</strong> {{devCookie}}</div>
        <div *ngIf="!devCookie"><strong>Dev Cookie:</strong> Not found</div>
        <div><strong>All Cookies:</strong> <span class="cookie-text">{{allCookies}}</span></div>
      </div>
      
      <div class="status-section" *ngIf="authInfo">
        <h3>User Information</h3>
        <div *ngFor="let key of getObjectKeys(authInfo)">
          <strong>{{key}}:</strong> {{authInfo[key]}}
        </div>
      </div>
    </div>
  `,
  styles: [`
    .iap-status {
      background: #f0f8ff;
      border: 1px solid #cce5ff;
      padding: 15px;
      border-radius: 4px;
      max-width: 600px;
      margin: 0 auto;
    }
    .status-section {
      margin-bottom: 15px;
    }
    .status-section h3 {
      color: #0066cc;
      margin-bottom: 10px;
      border-bottom: 1px solid #cce5ff;
      padding-bottom: 5px;
    }
    .cookie-text {
      word-break: break-all;
      font-size: 0.9em;
    }
  `]
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