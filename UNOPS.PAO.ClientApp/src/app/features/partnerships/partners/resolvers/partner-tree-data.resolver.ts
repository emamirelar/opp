import { Injectable } from '@angular/core';
import { Resolve, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { PartnerTreeService } from '../services/partner-tree.service';
import { PartnerTree } from '../models/partner-tree.model';

@Injectable({
  providedIn: 'root'
})
export class PartnerTreeDataResolver implements Resolve<PartnerTree> {
  constructor(private partnerTreeService: PartnerTreeService) {}

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<PartnerTree> {
    const recordId = route.paramMap.get('recordId') || '';
    return this.partnerTreeService.getPartnerTreeDataById(recordId);
  }
} 
