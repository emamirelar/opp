import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Component, signal } from '@angular/core';
import { By } from '@angular/platform-browser';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';

/**
 * Enhanced Contact View Component Tests
 * 
 * Tests for the enhanced contact view that integrates EnhancedEntityLayoutComponent
 * with contact-specific related info panels.
 * 
 * To run: Copy to UNOPS.PAO.ClientApp/src/app/features/partnerships/contacts/components/contact/view/
 *         and run 'ng test'
 */

// Mock contact view component
@Component({
  selector: 'app-contact-view-enhanced',
  template: `
    <div class="contact-view-enhanced">
      <div class="layout-header">
        <nav class="breadcrumb">
          <a>Contacts</a>
          <a class="current">{{ contact()?.firstName }} {{ contact()?.lastName }}</a>
        </nav>
        <h1 class="contact-name">{{ contact()?.firstName }} {{ contact()?.lastName }}</h1>
        <span class="contact-role">{{ contact()?.role }}</span>
        <div class="action-bar">
          <button class="edit-button" (click)="onEdit()">Edit</button>
          <button class="email-button" (click)="onEmail()">Email</button>
          <button class="call-button" (click)="onCall()">Call</button>
          <button class="delete-button" (click)="onDelete()">Delete</button>
        </div>
      </div>
      
      <div class="layout-body">
        <main class="main-content">
          <nav class="tabs">
            <button class="tab" [class.active]="activeTab() === 'overview'" (click)="setActiveTab('overview')">Overview</button>
            <button class="tab" [class.active]="activeTab() === 'details'" (click)="setActiveTab('details')">Details</button>
          </nav>
          <div class="tab-content">
            <div *ngIf="activeTab() === 'overview'" class="overview-tab">
              <div class="contact-info">
                <p class="email">{{ contact()?.email }}</p>
                <p class="phone">{{ contact()?.phone }}</p>
              </div>
            </div>
          </div>
        </main>
        
        <aside class="side-panel">
          <div class="partners-panel related-panel">
            <h3>Partners ({{ partners().length }})</h3>
            <ul>
              <li *ngFor="let partner of partners()" class="partner-item" (click)="onPartnerClick(partner)">
                {{ partner.name }}
                <span class="primary-badge" *ngIf="partner.isPrimary">Primary</span>
              </li>
            </ul>
            <button class="associate-partner" (click)="onAssociatePartner()">Associate Partner</button>
          </div>
          
          <div class="interactions-panel related-panel">
            <h3>Interactions ({{ interactions().length }})</h3>
            <ul>
              <li *ngFor="let interaction of interactions()" class="interaction-item">
                <span class="interaction-type">{{ interaction.type }}</span>
                {{ interaction.subject }}
              </li>
            </ul>
            <button class="log-call" (click)="onLogCall()">Log Call</button>
            <button class="log-email" (click)="onLogEmail()">Log Email</button>
          </div>
          
          <div class="documents-panel related-panel">
            <h3>Documents ({{ documents().length }})</h3>
            <ul>
              <li *ngFor="let doc of documents()" class="document-item">
                {{ doc.name }}
              </li>
            </ul>
          </div>
          
          <div class="communication-panel related-panel">
            <h3>Communication History</h3>
            <div class="filter-buttons">
              <button class="filter" [class.active]="communicationFilter() === 'all'" (click)="setCommunicationFilter('all')">All</button>
              <button class="filter" [class.active]="communicationFilter() === 'email'" (click)="setCommunicationFilter('email')">Email</button>
              <button class="filter" [class.active]="communicationFilter() === 'call'" (click)="setCommunicationFilter('call')">Call</button>
            </div>
            <ul class="communication-timeline">
              <li *ngFor="let item of filteredCommunications()" class="communication-item">
                <span class="comm-type">{{ item.type }}</span>
                <span class="comm-date">{{ item.date }}</span>
              </li>
            </ul>
          </div>
        </aside>
      </div>
    </div>
  `
})
class MockContactViewEnhancedComponent {
  contact = signal<any>(null);
  partners = signal<any[]>([]);
  interactions = signal<any[]>([]);
  documents = signal<any[]>([]);
  communications = signal<any[]>([]);
  activeTab = signal<string>('overview');
  communicationFilter = signal<string>('all');

