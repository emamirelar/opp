import { ChangeDetectionStrategy, Component, OnInit, OnDestroy, ChangeDetectorRef, inject, ViewChild, ElementRef, computed, effect } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { LayoutService } from '../../services/layout.service';
import { LanguageSelectorComponent } from './language-selector/language-selector.component';
import { StyleClassModule } from 'primeng/styleclass';
import { PrimeIcons } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { OverlayPanelModule } from 'primeng/overlaypanel';
import { ToastModule } from 'primeng/toast';
import { ProgressBarModule } from 'primeng/progressbar';
import { TooltipModule } from 'primeng/tooltip';
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
import { GlobalSearchBarComponent } from './global-search-bar/global-search-bar.component';
import { RoleService } from '../../../../essentials/services/role.service';
import { RoleDialogComponent } from './role-dialog/role-dialog.component';
import { ProfileDialogComponent } from '../profile-dialog/profile-dialog.component';

import { GlobalFiltersDialogComponent } from './global-filters-dialog/global-filters-dialog.component';
import { TranslateModule } from '@ngx-translate/core';
import { GlobalFilterService } from '../../../../services/global-filter.service';

interface UserInfo {
  userId: number;
  name: string;
  userEmail: string;
  orgUnit: string;
  orgUnitDescription?: string;
  supervisorId: number;
  supervisorName?: string;
  supervisorEmail?: string;
}

@Component({
  selector: 'app-topbar',
  imports: [
    CommonModule,
    HttpClientModule,
    LanguageSelectorComponent,
    StyleClassModule,
    ButtonModule,
    OverlayPanelModule,
    ToastModule,
    ProgressBarModule,
    TooltipModule,
    ConfirmDialogModule,
    MenuModule,
    RippleModule,
    InputTextModule,
    AvatarModule,
    GlobalSearchBarComponent,
    RoleDialogComponent,
    ProfileDialogComponent,
    GlobalFiltersDialogComponent,
    TranslateModule
  ],
  templateUrl: './topbar.component.html',
  styleUrl: './topbar.component.scss',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [MessageService, ConfirmationService]
})
export class TopbarComponent implements OnInit, OnDestroy {
  @ViewChild(RoleDialogComponent) roleDialog!: RoleDialogComponent;
  @ViewChild(ProfileDialogComponent) profileDialog!: ProfileDialogComponent;

  @ViewChild(GlobalFiltersDialogComponent) globalFiltersDialog!: GlobalFiltersDialogComponent;

  items!: MenuItem[];
  notifications: Notification[] = [];
  unreadCount: number = 0;
  isDevelopment: boolean = false;
  private notificationSubscription?: Subscription;
  private userId: string = '';
  private previousNotifications: Notification[] = [];
  private importService = inject(ImportService);
  private importDialogService = inject(ImportDialogService);
  private confirmationService = inject(ConfirmationService);
  private notificationInterval: any;
  private http = inject(HttpClient);
  private roleService = inject(RoleService);
  
  menuActive: boolean = false;
  userInfo: UserInfo | null = null;
  profileMenuItems: MenuItem[] = [];
  userRoles: string[] = [];
  roleMenuItems: MenuItem[] = [];

  // Propriété pour le filtre d'unité organisationnelle
  isOrgUnitFilterActive: boolean = false;
  private globalFilterSubscription?: Subscription;
  
  // Mobile detection
  isMobile: boolean = false;

  constructor(
    public layoutService: LayoutService,
    private notificationService: NotificationService,
    private componentResolverService: ComponentResolverService,
    private authService: AuthService,
    private messageService: MessageService,
    private cdr: ChangeDetectorRef,
    private router: Router,
    private globalFilterService: GlobalFilterService
  ) {
    // Check if we're in development mode
    this.isDevelopment = this.checkIfDevelopment();
  }

  private checkIfDevelopment(): boolean {
    // Method 1: Check for localhost in URL
    const isLocalhost = window.location.hostname === 'localhost' || 
                        window.location.hostname === '127.0.0.1';
                        
    // Method 2: Check for dev cookie
    const hasDevCookie = document.cookie.split(';')
      .some(c => c.trim().startsWith('dev-user-email='));
      
    return isLocalhost || hasDevCookie;
  }

  ngOnInit() {
    // Initialize mobile detection
    this.detectMobile();
    
    this.authService.user().subscribe({
      next: (claims) => {
        const userIdClaim = claims.find(c => c.type === 'userId');
        if (userIdClaim) {
          this.userId = userIdClaim.value;
          this.loadUserRoles();
          this.startNotificationPolling();
        }
        this.loadUserInfo();
      },
      error: (error) => {
        // Error getting user claims
        console.error('Error getting user claims:', error);
      }
    });

    this.profileMenuItems = [];
    this.setupProfileMenu();
    
    // Debug: Vérifier l'état initial du service
    console.log('Initial GlobalFilter state:', {
      filterEnabled: this.globalFilterService.isFilterEnabled(),
      selectedOrgUnitId: this.globalFilterService.getSelectedOrgUnitId(),
      activeOrgUnitId: this.globalFilterService.getActiveOrgUnitId()
    });
    
    // Souscrire aux changements du filtre d'unité organisationnelle
    this.globalFilterSubscription = this.globalFilterService.activeOrgUnitId$.subscribe({
      next: (activeOrgUnitId) => {
        console.log('GlobalFilter - activeOrgUnitId changed:', activeOrgUnitId);
        this.isOrgUnitFilterActive = activeOrgUnitId !== null;
        console.log('GlobalFilter - isOrgUnitFilterActive set to:', this.isOrgUnitFilterActive);
        this.cdr.markForCheck();
      },
      error: (error) => {
        console.error('Error subscribing to global filter changes:', error);
      }
    });
  }

