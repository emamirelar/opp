import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import {Button} from 'primeng/button';

@Component({
  selector: 'app-go-back',
  standalone: true,
  imports: [TranslateModule, Button],
  template: `
    <p-button
      type="button"
      icon="pi pi-arrow-left"
      text rounded
      [label]=" 'button.back' | translate "
      (click)="goBack()">
    </p-button>
  `,
  styles: []
})
export class GoBackComponent implements OnInit {
  private router = inject(Router);

  private previousUrl?: string;

  ngOnInit(): void {
    const previousUrl = history.state?.previousUrl;
    this.previousUrl = previousUrl || undefined;
  }

  goBack() {
    if (this.previousUrl) {
      this.router.navigateByUrl(this.previousUrl);
    } else {
      window.history.back();
    }
  }
}
