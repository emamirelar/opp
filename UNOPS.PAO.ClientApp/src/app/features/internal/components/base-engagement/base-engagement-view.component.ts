import {
  ChangeDetectionStrategy,
  Component,
  inject,
  OnInit,
  signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';

// PrimeNG Modules
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { SkeletonModule } from 'primeng/skeleton';
import { TableModule } from 'primeng/table';

// Services and Models
import { BaseEngagementService } from '../../services/base-engagement.service';
import { BaseEngagement, StageSeverity } from '../../models/base-engagement.model';

@Component({
  selector: 'app-base-engagement-view',
  standalone: true,
  imports: [
    CommonModule,
    PanelModule,
    ButtonModule,
    TagModule,
    DividerModule,
    SkeletonModule,
    TableModule
  ],
  template: `
    <div class="base-engagement-view" *ngIf="!loading(); else loadingTemplate">
      <!-- Header -->
      <div class="flex justify-between items-center mb-6">
        <div class="flex items-center gap-3">
          <p-button 
            icon="pi pi-arrow-left"
            [rounded]="true"
            [text]="true"
            (onClick)="onBack()">
          </p-button>
          <h1 class="text-2xl font-semibold">{{ engagement()?.displayName }}</h1>
        </div>
      </div>

      <!-- Engagement Details -->
      <p-panel header="Engagement Information" styleClass="mb-6">
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6" *ngIf="engagement() as eng">
          <div class="field-item">
            <label class="font-medium text-surface-700">Engagement ID</label>
            <p>{{ eng.engagementNumber }}</p>
          </div>
          <div class="field-item">
            <label class="font-medium text-surface-700">Stage</label>
            <p>
              <p-tag [value]="eng.stageDisplay" [severity]="getStageSeverity(eng.engagementStage)"></p-tag>
            </p>
          </div>
          <div class="field-item">
            <label class="font-medium text-surface-700">Duration</label>
            <p>{{ eng.durationDisplay }}</p>
          </div>
          <div class="field-item" *ngIf="eng.engagementAmount">
            <label class="font-medium text-surface-700">Budget</label>
            <p>{{ eng.budgetDisplay }}</p>
          </div>
          <div class="field-item" *ngIf="eng.businessDeveloperName">
            <label class="font-medium text-surface-700">Business Developer</label>
            <p>{{ eng.businessDeveloperDisplay }}</p>
          </div>
          <div class="field-item" *ngIf="eng.engagementProjectExecutiveName">
            <label class="font-medium text-surface-700">Project Executive</label>
            <p>{{ eng.engagementProjectExecutiveName }}</p>
          </div>
          <div class="field-item" *ngIf="eng.implementationCountriesList">
            <label class="font-medium text-surface-700">Implementation Countries</label>
            <p>{{ eng.implementationCountriesList }}</p>
          </div>
          <div class="field-item" *ngIf="eng.outputsList">
            <label class="font-medium text-surface-700">Outputs</label>
            <p>{{ eng.outputsList }}</p>
          </div>
          <div class="field-item" *ngIf="eng.sdgList">
            <label class="font-medium text-surface-700">SDGs</label>
            <p>{{ eng.sdgList }}</p>
          </div>
          <div class="field-item col-span-full" *ngIf="eng.engagementDescription">
            <label class="font-medium text-surface-700">Description</label>
            <p>{{ eng.engagementDescription }}</p>
          </div>
          <div class="field-item col-span-full" *ngIf="eng.engagementLongDescription">
            <label class="font-medium text-surface-700">Detailed Description</label>
            <p>{{ eng.engagementLongDescription }}</p>
          </div>
        </div>
      </p-panel>

      <!-- Partners -->
      <p-panel header="Partners" *ngIf="engagement()?.partners && engagement()!.partners.length > 0">
        <p-table [value]="engagement()!.partners" styleClass="p-datatable-sm">
          <ng-template pTemplate="header">
            <tr>
              <th>Partner</th>
              <th>Type</th>
              <th>Description</th>
            </tr>
          </ng-template>
          <ng-template pTemplate="body" let-partner>
            <tr>
              <td class="font-medium">{{ partner.partnerDisplayName }}</td>
              <td>
                <span class="px-2 py-1 rounded text-sm" 
                      [style.background-color]="getPartnerTypeColor(partner.partnerType) + '20'"
                      [style.color]="getPartnerTypeColor(partner.partnerType)">
                  {{ partner.partnerTypeDisplay }}
                </span>
              </td>
              <td class="text-sm">
                {{ partner.partnerDescription || '-' }}
              </td>
            </tr>
          </ng-template>
        </p-table>
      </p-panel>
    </div>

    <!-- Loading Template -->
    <ng-template #loadingTemplate>
      <div class="space-y-6">
        <p-skeleton height="2rem" width="300px"></p-skeleton>
        <p-skeleton height="300px"></p-skeleton>
        <p-skeleton height="200px"></p-skeleton>
      </div>
    </ng-template>
  `,
  styles: [`
    .field-item {
      @apply flex flex-col gap-1;
    }
    .field-item label {
      @apply text-sm;
    }
    .field-item p {
      @apply text-surface-900;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BaseEngagementViewComponent implements OnInit {
  // Signals for reactive state
  engagement = signal<BaseEngagement | null>(null);
  loading = signal<boolean>(false);

  // Services
  private baseEngagementService = inject(BaseEngagementService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const id = Number(params.get('id'));
      if (id && id > 0) {
        this.loadEngagement(id);
      }
    });
  }

  private async loadEngagement(id: number) {
    this.loading.set(true);
    try {
      const engagement = await this.baseEngagementService.getBaseEngagementById(id).toPromise();
      this.engagement.set(engagement || null);
    } catch (error) {
      console.error('Error loading engagement:', error);
      this.router.navigate(['/base-engagements']);
    } finally {
      this.loading.set(false);
    }
  }

  onBack() {
    this.router.navigate(['/base-engagements']);
  }

  getStageSeverity(stage?: string): StageSeverity {
    return this.baseEngagementService.getStageSeverity(stage || '');
  }

  getPartnerTypeColor(partnerType?: string): string {
    return this.baseEngagementService.getPartnerTypeColor(partnerType || '');
  }
}
