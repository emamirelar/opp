import { Component, Input, OnChanges, OnInit, SimpleChanges, ChangeDetectionStrategy } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-lookerstudio',
  imports: [CommonModule],
  templateUrl: './lookerstudio.component.html',
  styleUrl: './lookerstudio.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LookerstudioComponent implements OnChanges, OnInit {
  @Input() dashboardId: string = '';
  @Input() partnerCode: string = '';
  @Input() isLoading: boolean = false;
  @Input() minHeight: string = 'calc(100vh - 300px)'; // Default min-height

  @Input() type : 'partnerTree' | 'partner' = 'partnerTree';

  url: SafeResourceUrl;

  constructor(private sanitizer: DomSanitizer) {
    this.url = this.sanitizer.bypassSecurityTrustResourceUrl('');
  }

  ngOnInit(): void {
    this.updateUrl();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['dashboardId'] || changes['partnerCode'] || changes['type']) {
      this.updateUrl();
    }
  }

  updateUrl(): void {
    if (this.partnerCode) {
      this.url = this.type === 'partner' ? this.partnerUrl() : this.partnerTreeUrl();
    }
  }

  partnerTreeUrl(): SafeResourceUrl {
    if (this.dashboardId) {
      const baseUrl = `https://lookerstudio.google.com/embed/reporting/${this.dashboardId}/page/085GF`;
      const filterValue = `include%EE%80%800%EE%80%80IN%EE%80%80${encodeURIComponent(this.partnerCode)}`;
      const filterJson = `{"df30":"${filterValue}"}`;
      const params = encodeURIComponent(filterJson);
      const embedUrl = `${baseUrl}?params=${params}`;
      return this.sanitizer.bypassSecurityTrustResourceUrl(embedUrl);
    }
    return this.sanitizer.bypassSecurityTrustResourceUrl('');
  }

  partnerUrl(): SafeResourceUrl {
    const parnterUrl = `https://lookerstudio.google.com/embed/reporting/dcf96b62-ae61-4d6c-8614-34b9faf91cd8/page/p_d0oidwu2rd?params=%7B%22df30%22:%22include%25EE%2580%25800%25EE%2580%2580IN%25EE%2580%2580${encodeURIComponent(this.partnerCode)}%22,%22df25%22:%22include%25EE%2580%25800%25EE%2580%2580IN%25EE%2580%2580${encodeURIComponent(this.partnerCode)}%22%7D`;
    return this.sanitizer.bypassSecurityTrustResourceUrl(parnterUrl);
  }



}
