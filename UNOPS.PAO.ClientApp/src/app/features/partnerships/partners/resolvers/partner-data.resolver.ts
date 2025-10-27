import { Injectable } from '@angular/core';
import { Resolve, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { PartnerService } from '../services/partner.service';
import { Partner } from '../models/partner.model';

@Injectable({
  providedIn: 'root'
})
export class PartnerDataResolver implements Resolve<Partner> {
  constructor(private partnerService: PartnerService) {}

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<Partner> {
    const recordId = route.paramMap.get('recordId') || '';
    return this.partnerService.getPartnerById(recordId);
  }
} 
