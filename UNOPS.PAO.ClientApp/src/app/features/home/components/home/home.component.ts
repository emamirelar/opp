import { Component, OnInit } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { HomeDashboardComponent } from '../home-dashboard/home-dashboard.component';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
  standalone: true,
  imports: [TranslateModule, HomeDashboardComponent]
})
export class HomeComponent implements OnInit {
  constructor() {}
  
  ngOnInit() {
    // Check for dev cookie for logging purposes only
    const cookies = document.cookie.split(';').map(c => c.trim());
    const devCookie = cookies.find(c => c.startsWith('dev-user-email='));
    
    if (devCookie) {
      
    } else {
      
    }
  }
}
