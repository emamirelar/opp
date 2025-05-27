import {ChangeDetectionStrategy, Component, inject, signal, ViewChild, WritableSignal, ChangeDetectorRef, OnInit, OnDestroy} from '@angular/core';
import { Interaction } from '../../../models/interaction.model';
import { InteractionService } from '../../../services/interaction.service';
import { NgIf} from '@angular/common';
import {Button, ButtonDirective} from 'primeng/button';
import { Router, ActivatedRoute} from '@angular/router';
import { InteractionModalComponent } from '../modal/interaction-modal.component';
import { INTERACTION_TYPE_TRANSLATION_KEYS, InteractionType } from '../../../models/interaction-type.enum';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ListviewComponent } from '../../../../../common/pages/components/listview/listview.component';
import { ListViewColumn } from '../../../../../common/pages/components/listview/listview.model';
import { DialogService } from 'primeng/dynamicdialog';
import { PermissionUtilityService } from '../../../../../essentials/services/permission-utility.service';
import { FeedbackDialogService } from '../../../../../common/reusables/services/feedback-dialog.service';

@Component({
  selector: 'app-interaction-list',
  standalone: true,
  imports: [
    Button,
    NgIf,
    TranslateModule,
    ListviewComponent,
  ],
  providers: [
    DialogService
  ],
  templateUrl: './interaction-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InteractionListComponent implements OnInit, OnDestroy {
  selectedInteraction: WritableSignal<Interaction | undefined> = signal(undefined);

  @ViewChild("listviewComponent")
  listviewComponent?: ListviewComponent;

  // Inject services
  router = inject(Router);
  route = inject(ActivatedRoute);
  permissionUtilityService = inject(PermissionUtilityService);
  feedbackDialogService = inject(FeedbackDialogService);
  cdr = inject(ChangeDetectorRef);

  // Permission handling
  private permissionUtils = this.permissionUtilityService.createEntityPermissions('Interaction');
  entityPermissions = this.permissionUtils.entityPermissions;
  permissionsLoading = this.permissionUtils.permissionsLoading;

  columns: ListViewColumn[] = [
    {
      field: 'type',
      label: 'label.interaction.type',
      sortable: true,
      type: 'text'
    },
    /*{
      field: 'contactName',
      label: 'label.interaction.contactName',
      sortable: false,
      type: 'text'
    },*/
    {
      field: 'date',
      label: 'label.interaction.date',
      sortable: true,
      type: 'date'
    },
    {
      field: 'subject',
      label: 'label.interaction.subject',
      sortable: false,
      type: 'text'
    },
    {
      field: 'data',
      label: 'label.interaction.description',
      sortable: false,
      type: 'text'
    }
  ];

  private dialogService = inject(DialogService);
  private translateService = inject(TranslateService);

  constructor(
    private interactionService: InteractionService,
  ) {
    this.openModalFromRoute();
    this.setInteractionFromHistoryState();
  }

  ngOnInit() {
    // Load permissions using utility service
    this.permissionUtils.loadPermissions(this.router, this.cdr);
  }

  ngOnDestroy() {
    // No need to clear caches manually - utility service handles this
  }

  private setInteractionFromHistoryState() {
    this.route.queryParams.subscribe(params => {
      if (params['openNewDialog'] === 'true') {
        const state = history.state;
        if (state?.data) {
          this.selectedInteraction.set(state.data);
          this.openInteractionModal(state.data);
        }
      }
    });
  }

  openModalFromRoute(): void {
    this.route.params.subscribe(params => {
      const interactionId = params['id'];
      if (interactionId) {
        this.interactionService.getById(interactionId).subscribe(
          (response) => {
            if (response.body) {
              this.openEditInteractionModal(response.body);
            }
          }
        );
      }
    });
  }

  openNewInteractionModal(): void {
    // Check if user has create permission
    if (!this.permissionUtilityService.canCreate(this.entityPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: 'You do not have permission to create interactions',
        summary: 'Permission Denied'
      });
      return;
    }
    
    this.openInteractionModal();
  }

  openEditInteractionModal(item: any): void {
    // Check if user has update permission
    if (!this.permissionUtilityService.canUpdate(this.entityPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: 'You do not have permission to edit interactions',
        summary: 'Permission Denied'
      });
      return;
    }
    
    this.interactionService.getById(item.id).subscribe({
      next: (response) => {
        if (response.body) {
          this.selectedInteraction.set(response.body);
          this.openInteractionModal(response.body);
        }
      },
      error: (error) => console.error('Error fetching interaction details', error)
    });
  }

  private openInteractionModal(record?: Interaction): void {
    const dialogRef = this.dialogService.open(InteractionModalComponent, {
      header: this.translateService.instant(record?.id ? 'title.editInteraction' : 'title.newInteraction'),
      width: '50rem',
      breakpoints: { '1199px': '95vw' },
      data: {
        record: record
      }
    });

    dialogRef.onClose.subscribe(result => {
      if (result) {
        this.listviewComponent?.refreshData();
      }
      
      // Navigate back to interactions list without the id param
      this.router.navigate(['/interactions'], { replaceUrl: true });
    });
  }
}
