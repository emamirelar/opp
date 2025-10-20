import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface EntityPanelState {
  isOpen: boolean;
  entityType?: string;
  entityId?: string;
  entityData?: any;
}

@Injectable({
  providedIn: 'root'
})
export class EntityPanelService {
  private panelState = new BehaviorSubject<EntityPanelState>({
    isOpen: false
  });

  panelState$ = this.panelState.asObservable();

  openPanel(entityType: string, entityId: string, entityData?: any) {
    this.panelState.next({
      isOpen: true,
      entityType,
      entityId,
      entityData
    });
  }

  closePanel() {
    this.panelState.next({
      isOpen: false
    });
  }

  getCurrentState(): EntityPanelState {
    return this.panelState.value;
  }
} 
