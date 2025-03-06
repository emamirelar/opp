import {ChangeDetectionStrategy, Component, inject, OnInit, signal, WritableSignal} from '@angular/core';
import { Interaction } from '../../../models/interaction.model';
import { InteractionService } from '../../../services/interaction.service';
import {TableModule} from 'primeng/table';
import {DatePipe, JsonPipe, NgIf} from '@angular/common';
import {Button, ButtonDirective, ButtonIcon, ButtonLabel} from 'primeng/button';
import {RouterLink, Router, ActivatedRoute} from '@angular/router';
import {InteractionModalComponent} from '../modal/interaction-modal.component';
import { INTERACTION_TYPE_TRANSLATION_KEYS, InteractionType } from '../../../models/interaction-type.enum';
import { TranslateModule } from '@ngx-translate/core';
import { Location } from '@angular/common';

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
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InteractionListComponent implements OnInit {
  interactions: WritableSignal<Interaction[]> = signal([]);
  displayModal = signal(false);
  selectedInteraction: WritableSignal<Interaction | undefined> = signal(undefined);
  loading = signal(false);
  location = inject(Location);

  constructor(
    private interactionService: InteractionService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.loadInteractions();
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

  loadInteractions() {
    this.loading.set(true);
    this.interactionService.getAll().subscribe(
      (response) => {
        this.interactions.set(response.body || []);
        this.loading.set(false);
      }
    );
  }

  openNewInteractionModal(): void {
    this.selectedInteraction.set(undefined);
    this.displayModal.set(true);
  }

  openEditInteractionModal(interaction: Interaction, updateUrl = false): void {
    this.selectedInteraction.set(interaction);
    this.displayModal.set(true);
    if (updateUrl) {
      this.location.go(`/interactions/${interaction.id}`);
    }
  }

  onModalClose(): void {
    this.displayModal.set(false);
    this.loadInteractions();
    this.router.navigate(['/interactions'], { replaceUrl: true });
  }

  onInteractionDeleted() {
    this.loadInteractions();
  }

  getInteractionTypeTranslationKey(type: InteractionType): string {
    return INTERACTION_TYPE_TRANSLATION_KEYS[type];
  }

  getContactName(interaction: Interaction): string {
    return interaction.contactName || `Contact ${interaction.contactId}`;
  }
}
