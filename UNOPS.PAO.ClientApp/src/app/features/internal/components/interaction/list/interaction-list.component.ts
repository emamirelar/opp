import {ChangeDetectionStrategy, Component, inject, OnInit, signal, WritableSignal} from '@angular/core';
import { Interaction } from '../../../models/interaction.model';
import { InteractionService } from '../../../services/interaction.service';
import {TableModule} from 'primeng/table';
import {DatePipe, JsonPipe, NgIf} from '@angular/common';
import {Button, ButtonDirective, ButtonIcon, ButtonLabel} from 'primeng/button';
import {RouterLink, Router, ActivatedRoute} from '@angular/router';
import {InteractionModalComponent} from '../modal/interaction-modal.component';
import { INTERACTION_TYPE_TRANSLATION_KEYS, InteractionType } from '../../../models/interaction-type.enum';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import {InteractionListData} from './interaction-list.data';

@Component({
  selector: 'app-interaction-list',
  standalone: true,
  imports: [
    TableModule,
    DatePipe,
    ButtonDirective,
    ButtonIcon,
    RouterLink,
    InteractionModalComponent,
    ButtonLabel,
    Button,
    NgIf,
    JsonPipe,
    TranslateModule
  ],
  templateUrl: './interaction-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [InteractionListData]
})
export class InteractionListComponent implements OnInit {
  displayModal = signal(false);
  selectedInteraction: WritableSignal<Interaction | undefined> = signal(undefined);

  public interactionListData = inject(InteractionListData);

  constructor(
    private interactionService: InteractionService,
    private router: Router,
    private route: ActivatedRoute,
  ) {}

  ngOnInit(): void {
    this.interactionListData.initialLoad();
    this.openModalFromRoute();
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
        }
      },
      error: (error) => console.error('Error fetching interaction details', error)
    });
  }

  onModalClose(): void {
    this.displayModal.set(false);
    this.interactionListData.loadInteractions();
    this.router.navigate(['/interactions'], { replaceUrl: true });
  }

  onInteractionDeleted() {
    this.interactionListData.loadInteractions();
  }

  getInteractionTypeTranslationKey(type: InteractionType): string {
    return INTERACTION_TYPE_TRANSLATION_KEYS[type];
  }

  getContactName(interaction: Interaction): string {
    return interaction.contactName || `Contact ${interaction.contactId}`;
  }
}