  private setupProfileMenu() {
    this.profileMenuItems = [
      {
        label: 'View Profile',
        icon: 'pi pi-user',
        command: () => this.showProfile()
      },
      {
        separator: true
      },
      {
        label: 'Impersonate Roles',
        icon: 'pi pi-users',
        command: () => this.showRoleDialog()
      }
    ];

    // Add development-only menu items
    if (this.isDevelopment) {
      this.profileMenuItems.push(
        {
          separator: true
        },
        {
          label: 'Dev Login',
          icon: 'pi pi-user-edit',
          command: () => this.navigateToDevLogin()
        },
        {
          label: 'Debug Info',
          icon: 'pi pi-cog',
          command: () => this.navigateToDebug()
        }
      );
    }
  }

  private loadUserInfo() {
    // Get email from claims to pass as parameter
    this.authService.user().subscribe({
      next: (claims) => {
        const emailClaim = claims.find(c => c.type === 'email' || 
                                     c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress');
        
        const email = emailClaim?.value;
        const apiUrl = email ? `/api/user-info/current?email=${encodeURIComponent(email)}` : '/api/user-info/current';
        
        this.http.get<any>(apiUrl).subscribe({
          next: (response) => {
            // Extract user info from the nested response structure
            const userInfoData = response.userInfoWithOrgSettings || response;
            
            if (userInfoData) {
              this.userInfo = {
                userId: userInfoData.userId || 0,
                name: userInfoData.name || email || 'Unknown User',
                userEmail: userInfoData.userEmail || email || '',
                orgUnit: userInfoData.orgUnit || 'N/A',
                orgUnitDescription: userInfoData.orgUnitDescription || '',
                supervisorId: userInfoData.supervisorId || 0,
                supervisorName: userInfoData.supervisorName || '',
                supervisorEmail: userInfoData.supervisorEmail || ''
              };
              this.cdr.markForCheck(); // Trigger change detection
              this.loadNotifications();
              this.loadUserRoles();
            } else {
              console.warn('No user info data received from API');
              // Create a minimal user info from email claim as fallback
              if (email) {
                this.userInfo = {
                  userId: 0,
                  name: email.split('@')[0],
                  userEmail: email,
                  orgUnit: 'N/A',
                  orgUnitDescription: '',
                  supervisorId: 0,
                  supervisorName: '',
                  supervisorEmail: ''
                };
                this.cdr.markForCheck();
              }
            }
          },
          error: (err) => {
            console.error('Error loading user info:', err);
            // As a fallback, try to create basic user info from claims
            const emailClaim = claims.find(c => c.type === 'email' || 
                                         c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress');
            if (emailClaim?.value) {
              this.userInfo = {
                userId: 0,
                name: emailClaim.value.split('@')[0],
                userEmail: emailClaim.value,
                orgUnit: 'N/A',
                orgUnitDescription: '',
                supervisorId: 0,
                supervisorName: '',
                supervisorEmail: ''
              };
              this.cdr.markForCheck();
            }
          }
        });
      },
      error: (error) => {
        console.error('Error getting user claims:', error);
      }
    });
  }

  ngOnDestroy() {
    this.stopNotificationPolling();
    if (this.globalFilterSubscription) {
      this.globalFilterSubscription.unsubscribe();
    }
  }

  private startNotificationPolling() {
   // this.loadNotifications();

    /*this.notificationSubscription = interval(15000)
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
          // Error loading notifications
        }
      });*/
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
        // Error loading notifications
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
          // Error processing notification data
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
        // Error marking notification as read
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
                // Error updating notification
              }
            });
          },
          error: (err: any) => {
            // Error cancelling file analysis
          }
        });
      }
    });
  }

  navigateToDevLogin() {
    window.open('https://localhost:7123/dev-login', '_blank');
  }
  
  navigateToDebug() {
    window.open('https://localhost:7123/api/dev/debug', '_blank');
  }

  private loadUserRoles() {
    this.roleService.getUserRoles().subscribe({
      next: (userRoles) => {
        this.userRoles = userRoles.roles;
        this.cdr.markForCheck();
      },
      error: (error) => {
        console.error('Error loading user roles:', error);
      }
    });
  }

  showRoleDialog() {
    this.roleDialog.show();
  }

  showProfile() {
    if (this.userInfo) {
      this.profileDialog.show(this.userInfo);
    } else {
      // Temporary fallback for debugging
      console.warn('UserInfo is null, creating temporary profile data');
      const tempUserInfo = {
        userId: 0,
        name: 'Debug User',
        userEmail: 'debug@example.com',
        orgUnit: 'N/A',
        orgUnitDescription: '',
        supervisorId: 0,
        supervisorName: '',
        supervisorEmail: ''
      };
      this.profileDialog.show(tempUserInfo);
    }
  }

  openGlobalFilters() {
    this.globalFiltersDialog.show();
  }



  onAIAssistantToggle() {
    this.layoutService.onAIAssistantToggle();
  }

  onMenuButtonClick() {
    // Check if we're on the AI route
    if (this.router.url.startsWith('/ai')) {
      // On AI route, toggle the AI sidebar collapse
      this.layoutService.onAiSidebarToggle();
    } else {
      // On regular routes, toggle the main sidebar
      this.layoutService.onMenuToggle();
    }
  }

  private detectMobile() {
    this.isMobile = window.innerWidth <= 768;
  }

  isOnAiPage(): boolean {
    return this.isMobile && this.router.url.includes('/ai');
  }
}
