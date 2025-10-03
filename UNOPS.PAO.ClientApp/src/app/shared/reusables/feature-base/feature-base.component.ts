import { Component, ChangeDetectionStrategy, ChangeDetectorRef, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { LanguageService } from '@shared/services/language.service';
import { Subscription } from 'rxjs';
import { DialogModule } from 'primeng/dialog';
import { FeedbackDialogService } from '../../services/feedback-dialog.service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

export interface ColumnDefinition {
  label: string,
  id: string,
  editable?: boolean | ((node: any) => boolean),
}

@Component({
  selector: 'app-feature-base',
  imports: [DialogModule, TranslateModule],
  templateUrl: './feature-base.component.html',
  styleUrl: './feature-base.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FeatureBaseComponent implements OnInit{
  langChangeSubscription: Subscription = new Subscription;
  router = inject(Router);
  activatedRoute = inject(ActivatedRoute);
  columns = signal<ColumnDefinition[]>([]);
  isDataLoading: any;
  service: any;
  data: any;
  languageService = inject(LanguageService);
  translateService = inject(TranslateService);
  cdr = inject(ChangeDetectorRef);
  feedbackDialogService = inject(FeedbackDialogService);

  getColumns(): ColumnDefinition[] {
    return [];
  }

  ngOnInit() {

  }
}
