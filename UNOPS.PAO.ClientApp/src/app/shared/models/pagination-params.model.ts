import {HttpParams} from '@angular/common/http';

export interface PaginationParams {
  pageIndex?: number;
  pageSize?: number;
  orderBy?: string;
  ascending?: string;
}

export function toHttpParams(params: PaginationParams): HttpParams {
  let httpParams = new HttpParams()
  for (const [key, val] of Object.entries(params) as [string, unknown][]) {
    if (val != null) {
      httpParams = httpParams.set(key, String(val));
    }
  }
  return httpParams;
}
