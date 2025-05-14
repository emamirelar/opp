import { ChangeDetectionStrategy, Component, OnInit, OnDestroy, ChangeDetectorRef, ViewChild, inject } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { LayoutService } from '../../services/layout.service';
import { LanguageSelectorComponent } from './language-selector/language-selector.component';
import { ProfileMenubarComponent } from './profile-menubar/profile-menubar.component';
import { StyleClassModule } from 'primeng/styleclass';
import { PrimeIcons } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { OverlayPanelModule } from 'primeng/overlaypanel';
import { ToastModule } from 'primeng/toast';
import { ProgressBarModule } from 'primeng/progressbar';
import { NotificationService, Notification } from '../../../services/notification.service';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { ComponentResolverService } from '../../../../features/internal/services/component-resolver.service';
import { interval, Subscription } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { AuthService } from '../../../../essentials/services/auth.service';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ImportDialogService } from '../../../reusables/components/import/dialog/import-dialog.service';
import { ImportService } from '../../../reusables/components/import/import.service';
import { Router } from '@angular/router';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import {GlobalSearchBarComponent} from './global-search-bar/global-search-bar.component';
import { TooltipModule } from 'primeng/tooltip';

@Component({
  selector: 'app-topbar',
  imports: [
    CommonModule,
    HttpClientModule,
    LanguageSelectorComponent,
    ProfileMenubarComponent,
    StyleClassModule,
    ButtonModule,
    OverlayPanelModule,
    ToastModule,
    ProgressBarModule,
    TooltipModule,
    ConfirmDialogModule,
    GlobalSearchBarComponent
  ],
  templateUrl: './topbar.component.html',
  styleUrl: './topbar.component.scss',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [MessageService, ConfirmationService]
})
export class TopbarComponent implements OnInit, OnDestroy {
  items!: MenuItem[];
  notifications: Notification[] = [];
  unreadCount: number = 0;
  private notificationSubscription?: Subscription;
  private userId: string = '';
  private previousNotifications: Notification[] = [];
  private importService = inject(ImportService);
  private importDialogService = inject(ImportDialogService);
  private confirmationService = inject(ConfirmationService);

  constructor(
    public layoutService: LayoutService,
    private notificationService: NotificationService,
    private componentResolverService: ComponentResolverService,
    private authService: AuthService,
    private messageService: MessageService,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) { }

  ngOnInit() {
    this.authService.user().subscribe({
      next: (claims) => {
        const userIdClaim = claims.find(c => c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier');
        if (userIdClaim) {
          this.userId = userIdClaim.value;
          this.startNotificationPolling();
        }
      },
      error: (error) => {
        console.error('Error getting user claims:', error);
      }
    });
  }

  ngOnDestroy() {
    this.stopNotificationPolling();
  }

  private startNotificationPolling() {
    // Initial load
    this.loadNotifications();

    // Start polling every 15 seconds
    this.notificationSubscription = interval(15000)
      .pipe(
        switchMap(() => this.notificationService.getNotifications(this.userId))
      )
      .subscribe({
        next: (notifications: Notification[]) => {
          this.handleNewNotifications(notifications);
          this.notifications = notifications;
          this.unreadCount = notifications.length;
          this.cdr.markForCheck();
        },
        error: (error: any) => {
          console.error('Error loading notifications:', error);
        }
      });
  }

  private handleNewNotifications(newNotifications: Notification[]) {
    // Find new notifications that weren't in the previous list
    const newItems = newNotifications.filter(newNotif =>
      !this.previousNotifications.some(prevNotif => prevNotif.id === newNotif.id)
    );

    if (newItems.length > 0) {
      // Prepare messages for different notification types
      let message = '';

      if (newItems.length === 1) {
        const notification = newItems[0];

        // Enhanced message for bulk import notifications
        if (notification.category === 'bulk_contact_action' || notification.category.startsWith('bulk_')) {
          // Check if we have records to count
          if (notification.records && notification.records.length > 0) {
            message = `You have a new notification. Click on the bell icon to read it.`;
          } else {
            message = 'Data imported and ready for review. Click to open.';
          }
        } else {
          // Default message for other notifications
          message = notification.message;
        }
      } else {
        message = `You have ${newItems.length} new notifications`;
      }

      // Show toast for new notifications
      this.messageService.add({
        severity: 'info',
        summary: 'New Notification',
        detail: message,
        life: 5000,
        sticky: false
      });

      // Only update previousNotifications when we find new ones
      this.previousNotifications = [...newNotifications];
    }
  }

  // Helper method to count records in a notification
  private getRecordCount(notification: Notification): number {
    if (!notification.records || !notification.records.length) {
      return 0;
    }

    // If records is an array with actual data
    if (notification.records.length > 1) {
      return notification.records.length;
    }

    // If records contains a single item that might be a JSON string
    if (notification.records.length === 1) {
      const firstItem = notification.records[0];

      // If it's a string that might be JSON
      if (typeof firstItem === 'string') {
        try {
          const parsed = JSON.parse(firstItem);
          if (Array.isArray(parsed)) {
            return parsed.length;
          }
        } catch (e) {
          // Not a valid JSON string
        }
      }

      // If it has a 'records' property that might contain the actual records
      if (typeof firstItem === 'object' && firstItem !== null && 'records' in firstItem) {
        const nestedRecords = firstItem.records;
        if (Array.isArray(nestedRecords)) {
          return nestedRecords.length;
        } else if (typeof nestedRecords === 'string') {
          try {
            const parsed = JSON.parse(nestedRecords);
            if (Array.isArray(parsed)) {
              return parsed.length;
            }
          } catch (e) {
            // Not a valid JSON string
          }
        }
      }
    }

    return 1; // Default to 1 if we can't determine the count
  }

  private stopNotificationPolling() {
    if (this.notificationSubscription) {
      this.notificationSubscription.unsubscribe();
    }
  }

  loadNotifications() {
    this.notificationService.getNotifications(this.userId).subscribe({
      next: (notifications: Notification[]) => {
        this.notifications = notifications;
        this.unreadCount = notifications.length;
        // Initialize previousNotifications with the initial set
        this.previousNotifications = [...notifications];
        this.cdr.markForCheck();
      },
      error: (error: any) => {
        console.error('Error loading notifications:', error);
      }
    });
  }

  handleNotificationClick(notification: Notification) {
    if (notification.category && notification.records && notification.records.length > 0) {
      // Check if this is a bulk import notification
      if (notification.category.startsWith('bulk_') && notification.responseType !== 'Error') {
        try {
          // Clear previous data
          this.importDialogService.data.set([]);

          // Set new data - we capture the result to check if it worked
          this.importDialogService.setData(notification.records);

          // Verify data was set properly
          const currentData = this.importDialogService.data();

          if (currentData.length === 0) {
            // Show error message if no data was processed
            this.messageService.add({
              severity: 'error',
              summary: 'Data Error',
              detail: 'Could not process notification data. Please check the console for details.',
              life: 5000
            });
            return;
          }

          // Store notification ID in the service for later use when import is completed or canceled
          this.importDialogService.setNotificationInfo(notification.id, this.userId, notification.message);

          // Then open the dialog with the correct header
          this.importDialogService.openImportDialog(
            notification.category === 'bulk_contact_action' ? 'Import Contact' : 'Import'
          );
        } catch (error) {
          console.error('Error processing notification data:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Processing Error',
            detail: 'An error occurred while processing notification data',
            life: 5000
          });
        }
      } else {
        // Use component resolver for other types of notifications
        this.componentResolverService.loadComponent(notification.category, null, notification.records);

        // Mark non-import notification as read
        this.markNotificationAsRead(notification.id);
      }
    }
  }

