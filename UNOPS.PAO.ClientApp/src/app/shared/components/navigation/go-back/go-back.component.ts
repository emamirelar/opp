import { Component, OnInit, inject, ChangeDetectionStrategy } from '@angular/core';
import { Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import {Button} from 'primeng/button';

@Component({
  selector: 'app-go-back',
  standalone: true,
  imports: [TranslateModule, Button],
  templateUrl: './go-back.component.html',
  styleUrls: ['./go-back.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
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
