import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-lookerstudio',
  imports: [CommonModule],
  templateUrl: './lookerstudio.component.html',
  styleUrl: './lookerstudio.component.scss'
})
export class LookerstudioComponent implements OnChanges {
  @Input() dashboardId: string = '';
  @Input() partnerCode: string = '';
  @Input() isLoading: boolean = false;
  @Input() minHeight: string = 'calc(100vh - 300px)'; // Default min-height
  
  dashboardUrl: SafeResourceUrl;

  constructor(private sanitizer: DomSanitizer) {
    this.dashboardUrl = this.sanitizer.bypassSecurityTrustResourceUrl('');
  }

  ngOnChanges(changes: SimpleChanges): void {
    if ((changes['dashboardId'] || changes['partnerCode']) && this.dashboardId && this.partnerCode) {
      this.createDashboardUrl();
    }
  }

  createDashboardUrl() {
    // Create the embed URL with the dashboard ID and filter format
    const baseUrl = `https://lookerstudio.google.com/embed/reporting/${this.dashboardId}/page/085GF`;
    
    // Create the filter string with the PartnerCode
    const filterValue = `include%EE%80%800%EE%80%80IN%EE%80%80${encodeURIComponent(this.partnerCode)}`;
    const filterJson = `{"df30":"${filterValue}"}`;
    const params = encodeURIComponent(filterJson);
    const embedUrl = `${baseUrl}?params=${params}`;
    
    this.dashboardUrl = this.sanitizer.bypassSecurityTrustResourceUrl(embedUrl);
  }
}
