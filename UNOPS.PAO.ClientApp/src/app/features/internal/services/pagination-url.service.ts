import { inject, Injectable } from "@angular/core";
import { Observable, of } from "rxjs";
import { ActivatedRoute, Params, Router } from "@angular/router";
import { map } from "rxjs/operators";
import {PaginationParams} from '../../../common/models/pagination-params.model';


@Injectable({
    providedIn: 'root'
})
export class PaginationUrlService {
    private route = inject(ActivatedRoute);
    private router = inject(Router);

    getCurrentPaginationParams(): Observable<PaginationParams> {
        return this.route.queryParams.pipe(
            map(params => ({
                pageIndex: Number(params['pageIndex'] || 1),
                pageSize: Number(params['pageSize'] || 10),
                orderBy: params['orderBy'],
                ascending: params['ascending']?.toString()
            }))
        );
    }

    updatePaginationParams(updates: Partial<PaginationParams>): void {
      const currentParams = { ...this.router.getCurrentNavigation()?.extractedUrl.queryParams };
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
