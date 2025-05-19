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
  templateUrl: './partner-data.component.html'
})
export class PartnerDataComponent implements OnInit {
  partnerId: string = '';
  partnerName: string = '';
  dashboardId: string = 'e5ec2e1c-db96-4e34-a250-2d28ace5b053';
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
            this.partnerName = partnerData.name || '';
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
        this.partnerName = data.name || '';
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
    // Create the embed URL with the appropriate filters using the correct format
    const baseUrl = `https://lookerstudio.google.com/embed/reporting/${this.dashboardId}/page/t2gSC`;
    
    // Create the filter string with pre-encoded special characters
    const filterValue = `include%EE%80%800%EE%80%80IN%EE%80%80${encodeURIComponent(this.partnerName)}`;
    const filterJson = `{"df166":"${filterValue}"}`;
    const params = encodeURIComponent(filterJson);
    const embedUrl = `${baseUrl}?params=${params}`;
    
    this.dashboardUrl = this.sanitizer.bypassSecurityTrustResourceUrl(embedUrl);
  }
}
