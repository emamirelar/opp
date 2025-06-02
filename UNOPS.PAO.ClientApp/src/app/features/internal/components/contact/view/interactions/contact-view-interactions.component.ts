import {Component, Input, OnInit, signal} from '@angular/core';
import {Panel} from 'primeng/panel';
import {CardModule} from 'primeng/card';
import {DividerModule} from 'primeng/divider';
import {ButtonModule} from 'primeng/button';
import {TooltipModule} from 'primeng/tooltip';
import {DynamicDialogModule} from 'primeng/dynamicdialog';
import {DialogService, DynamicDialogRef} from 'primeng/dynamicdialog';
import {InteractionModalComponent} from '../../../interaction/modal/interaction-modal.component';
import {ContactViewInteractionsDialogComponent} from './dialog/contact-view-interactions-dialog.component';
import {ContactViewInteractionsItemComponent} from './item/contact-view-interactions-item.component';
import {map} from 'rxjs/operators';
import {InteractionService} from '../../../../services/interaction.service';
import {Interaction as InteractionModel} from '../../../../models/interaction.model';
import {GroupedInteraction, InteractionViewModel} from './interaction-view.model';

@Component({
  selector: 'app-contact-view-interactions',
  standalone: true,
  imports: [
    Panel,
    CardModule,
    DividerModule,
    ButtonModule,
    TooltipModule,
    DynamicDialogModule,
    ContactViewInteractionsItemComponent
  ],
  templateUrl: './contact-view-interactions.component.html',
  providers: [DialogService]
})
export class ContactViewInteractionsComponent implements OnInit {
  @Input() contactId!: string;
  @Input() disabled: boolean = false;

  private dialogRef: DynamicDialogRef | null = null;
  interactions: InteractionViewModel[] = [];
  isLoading = signal<boolean>(false);

  constructor(
    private dialogService: DialogService,
    private interactionService: InteractionService
  ) {}

  ngOnInit() {
    if (this.contactId) {
      this.loadInteractions();
    }
  }

  get groupedInteractions(): GroupedInteraction[] {
    const interactions = [...this.interactions];

    const grouped = interactions.reduce((acc, interaction) => {
      const date = new Date(interaction.date);
      const month = date.toLocaleString('default', { month: 'long' });
      const year = date.getFullYear();
      const key = `${month}-${year}`;

      if (!acc[key]) {
        acc[key] = {
          month,
          year,
          interactions: []
        };
      }
      acc[key].interactions.push(interaction);
      return acc;
    }, {} as Record<string, GroupedInteraction>);

    return Object.values(grouped).sort((a, b) => {
      const dateA = new Date(a.year, new Date(`${a.month} 1`).getMonth());
      const dateB = new Date(b.year, new Date(`${b.month} 1`).getMonth());
      return dateB.getTime() - dateA.getTime();
    });
  }

  getCurrentMonth(): string {
    return new Date().toLocaleString('default', { month: 'long' });
  }

  getCurrentYear(): number {
    return new Date().getFullYear();
  }

  isPreviousMonth(month: string, year: number): boolean {
    const today = new Date();
    const lastMonth = new Date(today.getFullYear(), today.getMonth() - 1);
    return month === lastMonth.toLocaleString('default', { month: 'long' }) && year === lastMonth.getFullYear();
  }

  isOlderMonth(month: string, year: number): boolean {
    return !this.isPreviousMonth(month, year) &&
           (year < this.getCurrentYear() ||
           (year === this.getCurrentYear() && new Date(`${month} 1`).getMonth() < new Date().getMonth() - 1));
  }

  getMonthsAgo(month: string, year: number): number {
    const today = new Date();
    const given = new Date(year, new Date(`${month} 1`).getMonth());
    const diffMonths = (today.getFullYear() - given.getFullYear()) * 12 + (today.getMonth() - given.getMonth());
    return diffMonths;
  }

  openInteractionModal(interaction: InteractionViewModel): void {
    this.dialogRef = this.dialogService.open(InteractionModalComponent, {
      header: 'Interaction Details',
      width: '50rem',
      breakpoints: {'1199px': '95vw'},
      data: {
        record: interaction
      }
    });

    this.dialogRef.onClose.subscribe(result => {
      if (result) {
        this.loadInteractions();
      }
    });
  }

  openFullScreenInteractions(): void {
    this.dialogRef = this.dialogService.open(ContactViewInteractionsDialogComponent, {
      header: 'Interactions',
      width: '90vw',
      height: '90vh',
      closable: true,
      style: { maxWidth: '800px' },
      data: {
        contactId: this.contactId
      }
    });

    this.dialogRef.onClose.subscribe(result => {
      if (result) {
        this.loadInteractions();
      }
    });
  }

  private mapToViewModel(interaction: InteractionModel): InteractionViewModel {
    const limitWords = (text: string, limit: number = 20): string => {
      if (!text) return '';
      const words = text.split(' ');
      if (words.length <= limit) return text;
      return words.slice(0, limit).join(' ') + '...';
    };

    return {
      id: interaction.id,
      type: interaction.type.toString(),
      date: new Date(interaction.date),
      description: limitWords(interaction.description || ''),
      status: interaction.status,
    };
  }

  loadInteractions(): void {
    if (!this.contactId) {
      return;
    }

    this.isLoading.set(true);

    this.interactionService.getAll({
      contactId: Number(this.contactId),
      pageSize: 5
    })
      .pipe(
        map(response => response.body?.records.map(i => this.mapToViewModel(i)) || []),
      )
      .subscribe(data => {
        this.interactions = data;
        this.isLoading.set(false);
      });
  }


}

