import {ChangeDetectionStrategy, Component, inject, signal, ViewChild, WritableSignal} from '@angular/core';
import { Interaction } from '../../../models/interaction.model';
import { InteractionService } from '../../../services/interaction.service';
import { NgIf} from '@angular/common';
import {Button, ButtonDirective} from 'primeng/button';
import { Router, ActivatedRoute} from '@angular/router';
import { InteractionModalComponent } from '../modal/interaction-modal.component';
import { INTERACTION_TYPE_TRANSLATION_KEYS, InteractionType } from '../../../models/interaction-type.enum';
import { TranslateModule } from '@ngx-translate/core';
import { ListviewComponent } from '../../../../../common/pages/components/listview/listview.component';
import { ListViewColumn } from '../../../../../common/pages/components/listview/listview.model';

@Component({
  selector: 'app-interaction-list',
  standalone: true,
  imports: [
    InteractionModalComponent,
    Button,
    NgIf,
    TranslateModule,
    ListviewComponent,
  ],
  templateUrl: './interaction-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InteractionListComponent {
  displayModal = signal(false);
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
    {
      field: 'contactName',
      label: 'label.interaction.contactName',
      sortable: false,
      type: 'text'
    },
    {
      field: 'date',
      label: 'label.interaction.date',
      sortable: true,
      type: 'date'
    },
    {
      field: 'data',
      label: 'label.interaction.notes',
      sortable: false,
      type: 'text'
    }
  ];

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
          this.displayModal.set(true);
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
    this.selectedInteraction.set(undefined);
    this.displayModal.set(true);
  }

  openEditInteractionModal(item: any): void {
    this.interactionService.getById(item.id).subscribe({
      next: (response) => {
        if (response.body) {
          this.selectedInteraction.set(response.body);
          this.displayModal.set(true);
          this.listviewComponent?.refreshData()
        }
      },
      error: (error) => console.error('Error fetching interaction details', error)
    });
  }

  onModalClose(): void {
    this.displayModal.set(false);
    this.router.navigate(['/interactions'], { replaceUrl: true });
  }

  onInteractionDeleted() {
    this.listviewComponent?.refreshData()
  }
}
