import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

export interface Notification {
  id: number;
  message: string;
  records?: any[];
  category: string;
  responseType: string;
  entity?: string; // Entity type for navigation (e.g., "Opportunity", "Partner")
  entityId?: number; // Entity ID for navigation
  status?: 'Pending' | 'Progress' | 'Done';
  isRead?: boolean;
  createdAt?: string;
  readAt?: string;
}

export interface UpdateNotificationRequest {
  message: string;
  status: 'Pending' | 'Progress' | 'Done';
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private apiUrl = '/api/notifications';

  constructor(private http: HttpClient) { }

  getNotifications(userId: string, unreadOnly?: boolean): Observable<Notification[]> {
    const params = unreadOnly !== undefined ? `?userId=${userId}&unreadOnly=${unreadOnly}` : `?userId=${userId}`;
    return this.http.get<Notification[]>(`${this.apiUrl}${params}`).pipe(
      tap(notifications => {
      })
    );
  }

  markAsRead(notificationId: number, userId: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${notificationId}/read?userId=${userId}`, {});
  }

  /**
   * Update an existing notification message and status
   * @param notificationId ID of the notification to update
   * @param message New message for the notification
   * @param status New status for the notification
   * @returns Observable of the API response
   */
  updateNotification(
    notificationId: number, 
    message: string, 
    status: 'Pending' | 'Progress' | 'Done'
  ): Observable<any> {
    const payload: UpdateNotificationRequest = {
      message,
      status
    };
    return this.http.put(`${this.apiUrl}/${notificationId}/update`, payload);
  }
} 