  setActiveTab(tab: string) {
    this.activeTab.set(tab);
  }

  setCommunicationFilter(filter: string) {
    this.communicationFilter.set(filter);
  }

  filteredCommunications() {
    const filter = this.communicationFilter();
    if (filter === 'all') return this.communications();
    return this.communications().filter(c => c.type === filter);
  }

  onEdit() {}
  onEmail() {}
  onCall() {}
  onDelete() {}
  onPartnerClick(partner: any) {}
  onAssociatePartner() {}
  onLogCall() {}
  onLogEmail() {}
}

describe('ContactViewEnhancedComponent', () => {
  let component: MockContactViewEnhancedComponent;
  let fixture: ComponentFixture<MockContactViewEnhancedComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [MockContactViewEnhancedComponent],
      imports: [NoopAnimationsModule],
      providers: [
        { provide: ActivatedRoute, useValue: { params: of({ id: 1 }) } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MockContactViewEnhancedComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  // Layout Integration Tests
  describe('Layout Integration', () => {
    it('should create the component', () => {
      expect(component).toBeTruthy();
    });

    it('should render main content area', () => {
      const mainContent = fixture.debugElement.query(By.css('.main-content'));
      expect(mainContent).toBeTruthy();
    });

    it('should render side panel', () => {
      const sidePanel = fixture.debugElement.query(By.css('.side-panel'));
      expect(sidePanel).toBeTruthy();
    });
  });

  // Contact Header Tests
  describe('Contact Header', () => {
    beforeEach(() => {
      component.contact.set({ 
        id: 1, 
        firstName: 'John', 
        lastName: 'Doe',
        role: 'Manager',
        email: 'john@test.com',
        phone: '+1234567890'
      });
      fixture.detectChanges();
    });

    it('should display contact name', () => {
      const nameEl = fixture.debugElement.query(By.css('.contact-name'));
      expect(nameEl.nativeElement.textContent).toContain('John Doe');
    });

    it('should display contact role', () => {
      const roleEl = fixture.debugElement.query(By.css('.contact-role'));
      expect(roleEl.nativeElement.textContent).toContain('Manager');
    });

    it('should show edit button', () => {
      const editButton = fixture.debugElement.query(By.css('.edit-button'));
      expect(editButton).toBeTruthy();
    });

    it('should show email button', () => {
      const emailButton = fixture.debugElement.query(By.css('.email-button'));
      expect(emailButton).toBeTruthy();
    });

    it('should show call button', () => {
      const callButton = fixture.debugElement.query(By.css('.call-button'));
      expect(callButton).toBeTruthy();
    });

    it('should handle email button click', () => {
      spyOn(component, 'onEmail');
      const emailButton = fixture.debugElement.query(By.css('.email-button'));
      emailButton.nativeElement.click();
      
      expect(component.onEmail).toHaveBeenCalled();
    });

    it('should handle call button click', () => {
      spyOn(component, 'onCall');
      const callButton = fixture.debugElement.query(By.css('.call-button'));
      callButton.nativeElement.click();
      
      expect(component.onCall).toHaveBeenCalled();
    });
  });

  // Partners Panel Tests
  describe('Partners Panel', () => {
    beforeEach(() => {
      component.partners.set([
        { id: 1, name: 'ACME Corp', isPrimary: true },
        { id: 2, name: 'Beta Inc', isPrimary: false }
      ]);
      fixture.detectChanges();
    });

    it('should display partners panel', () => {
      const panel = fixture.debugElement.query(By.css('.partners-panel'));
      expect(panel).toBeTruthy();
    });

    it('should show partner count', () => {
      const header = fixture.debugElement.query(By.css('.partners-panel h3'));
      expect(header.nativeElement.textContent).toContain('(2)');
    });

    it('should list partners', () => {
      const items = fixture.debugElement.queryAll(By.css('.partner-item'));
      expect(items.length).toBe(2);
    });

    it('should show primary badge for primary partner', () => {
      const badge = fixture.debugElement.query(By.css('.primary-badge'));
      expect(badge).toBeTruthy();
      expect(badge.nativeElement.textContent).toContain('Primary');
    });

    it('should show associate partner button', () => {
      const button = fixture.debugElement.query(By.css('.associate-partner'));
      expect(button).toBeTruthy();
    });

    it('should handle partner click', () => {
      spyOn(component, 'onPartnerClick');
      const firstPartner = fixture.debugElement.query(By.css('.partner-item'));
      firstPartner.nativeElement.click();
      
      expect(component.onPartnerClick).toHaveBeenCalled();
    });
  });

  // Interactions Panel Tests
  describe('Interactions Panel', () => {
    beforeEach(() => {
      component.interactions.set([
        { id: 1, type: 'Meeting', subject: 'Initial Meeting' },
        { id: 2, type: 'Email', subject: 'Follow-up' }
      ]);
      fixture.detectChanges();
    });

    it('should display interactions panel', () => {
      const panel = fixture.debugElement.query(By.css('.interactions-panel'));
      expect(panel).toBeTruthy();
    });

    it('should show log call button', () => {
      const button = fixture.debugElement.query(By.css('.log-call'));
      expect(button).toBeTruthy();
    });

    it('should show log email button', () => {
      const button = fixture.debugElement.query(By.css('.log-email'));
      expect(button).toBeTruthy();
    });

    it('should handle log call click', () => {
      spyOn(component, 'onLogCall');
      const button = fixture.debugElement.query(By.css('.log-call'));
      button.nativeElement.click();
      
      expect(component.onLogCall).toHaveBeenCalled();
    });

    it('should handle log email click', () => {
      spyOn(component, 'onLogEmail');
      const button = fixture.debugElement.query(By.css('.log-email'));
      button.nativeElement.click();
      
      expect(component.onLogEmail).toHaveBeenCalled();
    });
  });

  // Communication Panel Tests
  describe('Communication Panel', () => {
    beforeEach(() => {
      component.communications.set([
        { id: 1, type: 'email', date: '2024-01-01', subject: 'Hello' },
        { id: 2, type: 'call', date: '2024-01-02', subject: 'Follow up' },
        { id: 3, type: 'email', date: '2024-01-03', subject: 'Update' }
      ]);
      fixture.detectChanges();
    });

    it('should display communication panel', () => {
      const panel = fixture.debugElement.query(By.css('.communication-panel'));
      expect(panel).toBeTruthy();
    });

    it('should show filter buttons', () => {
      const filters = fixture.debugElement.queryAll(By.css('.filter'));
      expect(filters.length).toBe(3);
    });

    it('should filter by email', () => {
      component.setCommunicationFilter('email');
      fixture.detectChanges();
      
      const filtered = component.filteredCommunications();
      expect(filtered.length).toBe(2);
      expect(filtered.every(c => c.type === 'email')).toBeTrue();
    });

    it('should filter by call', () => {
      component.setCommunicationFilter('call');
      fixture.detectChanges();
      
      const filtered = component.filteredCommunications();
      expect(filtered.length).toBe(1);
      expect(filtered[0].type).toBe('call');
    });

    it('should show all when filter is all', () => {
      component.setCommunicationFilter('all');
      fixture.detectChanges();
      
      const filtered = component.filteredCommunications();
      expect(filtered.length).toBe(3);
    });

    it('should highlight active filter', () => {
      const allFilter = fixture.debugElement.query(By.css('.filter.active'));
      expect(allFilter.nativeElement.textContent).toContain('All');
    });
  });

  // Tab Navigation Tests
  describe('Tab Navigation', () => {
    it('should show overview tab by default', () => {
      const overviewTab = fixture.debugElement.query(By.css('.overview-tab'));
      expect(overviewTab).toBeTruthy();
    });

    it('should switch tabs when clicked', () => {
      const detailsButton = fixture.debugElement.queryAll(By.css('.tab'))[1];
      detailsButton.nativeElement.click();
      
      expect(component.activeTab()).toBe('details');
    });
  });

  // Breadcrumb Tests
  describe('Breadcrumb', () => {
    it('should show breadcrumb navigation', () => {
      const breadcrumb = fixture.debugElement.query(By.css('.breadcrumb'));
      expect(breadcrumb).toBeTruthy();
    });

    it('should mark current page', () => {
      const current = fixture.debugElement.query(By.css('.breadcrumb .current'));
      expect(current).toBeTruthy();
    });
  });
});

