import {inject, Injectable, signal, WritableSignal} from '@angular/core';
import {Interaction} from '../../../models/interaction.model';
import {InteractionService} from '../../../services/interaction.service';
import { PaginationUrlService } from '../../../services/pagination-url.service';
import {PaginationParams} from '../../../../../common/models/pagination-params.model';

const defaultPaginationParams: PaginationParams = {
  pageIndex: 1,
  pageSize: 10
}

@Injectable()
export class InteractionListData {

  isLoading = signal(false);
  interactions: WritableSignal<Interaction[]> = signal([]);
  totalCount: WritableSignal<number> = signal(0);

  pageParams: WritableSignal<PaginationParams> = signal(defaultPaginationParams);

  private interactionService = inject(InteractionService);
  private paginationUrlService = inject(PaginationUrlService);

  initialLoad() {
    this.paginationUrlService.getCurrentPaginationParams().subscribe((params) => {
      this.loadInteractions(params);
    }).unsubscribe();
  }

  onPageChange(event: any) {
    const ascending = event.sortOrder === 1 ? "true" : "false";
    this.loadInteractions({
      pageIndex: Math.trunc(event.first / event.rows) + 1,
      pageSize: event.rows,
      orderBy: event.sortField,
      ascending
    });
  }

  first() : number {
    return (this.pageParams()?.pageIndex ?? 0) * (this.pageParams()?.pageSize ?? 0);
  }

  loadInteractions(params? : PaginationParams) {
    params = params || this.pageParams() || defaultPaginationParams;
    this.isLoading.set(true);
    return this.interactionService.getAll(params).subscribe({
        next: (response) => {
          this.interactions.set(response.body?.records || []);
          this.totalCount.set(response.body?.totalCount || 0);
          this.paginationUrlService.updatePaginationParams(params as PaginationParams);
          this.pageParams.set(params as PaginationParams);
          this.isLoading.set(false);
        },
        error: (error) => {
          console.error('Error loading interactions', error);
          this.isLoading.set(false);
        }
      }
    );
  }
}
