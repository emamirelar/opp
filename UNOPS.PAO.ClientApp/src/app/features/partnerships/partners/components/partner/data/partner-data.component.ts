import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { CommonModule } from '@angular/common';
import { PartnerService } from '@partnerships/partners/services/partner.service';
import { Partner } from '@partnerships/partners/models/partner.model';
import { LookerstudioComponent } from '@shared/components/analytics/lookerstudio/lookerstudio.component';

@Component({
  selector: 'app-partner-data',
  standalone: true,
  imports: [CommonModule, LookerstudioComponent],
  templateUrl: './partner-data.component.html'
})
export class PartnerDataComponent implements OnInit {
  partnerId: string = '';
  partnerCode: string = '';
  dashboardId: string = 'dcf96b62-ae61-4d6c-8614-34b9faf91cd8';
  isLoading = signal(false);

  constructor(
    private route: ActivatedRoute,
    private partnerService: PartnerService
  ) {}

  ngOnInit(): void {

    // Get the recordId from the parent route
    this.route.parent?.paramMap.subscribe({
      next: (paramMap: ParamMap) => {
        this.partnerId = paramMap.get("recordId") || '';

        if (this.partnerId) {
          // Check if data is already available from the resolver
          this.route.parent?.data.subscribe(data => {
            if (data['erpDimValue']) {
              // Use resolved data (more efficient)
              const partnerData = data['erpDimValue'];
              this.partnerCode = partnerData.erpDimValue || '';
              this.isLoading.set(false);
            } else {
              this.loadPartnerDetails();
            }
          });
        }
      }
    });
  }

  loadPartnerDetails() {
    this.isLoading.set(true);

    this.partnerService.getPartnerById(this.partnerId).subscribe({
      next: (data: Partner) => {
        this.partnerCode = data.erpDimValue || '';
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
      }
    });
  }
}
