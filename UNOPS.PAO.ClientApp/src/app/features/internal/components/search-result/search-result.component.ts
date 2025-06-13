import { ChangeDetectionStrategy, Component, inject, OnInit, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Tab, TabList, TabPanel, TabPanels, Tabs } from 'primeng/tabs';
import { TranslateModule } from '@ngx-translate/core';
import { DialogModule } from 'primeng/dialog';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { BadgeModule } from 'primeng/badge';
import { ActivatedRoute, Router } from '@angular/router';
import { ListViewColumn } from '../../../../common/pages/components/listview/listview.model';
import {
  ListviewComponent
} from '../../../../common/pages/components/listview/listview.component';
import { ContactService } from '../../services/contact.service';
import { PartnerService } from '../../services/partner.service';
import { InteractionService } from '../../services/interaction.service';

interface SearchResult {
  id: string;
  title: string;
  subtitle?: string;
  type: string;
  [key: string]: any;
}

@Component({
  selector: 'app-search-result',
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    Tabs,
    TabList,
    Tab,
    TabPanels,
    TabPanel,
    DialogModule,
    CardModule,
    ButtonModule,
    TableModule,
    BadgeModule,
    ListviewComponent
  ],
  templateUrl: './search-result.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: `
    :host ::ng-deep {
      --p-tabs-tablist-background: transparent;
      
      .p-tabpanels {
        padding: 0;
      }
    }
  `
})
export class SearchResultComponent {
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  searchQuery = signal<string>('');
  searchResults: SearchResult[] = [];

  activeTabValue = signal<string>('0');

  contactResults = signal<SearchResult[]>([]);
  partnerResults = signal<SearchResult[]>([]);
  interactionResults = signal<SearchResult[]>([]);
  totalContactsCount = signal<number>(0);
  totalPartnersCount = signal<number>(0);
  totalInteractionsCount = signal<number>(0);

  constructor() {
    this.route.queryParams.subscribe(params => {
      this.searchQuery.set(params['q'] || '');
    });
  }

  // Contact columns and service
  contactColumns: ListViewColumn[] = [
    { field: 'profilePictureUrl', label: '', type: 'avatar', sortable: false, width: '5%' },
    { field: 'firstName', label: 'label.contact.firstName', type: 'text', sortable: true, width: '15%' },
    { field: 'lastName', label: 'label.contact.lastName', type: 'text', sortable: true, width: '15%' },
    { field: 'email', label: 'label.contact.email', type: 'email', sortable: true, width: '25%' },
    { field: 'mobile', label: 'label.contact.mobile', type: 'text', sortable: true, width: '15%' }
  ];
  contactService = inject(ContactService);

  // Partner columns and service
  partnerColumns: ListViewColumn[] = [
    {
      field: 'logoUrl',
      label: '',
      sortable: false,
      type: 'avatar'
    },
    {
      field: 'name',
      label: 'label.partner.name',
      sortable: false,
      type: 'text'
    },
    {
      field: 'status',
      label: 'label.partner.status',
      sortable: false,
      type: 'conditionalIcon',
      conditionFn: (rowData: any) => rowData.status === 'Active'
    },
    {
      field: 'newEngagement',
      label: 'label.partner.newEngagement',
      sortable: false,
      type: 'conditionalIcon',
      conditionFn: (rowData: any) => rowData.newEngagement === 'Allowed'
    }
  ];
  partnerService = inject(PartnerService);

  // Interaction columns and service
  interactionColumns: ListViewColumn[] = [
    {
      field: 'type',
      label: 'label.interaction.type',
      sortable: true,
      type: 'text'
    },
    {
      field: 'date',
      label: 'label.interaction.date',
      sortable: true,
      type: 'date'
    },
    {
      field: 'subject',
      label: 'label.interaction.subject',
      sortable: true,
      type: 'text'
    },
    {
      field: 'data',
      label: 'label.interaction.description',
      sortable: false,
      type: 'text'
    }
  ];
  interactionService = inject(InteractionService);

  navigateToContact(result: SearchResult) {
    this.router.navigate(['/partnerships/contacts', result.id]);
  }

  navigateToPartner(result: SearchResult) {
    this.router.navigate(['/partner', result.id]);
  }

  navigateToInteraction(result: SearchResult) {
    this.router.navigate(['/interaction', result.id]);
  }

  onTotalContactsChange(count: number) {
    this.totalContactsCount.set(count);
  }

  onTotalPartnersChange(count: number) {
    this.totalPartnersCount.set(count);
  }

  onTotalInteractionsChange(count: number) {
    this.totalInteractionsCount.set(count);
  }
}
