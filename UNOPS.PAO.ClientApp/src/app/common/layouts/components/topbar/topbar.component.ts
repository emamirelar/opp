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
import { HttpClientModule, HttpClient } from '@angular/common/http';
import { ComponentResolverService } from '../../../../features/internal/services/component-resolver.service';
import { interval, Subscription } from 'rxjs';
import { switchMap, tap } from 'rxjs/operators';
import { AuthService } from '../../../../essentials/services/auth.service';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ImportDialogService } from '../../../reusables/components/import/dialog/import-dialog.service';
import { ImportService } from '../../../reusables/components/import/import.service';
import { Router } from '@angular/router';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MenuModule } from 'primeng/menu';
import { RippleModule } from 'primeng/ripple';
import { InputTextModule } from 'primeng/inputtext';
import { AvatarModule } from 'primeng/avatar';
import { ProfileDialogComponent } from '../profile-dialog/profile-dialog.component';

interface UserInfo {
  userId: number;
  name: string;
  userEmail: string;
  orgUnit: string;
  supervisorId: number;
}

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
    ConfirmDialogModule,
    MenuModule,
    RippleModule,
    InputTextModule,
    AvatarModule,
    ProfileDialogComponent
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
  private notificationInterval: any;
  private http = inject(HttpClient);
  @ViewChild('profileDialog') profileDialog!: ProfileDialogComponent;
  
  menuActive: boolean = false;
  userInfo: UserInfo | null = null;
  profileMenuItems: MenuItem[] = [];

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
    this.authService.isLogedIn().pipe(
      tap(isLoggedIn => {
        if (isLoggedIn) {
          this.authService.user().subscribe({
            next: (claims) => {
              const userIdClaim = claims.find(c => c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier');
              if (userIdClaim) {
                this.userId = userIdClaim.value;
                this.loadUserInfo();
                this.startNotificationPolling();
              }
            },
            error: (error) => {
              console.error('Error getting user claims:', error);
            }
          });
        }
      })
    ).subscribe();

    this.profileMenuItems = [
      {
        label: 'Profile',
        icon: 'pi pi-user',
        command: () => this.showProfile()
      },
      {
        label: 'Logout',
        icon: 'pi pi-sign-out',
        command: () => this.logout()
      }
    ];
  }

  private loadUserInfo() {
    this.http.get<UserInfo>(`/api/user-info`).subscribe({
      next: (data) => {
        this.userInfo = data;
        this.loadNotifications();
      },
      error: (err) => {
        console.error('Error loading user info:', err);
        if (err.status === 401) {
          setTimeout(() => this.loadUserInfo(), 1000);
        }
      }
    });
  }

  ngOnDestroy() {
    this.stopNotificationPolling();
  }

  private startNotificationPolling() {
    this.loadNotifications();

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
    const newItems = newNotifications.filter(newNotif => 
      !this.previousNotifications.some(prevNotif => prevNotif.id === newNotif.id)
    );

    if (newItems.length > 0) {
      let message = '';
      
      if (newItems.length === 1) {
        const notification = newItems[0];
        
        if (notification.category === 'bulk_contact_action' || notification.category.startsWith('bulk_')) {
          if (notification.records && notification.records.length > 0) {
            message = `You have a new notification. Click on the bell icon to read it.`;
          } else {
            message = 'Data imported and ready for review. Click to open.';
          }
        } else {
          message = notification.message;
        }
      } else {
        message = `You have ${newItems.length} new notifications`;
      }
      
      this.messageService.add({
        severity: 'info',
        summary: 'New Notification',
        detail: message,
        life: 5000,
        sticky: false
      });
      
      this.previousNotifications = [...newNotifications];
    }
  }
  
  private getRecordCount(notification: Notification): number {
    if (!notification.records || !notification.records.length) {
      return 0;
    }
    
    if (notification.records.length > 1) {
      return notification.records.length;
    }
    
    if (notification.records.length === 1) {
      const firstItem = notification.records[0];
      
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
    
    return 1;
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
      if (notification.category.startsWith('bulk_') && notification.responseType !== 'Error') {
        try {
          this.importDialogService.data.set([]);
          
          this.importDialogService.setData(notification.records);
          
          const currentData = this.importDialogService.data();
          
          if (currentData.length === 0) {
            this.messageService.add({
              severity: 'error',
              summary: 'Data Error',
              detail: 'Could not process notification data. Please check the console for details.',
              life: 5000
            });
            return;
          }
          
          this.importDialogService.setNotificationInfo(notification.id, this.userId, notification.message);
          
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
        this.componentResolverService.loadComponent(notification.category, null, notification.records);
        
        this.markNotificationAsRead(notification.id);
      }
    }
  }
  
  markNotificationAsRead(notificationId: number): void {
    this.notificationService.markAsRead(notificationId, this.userId).subscribe({
      next: () => {
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
    
    const progressBarRegex = /\[(■+□*)\]/g;
    
    return message.replace(progressBarRegex, (match) => {
      return `<span class="progress-bar">${match}</span>`;
    });
  }

  cancelFileAnalysis(notification: Notification, event: Event): void {
    event.stopPropagation();
    
    let jobId = null;
    if (notification.message) {
      const match = notification.message.match(/Job ID: ([a-zA-Z0-9-]+)/);
      if (match && match[1]) {
        jobId = match[1];
      }
    }
    
    this.confirmationService.confirm({
      message: 'Are you sure you want to cancel this file analysis operation?',
      header: 'Cancel File Analysis',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.importService.cancelAnalysis().subscribe({
          next: () => {
            this.notificationService.updateNotification(
              notification.id,
              'File analysis was cancelled by user',
              'Done'
            ).subscribe({
              next: () => {
                this.markNotificationAsRead(notification.id);
                
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

  getInitials(): string {
    if (!this.userInfo?.name) return '?';
    return this.userInfo.name
      .split(' ')
      .map(n => n[0])
      .join('')
      .toUpperCase();
  }

  showProfile() {
    if (this.userInfo) {
      this.profileDialog.show(this.userInfo);
    }
  }

  logout() {
    this.router.navigate(['/login']);
  }
}