  // Separate method to mark notification as read
  markNotificationAsRead(notificationId: number): void {
    this.notificationService.markAsRead(notificationId, this.userId).subscribe({
      next: () => {
        // Update local state
        this.notifications = this.notifications.filter(n => n.id !== notificationId);
        this.unreadCount = this.notifications.length;
        this.cdr.markForCheck();
      },
      error: (error) => {
        console.error('Error marking notification as read:', error);
      }
    });
  }

  formatProgressMessage(message: string): string {
    if (!message) return '';

    // Find progress bar pattern like [■■■■□□□□□□□□□□□□□□□□]
    const progressBarRegex = /\[(■+□*)\]/g;

    // Replace the progress bar with HTML span with special styling
    return message.replace(progressBarRegex, (match) => {
      return `<span class="progress-bar">${match}</span>`;
    });
  }

  /**
   * Cancel a file analysis operation in progress
   * @param notification The notification for the file analysis operation
   * @param event The click event to stop propagation
   */
  cancelFileAnalysis(notification: Notification, event: Event): void {
    // Stop event propagation to prevent opening the notification
    event.stopPropagation();

    // Extract jobId from the notification message if available
    let jobId = null;
    if (notification.message) {
      const match = notification.message.match(/Job ID: ([a-zA-Z0-9-]+)/);
      if (match && match[1]) {
        jobId = match[1];
      }
    }

    // Show confirmation dialog
    this.confirmationService.confirm({
      message: 'Are you sure you want to cancel this file analysis operation?',
      header: 'Cancel File Analysis',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        // Call the cancel API
        this.importService.cancelAnalysis().subscribe({
          next: () => {
            // Update notification status
            this.notificationService.updateNotification(
              notification.id,
              'File analysis was cancelled by user',
              'Done'
            ).subscribe({
              next: () => {
                // Mark as read after updating
                this.markNotificationAsRead(notification.id);

                // Show success message
                this.messageService.add({
                  severity: 'success',
                  summary: 'Cancelled',
                  detail: 'File analysis operation has been cancelled',
                  life: 3000
                });
              },
              error: (err: any) => {
                console.error('Error updating notification:', err);
              }
            });
          },
          error: (err: any) => {
            console.error('Error cancelling file analysis:', err);
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Failed to cancel file analysis: ' + (err.message || 'Unknown error'),
              life: 5000
            });
          }
        });
      }
    });
  }
}
