import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { PartnerService } from '../../../services/partner.service';
import { Partner } from '../../../models/partner.model';

@Component({
  selector: 'app-partner-data',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './partner-data.component.html',
  styleUrls: ['./partner-data.component.scss']
})
export class PartnerDataComponent implements OnInit {
  partnerId: string = '';
  partnerCode: string = '';
  dashboardId: string = 'dcf96b62-ae61-4d6c-8614-34b9faf91cd8';
  isLoading = signal(false);
  dashboardUrl: SafeResourceUrl;

  constructor(
    private route: ActivatedRoute,
    private partnerService: PartnerService,
    private sanitizer: DomSanitizer
  ) {
    this.dashboardUrl = this.sanitizer.bypassSecurityTrustResourceUrl('');
  }

  ngOnInit(): void {
    // Get the recordId from the parent route
    this.route.parent?.paramMap.subscribe({
      next: (paramMap: ParamMap) => {
        this.partnerId = paramMap.get("recordId") || '';
        
        // Check if data is already available from the resolver
        this.route.parent?.data.subscribe(data => {
          if (data['partnerData']) {
            const partnerData: Partner = data['partnerData'];
            this.partnerCode = partnerData.partnerCode || '';
            this.createDashboardUrl();
          } else {
            // Fallback to loading details directly if resolver data isn't available
            this.loadPartnerDetails();
          }
        });
      }
    });
  }

  loadPartnerDetails() {
    this.isLoading.set(true);
    this.partnerService.getPartnerById(this.partnerId).subscribe({
      next: (data: Partner) => {
        this.partnerCode = data.partnerCode || '';
        this.createDashboardUrl();
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading partner details:', err);
        this.isLoading.set(false);
      }
    });
  }
  
  createDashboardUrl() {
    // Create the embed URL with the new dashboard ID and filter format
    const baseUrl = `https://lookerstudio.google.com/embed/reporting/${this.dashboardId}/page/085GF`;
    
    // Create the filter string with the PartnerCode
    const filterValue = `include%EE%80%800%EE%80%80IN%EE%80%80${encodeURIComponent(this.partnerCode)}`;
    const filterJson = `{"df30":"${filterValue}"}`;
    const params = encodeURIComponent(filterJson);
    const embedUrl = `${baseUrl}?params=${params}`;
    
    this.dashboardUrl = this.sanitizer.bypassSecurityTrustResourceUrl(embedUrl);
  }
}
