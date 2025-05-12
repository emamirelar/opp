import {ChangeDetectionStrategy, Component, inject, signal, ViewChild, WritableSignal} from '@angular/core';
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
export class InteractionListComponent {
  selectedInteraction: WritableSignal<Interaction | undefined> = signal(undefined);

  @ViewChild("listviewComponent")
  listviewComponent?: ListviewComponent;

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
    private router: Router,
    private route: ActivatedRoute,
  ) {
    this.openModalFromRoute();
    this.setInteractionFromHistoryState();
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
    this.openInteractionModal();
  }

  openEditInteractionModal(item: any): void {
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
