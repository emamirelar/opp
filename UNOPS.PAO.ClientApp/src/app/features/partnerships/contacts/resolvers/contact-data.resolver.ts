import { Injectable } from '@angular/core';
import { Resolve, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { ContactService } from '../services/contact.service';
import { Contact } from '../models/contact.model';

@Injectable({
  providedIn: 'root'
})
export class ContactDataResolver implements Resolve<Contact> {
  constructor(private contactService: ContactService) {}

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<Contact> {
    const recordId = route.paramMap.get('recordId') || '';
    return this.contactService.getContactById(recordId);
  }
}
