import { Component, Input, OnInit } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-coming-soon',
  templateUrl: './coming-soon.component.html',
  standalone: true,
  imports: [TranslateModule, CommonModule]
})
export class ComingSoonComponent implements OnInit {
  @Input() featureName: string = '';

  constructor(private route: ActivatedRoute) {}

  ngOnInit() {
    if (!this.featureName && this.route.snapshot.data['featureName']) {
      this.featureName = this.route.snapshot.data['featureName'];
    }
  }
}
