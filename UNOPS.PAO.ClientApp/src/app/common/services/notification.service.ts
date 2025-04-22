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
  status?: 'Pending' | 'Progress' | 'Done';
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

  getNotifications(userId: string): Observable<Notification[]> {
    return this.http.get<Notification[]>(`${this.apiUrl}?userId=${userId}`).pipe(
      tap(notifications => {
        console.log('Raw notifications from API:', notifications);
        if (notifications.length > 0) {
          console.log('Sample notification record structure:', 
            notifications[0].records ? notifications[0].records : 'No records');
        }
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