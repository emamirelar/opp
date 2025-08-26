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
import { TabViewModule } from 'primeng/tabview';
import { DialogModule } from 'primeng/dialog';
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
import { Router, RouterModule } from '@angular/router';
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
import { TourControlComponent } from '../../../components/tour-control/tour-control.component';
import { ConfigurationService } from '../../../../essentials/services/configuration.service';
import { AiAssistantData } from '../../../reusables/widgets/ai-assistant/ai-assistant.data';
import { FormsModule } from '@angular/forms';

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
    RouterModule,
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
    TabViewModule,
    DialogModule,
    GlobalSearchBarComponent,
    RoleDialogComponent,
    ProfileDialogComponent,
    GlobalFiltersDialogComponent,
    TranslateModule,
    TourControlComponent,
    FormsModule
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
  allNotifications: Notification[] = [];
  unreadCount: number = 0;
  activeNotificationTab: 'unread' | 'all' = 'unread';
  showNotificationDialog: boolean = false;
  isDevelopment: boolean = false;
  isSearchExpanded: boolean = false;
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

  // Chat history properties
  chatSessions: any[] = [];
  isLoadingChatSessions: boolean = false;
  chatSearchQuery: string = '';
  filteredChatSessions: any[] = [];
  isNewChatDisabled: boolean = false;

  constructor(
    public layoutService: LayoutService,
    private notificationService: NotificationService,
    private componentResolverService: ComponentResolverService,
    private authService: AuthService,
    private messageService: MessageService,
    private cdr: ChangeDetectorRef,
    private router: Router,
    private globalFilterService: GlobalFilterService,
    private configurationService: ConfigurationService,
    private aiAssistantData: AiAssistantData
  ) {
    // Check if we're in development mode
    this.isDevelopment = this.checkIfDevelopment();
  }

  private checkIfDevelopment(): boolean {
    // Get environment from configuration service
    const config = this.configurationService.getConfig();
    
    if (config && config.environment) {
      // Show impersonate roles button for any environment that is NOT 'Production'
      return config.environment.toLowerCase() !== 'production';
    }
    
    // Fallback to previous logic if config is not available
    const isLocalhost = window.location.hostname === 'localhost' || 
                        window.location.hostname === '127.0.0.1';
                        
    const hasDevCookie = document.cookie.split(';')
      .some(c => c.trim().startsWith('dev-user-email='));
      
    return isLocalhost || hasDevCookie;
  }

  ngOnInit() {
    // Initialize mobile detection
    this.detectMobile();
    
    // Re-check development mode now that component is initialized
    // This ensures we have the latest configuration data
    this.isDevelopment = this.checkIfDevelopment();
    
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

    // Load chat sessions if on AI page
    if (this.isOnAiPage()) {
      this.loadChatSessions();
    }

    // Listen for route changes to load chat sessions when navigating to AI page
    this.router.events.subscribe(() => {
      if (this.isOnAiPage()) {
        this.loadChatSessions();
      }
    });
  }

  private setupProfileMenu() {
    // Re-check development mode to ensure current status
    this.isDevelopment = this.checkIfDevelopment();
    
    this.profileMenuItems = [
      {
        label: 'View Profile',
        icon: 'pi pi-user',
        command: () => this.showProfile()
      },
      {
        separator: true
      }
    ];

    // Only show Impersonate Roles button when NOT in Production environment
    if (this.isDevelopment) {
      this.profileMenuItems.push({
        label: 'Impersonate Roles',
        icon: 'pi pi-users',
        command: () => this.showRoleDialog()
      });
    }

    var isLocalDevelopment = window.location.hostname === 'localhost' ||  window.location.hostname === '127.0.0.1';
    
    const hasDevCookie = document.cookie.split(';')
    .some(c => c.trim().startsWith('dev-user-email='));
    // Add development-only menu items
    if (isLocalDevelopment || hasDevCookie) {
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
    // Load notifications initially
    this.loadNotifications();

    // Set up polling every 15 seconds for unread notifications
    this.notificationSubscription = interval(15000)
      .pipe(
        switchMap(() => this.notificationService.getNotifications(this.userId, true)) // Get unread notifications
      )
      .subscribe({
        next: (notifications: Notification[]) => {
          this.handleNewNotifications(notifications);
          this.notifications = notifications;
          this.unreadCount = notifications.length;
          this.previousNotifications = [...notifications];
          this.cdr.markForCheck();
        },
        error: (error: any) => {
          console.error('Error polling notifications:', error);
        }
      });

    // Also refresh all notifications periodically (every 60 seconds to be less aggressive)
    this.notificationInterval = setInterval(() => {
      this.notificationService.getNotifications(this.userId, false).subscribe({
        next: (allNotifications: Notification[]) => {
          this.allNotifications = allNotifications;
          this.cdr.markForCheck();
        },
        error: (error: any) => {
          console.error('Error refreshing all notifications:', error);
        }
      });
    }, 60000); // Every minute for all notifications
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
    if (this.notificationInterval) {
      clearInterval(this.notificationInterval);
    }
  }

  loadNotifications() {
    // Load unread notifications
    this.notificationService.getNotifications(this.userId, true).subscribe({
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

    // Load all notifications
    this.notificationService.getNotifications(this.userId, false).subscribe({
      next: (allNotifications: Notification[]) => {
        this.allNotifications = allNotifications;
        this.cdr.markForCheck();
      },
      error: (error: any) => {
        // Error loading all notifications
      }
    });
  }

  getDisplayCount(): string {
    if (this.unreadCount > 99) {
      return '99+';
    }
    return this.unreadCount.toString();
  }

  getCurrentNotifications(): Notification[] {
    return this.activeNotificationTab === 'unread' ? this.notifications : this.allNotifications;
  }

  getLimitedNotifications(): Notification[] {
    const current = this.getCurrentNotifications();
    return current.slice(0, 5); // Show only top 5 notifications
  }

  hasMoreNotifications(): boolean {
    return this.getCurrentNotifications().length > 5;
  }

  openNotificationDialog(): void {
    this.showNotificationDialog = true;
  }

  closeNotificationDialog(): void {
    this.showNotificationDialog = false;
  }

  switchNotificationTab(tab: 'unread' | 'all') {
    this.activeNotificationTab = tab;
    this.cdr.markForCheck();
  }

  isNotificationRead(notification: Notification): boolean {
    return notification.isRead === true;
  }

  formatTimestamp(timestamp: string): string {
    const date = new Date(timestamp);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    if (diffHours < 24) return `${diffHours}h ago`;
    if (diffDays < 7) return `${diffDays}d ago`;
    
    return date.toLocaleDateString();
  }

  getCategoryIcon(notification: Notification): string {
    const category = notification.category?.toLowerCase() || '';
    
    // Check for specific entity types
    if (category.includes('contact') || category.includes('_contact_')) {
      return 'pi pi-user';
    } else if (category.includes('partner') || category.includes('_partner_')) {
      return 'pi pi-building';
    } else if (category.includes('interaction') || category.includes('_interaction_')) {
      return 'pi pi-comments';
    } else if (category.includes('file') || category.includes('import') || category.includes('export')) {
      return 'pi pi-file';
    } else if (category.includes('analysis') || category.includes('ai')) {
      return 'pi pi-chart-line';
    } else if (category.includes('bulk') || category.includes('batch')) {
      return 'pi pi-clone';
    } else if (notification.status === 'Progress') {
      return 'pi pi-spin pi-spinner';
    } else {
      return 'pi pi-bell';
    }
  }

  getCategoryIconColor(notification: Notification): string {
    const category = notification.category?.toLowerCase() || '';
    
    if (category.includes('contact')) {
      return '#10b981'; // Green for contacts
    } else if (category.includes('partner')) {
      return '#3b82f6'; // Blue for partners
    } else if (category.includes('interaction')) {
      return '#f59e0b'; // Orange for interactions
    } else if (category.includes('file') || category.includes('import') || category.includes('export')) {
      return '#8b5cf6'; // Purple for files
    } else if (category.includes('analysis') || category.includes('ai')) {
      return '#ef4444'; // Red for AI/analysis
    } else if (notification.status === 'Progress') {
      return '#3b82f6'; // Blue for progress
    } else {
      return '#6b7280'; // Gray for default
    }
  }

  handleNotificationClick(notification: Notification) {
    // Handle AI data modification notifications (category format: ENTITYTYPE_ID)
    if (notification.responseType.startsWith("data_")) {
      this.handleDataModificationNotification(notification);
      return;
    }

    // Handle other notification types that require records
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
    } else {
      // For notifications without records, just mark as read
      this.markNotificationAsRead(notification.id);
    }
  }

  handleDataModificationNotification(notification: Notification): void {
    // Parse category in format "ENTITYTYPE_ID"
    if (!notification.category || !notification.category.includes('_')) {
      console.warn('Invalid category format. Expected "ENTITYTYPE_ID", got:', notification.category);
      return;
    }

    const categoryParts = notification.category.split('_');
    if (categoryParts.length < 2) {
      console.warn('Invalid category format. Expected "ENTITYTYPE_ID", got:', notification.category);
      return;
    }

    const entityType = categoryParts[0].toLowerCase();
    const entityId = categoryParts[1];

    if (!entityType || !entityId || entityId === '0') {
      console.warn('Missing or invalid entity type or ID in category:', notification.category);
      return;
    }

    // Route to the appropriate entity page based on entity type
    let route: string;
    switch (entityType) {
      case 'partner':
        route = `/partnerships/partner/${entityId}`;
        break;
      case 'contact':
        route = `/partnerships/contacts/${entityId}`;
        break;
      case 'interaction':
        route = `/partnerships/interactions/${entityId}`;
        break;
      case 'project':
        route = `/project/view/${entityId}`;
        break;
      case 'opportunity':
        route = `/opportunity/view/${entityId}`;
        break;
      case 'user_preference':
        route = `/profile`;
        break;
      default:
        // Try to use the entity type as a direct route
        route = `/${entityType}/view/${entityId}`;
        break;
    }

    console.log(`Navigating to entity: ${entityType} with ID: ${entityId} -> ${route}`);
    this.router.navigate([route]);
    this.markNotificationAsRead(notification.id);
  }
  
  markNotificationAsRead(notificationId: number): void {
    this.notificationService.markAsRead(notificationId, this.userId).subscribe({
      next: () => {
        // Remove from unread notifications
        this.notifications = this.notifications.filter(n => n.id !== notificationId);
        this.unreadCount = this.notifications.length;
        
        // Update read status in all notifications
        const notificationIndex = this.allNotifications.findIndex(n => n.id === notificationId);
        if (notificationIndex >= 0) {
          this.allNotifications[notificationIndex] = {
            ...this.allNotifications[notificationIndex],
            isRead: true,
            readAt: new Date().toISOString()
          };
        }
        
        // Update previous notifications to avoid duplicate toast notifications
        this.previousNotifications = this.previousNotifications.filter(n => n.id !== notificationId);
        
        this.cdr.markForCheck();
      },
      error: (error) => {
        console.error('Error marking notification as read:', error);
      }
    });
  }

  handleNotificationClickFromDialog(notification: Notification): void {
    // Handle the notification click
    this.handleNotificationClick(notification);
    
    // Close the dialog after handling the click
    this.closeNotificationDialog();
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
    // Check if user is on mobile
    if (this.isMobile) {
      // On mobile, navigate to /ai instead of opening overlay
      this.router.navigate(['/ai']);
    } else {
      // On desktop, use the current overlay behavior
      this.layoutService.onAIAssistantToggle();
    }
  }

  onMenuButtonClick() {
    // Always toggle the main sidebar (removed AI-specific logic)
    this.layoutService.onMenuToggle();
  }

  private detectMobile() {
    this.isMobile = window.innerWidth <= 768;
  }

  isOnAiPage(): boolean {
    return this.router.url.includes('/ai');
  }

  /**
   * Navigate to the home page when logo is clicked
   */
  navigateToHome(): void {
    console.log('Logo clicked - navigating to home');
    this.router.navigate(['/']);
  }

  /**
   * Handle search expansion state
   */
  onSearchExpanded(isExpanded: boolean): void {
    this.isSearchExpanded = isExpanded;
  }

  // Chat history methods
  async loadChatSessions(): Promise<void> {
    if (!this.isOnAiPage()) return;
    
    this.isLoadingChatSessions = true;
    this.cdr.markForCheck();
    
    try {
      const response = await this.http.post<any[]>('/api/ai-assistant/get-user-sessions', {}).toPromise();
      if (response) {
        const sortedSessions = response.sort((a, b) => 
          new Date(b.lastUpdated).getTime() - new Date(a.lastUpdated).getTime()
        );
        this.chatSessions = sortedSessions;
        this.filteredChatSessions = [...sortedSessions];
      }
    } catch (error) {
      console.error('Error loading chat sessions:', error);
      this.chatSessions = [];
      this.filteredChatSessions = [];
    } finally {
      this.isLoadingChatSessions = false;
      this.cdr.markForCheck();
    }
  }

  onChatSearchInput(event: any): void {
    const query = event.target.value.toLowerCase().trim();
    this.chatSearchQuery = query;
    
    if (!query) {
      this.filteredChatSessions = [...this.chatSessions];
    } else {
      this.filteredChatSessions = this.chatSessions.filter(session =>
        (session.title || '').toLowerCase().includes(query)
      );
    }
    this.cdr.markForCheck();
  }

  clearChatSearch(): void {
    this.chatSearchQuery = '';
    this.filteredChatSessions = [...this.chatSessions];
    this.cdr.markForCheck();
  }

  startNewChat(): void {
    this.aiAssistantData.clearConversation();
    this.router.navigate(['/ai'], { replaceUrl: true });
    this.isNewChatDisabled = true;
    setTimeout(() => this.isNewChatDisabled = false, 1000);
  }

  openChatSession(session: any): void {
    this.router.navigate(['/ai', session.id], { replaceUrl: true });
    this.aiAssistantData.switchToSession(session.id).subscribe({
      error: (error) => console.error('Failed to switch session:', error)
    });
  }

  isSelectedChatSession(session: any): boolean {
    const currentSessionId = this.aiAssistantData.currentSessionId();
    return currentSessionId === session.id;
  }

  async toggleChatStar(session: any, event: Event): Promise<void> {
    event.stopPropagation();
    
    try {
      const newStarredState = !session.starred;
      
      // Optimistically update UI
      session.starred = newStarredState;
      this.cdr.markForCheck();
      
      // Call backend API
      await this.http.post('/api/ai-assistant/update-star', {
        sessionId: session.id,
        starred: newStarredState
      }).toPromise();
      
      // Reload sessions to ensure consistency
      await this.loadChatSessions();
      
    } catch (error) {
      console.error('Error updating star status:', error);
      // Revert optimistic update on error
      session.starred = !session.starred;
      this.cdr.markForCheck();
    }
  }

  formatChatDate(dateString: string): string {
    if (!dateString) return '';
    
    const date = new Date(dateString);
    const now = new Date();
    const diffTime = Math.abs(now.getTime() - date.getTime());
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    
    if (diffDays === 1) {
      return 'Today';
    } else if (diffDays === 2) {
      return 'Yesterday';
    } else if (diffDays <= 7) {
      return `${diffDays - 1} days ago`;
    } else {
      return date.toLocaleDateString();
    }
  }

  trackByChatId(index: number, session: any): string {
    return session.id || index;
  }
}
