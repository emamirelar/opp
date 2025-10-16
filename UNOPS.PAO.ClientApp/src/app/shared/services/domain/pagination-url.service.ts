import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { ActivatedRoute, Router } from "@angular/router";
import { map } from "rxjs/operators";
import {PaginationParams} from '@shared/models/pagination-params.model';


@Injectable({
    providedIn: 'root'
})
export class PaginationUrlService {
    private route = inject(ActivatedRoute);
    private router = inject(Router);

    getCurrentPaginationParams(): Observable<PaginationParams> {
        return this.route.queryParams.pipe(
            map(params => {
                const pageIndex = Number(params['pageIndex']);
                const pageSize = Number(params['pageSize']);
                
                return {
                    pageIndex: isNaN(pageIndex) ? 1 : pageIndex,
                    pageSize: isNaN(pageSize) ? 10 : pageSize,
                    orderBy: params['orderBy'],
                    ascending: params['ascending']?.toString()
                };
            })
        );
    }

    updatePaginationParams(updates: Partial<PaginationParams>): void {
      const navigation = this.router.getCurrentNavigation();
      const currentParams = navigation?.extractedUrl?.queryParams || {};
      const updatedParams = {
          ...currentParams,
          ...updates
      };

      this.router.navigate([], {
          relativeTo: this.route,
          queryParams: updatedParams,
          queryParamsHandling: 'merge'
      });
    }
}
