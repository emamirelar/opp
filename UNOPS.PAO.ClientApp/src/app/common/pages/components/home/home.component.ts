import { Component, OnInit } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
  standalone: true,
  imports: [TranslateModule]
})
export class HomeComponent implements OnInit {
  constructor() {}
  
  ngOnInit() {
    console.log('[HOME] Home component initialized');
    
    // Check for dev cookie for logging purposes only
    const cookies = document.cookie.split(';').map(c => c.trim());
    const devCookie = cookies.find(c => c.startsWith('dev-user-email='));
    
    if (devCookie) {
      console.log('[HOME] Dev cookie found:', devCookie);
    } else {
      console.log('[HOME] No dev cookie found');
    }
  }
}
