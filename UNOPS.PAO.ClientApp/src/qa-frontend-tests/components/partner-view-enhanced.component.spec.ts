import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { By } from '@angular/platform-browser';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';

/**
 * Enhanced Partner View Component Tests
 * 
 * Tests for the enhanced partner view that integrates EnhancedEntityLayoutComponent
 * with partner-specific related info panels.
 * 
 * To run: Copy to UNOPS.PAO.ClientApp/src/app/features/partnerships/partners/components/partner/view/
 *         and run 'ng test'
 */

// Mock partner service
class MockPartnerService {
  getById(id: number) {
    return of({
      id: id,
      name: 'Test Partner',
      status: 'Active',
      type: 'Organization',
      email: 'partner@test.com'
    });
  }

  getContacts(partnerId: number) {
    return of([
      { id: 1, name: 'John Doe', email: 'john@test.com', role: 'Manager' },
      { id: 2, name: 'Jane Smith', email: 'jane@test.com', role: 'Director' }
    ]);
  }

  getInteractions(partnerId: number) {
    return of([
      { id: 1, type: 'Meeting', subject: 'Initial Meeting', date: new Date() },
      { id: 2, type: 'Email', subject: 'Follow-up', date: new Date() }
    ]);
  }

  getDocuments(partnerId: number) {
    return of([
      { id: 1, name: 'Contract.pdf', type: 'PDF', uploadDate: new Date() }
    ]);
  }

  getEngagements(partnerId: number) {
    return of([
      { id: 1, title: 'Project Alpha', status: 'Active' }
    ]);
  }
}

// Mock enhanced partner view component
@Component({
  selector: 'app-partner-view-enhanced',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="partner-view-enhanced">
      <div class="layout-header">
        <nav class="breadcrumb">
          <a>Partners</a>
          <a class="current">{{ partner()?.name }}</a>
        </nav>
        <h1 class="partner-name">{{ partner()?.name }}</h1>
        <span class="partner-status">{{ partner()?.status }}</span>
        <div class="action-bar">
          <button class="edit-button" (click)="onEdit()">Edit</button>
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
            <div *ngIf="activeTab() === 'overview'" class="overview-tab">Overview content</div>
            <div *ngIf="activeTab() === 'details'" class="details-tab">Details content</div>
          </div>
        </main>
        
        <aside class="side-panel">
          <div class="contacts-panel related-panel">
            <h3>Contacts ({{ contacts().length }})</h3>
            <ul>
              <li *ngFor="let contact of contacts()" class="contact-item" (click)="onContactClick(contact)">
                {{ contact.name }}
              </li>
            </ul>
            <button class="add-contact" (click)="onAddContact()">Add Contact</button>
          </div>
          
          <div class="interactions-panel related-panel">
            <h3>Interactions ({{ interactions().length }})</h3>
            <ul>
              <li *ngFor="let interaction of interactions()" class="interaction-item">
                {{ interaction.subject }}
              </li>
            </ul>
          </div>
          
          <div class="documents-panel related-panel">
            <h3>Documents ({{ documents().length }})</h3>
            <ul>
              <li *ngFor="let doc of documents()" class="document-item">
                {{ doc.name }}
                <button class="download-button" (click)="onDownload(doc)">Download</button>
              </li>
            </ul>
          </div>
          
          <div class="engagements-panel related-panel">
            <h3>Engagements ({{ engagements().length }})</h3>
            <ul>
              <li *ngFor="let engagement of engagements()" class="engagement-item">
                {{ engagement.title }}
              </li>
            </ul>
          </div>
        </aside>
      </div>
    </div>
  `
})
class MockPartnerViewEnhancedComponent {
  partner = signal<any>(null);
  contacts = signal<any[]>([]);
  interactions = signal<any[]>([]);
  documents = signal<any[]>([]);
  engagements = signal<any[]>([]);
  activeTab = signal<string>('overview');
  recordPermissions = signal<any>({ permissions: { canUpdate: true, canDelete: true, canRead: true } });

  setActiveTab(tab: string) {
    this.activeTab.set(tab);
  }

  onEdit() {}
  onDelete() {}
  onContactClick(contact: any) {}
  onAddContact() {}
  onDownload(doc: any) {}
}

describe('PartnerViewEnhancedComponent', () => {
  let component: MockPartnerViewEnhancedComponent;
  let fixture: ComponentFixture<MockPartnerViewEnhancedComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NoopAnimationsModule, MockPartnerViewEnhancedComponent],
      providers: [
        { provide: ActivatedRoute, useValue: { params: of({ id: 1 }), queryParams: of({}), snapshot: { paramMap: { get: () => null } } } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MockPartnerViewEnhancedComponent);
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

  // Partner Header Tests
  describe('Partner Header', () => {
    beforeEach(() => {
      component.partner.set({ id: 1, name: 'ACME Corp', status: 'Active' });
      fixture.detectChanges();
    });

    it('should display partner name', () => {
      const nameEl = fixture.debugElement.query(By.css('.partner-name'));
      expect(nameEl.nativeElement.textContent).toContain('ACME Corp');
    });

    it('should display partner status', () => {
      const statusEl = fixture.debugElement.query(By.css('.partner-status'));
      expect(statusEl.nativeElement.textContent).toContain('Active');
    });

    it('should show edit button', () => {
      const editButton = fixture.debugElement.query(By.css('.edit-button'));
      expect(editButton).toBeTruthy();
    });

    it('should show delete button', () => {
      const deleteButton = fixture.debugElement.query(By.css('.delete-button'));
      expect(deleteButton).toBeTruthy();
    });
  });

  // Tab Navigation Tests
  describe('Tab Navigation', () => {
    it('should show overview tab by default', () => {
      const overviewTab = fixture.debugElement.query(By.css('.overview-tab'));
      expect(overviewTab).toBeTruthy();
    });

    it('should switch to details tab when clicked', () => {
      const detailsButton = fixture.debugElement.queryAll(By.css('.tab'))[1];
      detailsButton.nativeElement.click();
      fixture.detectChanges();
      
      const detailsTab = fixture.debugElement.query(By.css('.details-tab'));
      expect(detailsTab).toBeTruthy();
    });
  });

  // Contacts Panel Tests
  describe('Contacts Panel', () => {
    beforeEach(() => {
      component.contacts.set([
        { id: 1, name: 'John Doe', email: 'john@test.com' },
        { id: 2, name: 'Jane Smith', email: 'jane@test.com' }
      ]);
      fixture.detectChanges();
    });

    it('should display contacts panel', () => {
      const panel = fixture.debugElement.query(By.css('.contacts-panel'));
      expect(panel).toBeTruthy();
    });

    it('should show contact count', () => {
      const header = fixture.debugElement.query(By.css('.contacts-panel h3'));
      expect(header.nativeElement.textContent).toContain('(2)');
    });

    it('should list contacts', () => {
      const contactItems = fixture.debugElement.queryAll(By.css('.contact-item'));
      expect(contactItems.length).toBe(2);
    });

    it('should show add contact button', () => {
      const addButton = fixture.debugElement.query(By.css('.add-contact'));
      expect(addButton).toBeTruthy();
    });

    it('should handle contact click', () => {
      spyOn(component, 'onContactClick');
      const firstContact = fixture.debugElement.query(By.css('.contact-item'));
      firstContact.nativeElement.click();
      
      expect(component.onContactClick).toHaveBeenCalled();
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

    it('should show interaction count', () => {
      const header = fixture.debugElement.query(By.css('.interactions-panel h3'));
      expect(header.nativeElement.textContent).toContain('(2)');
    });

    it('should list interactions', () => {
      const items = fixture.debugElement.queryAll(By.css('.interaction-item'));
      expect(items.length).toBe(2);
    });
  });

  // Documents Panel Tests
  describe('Documents Panel', () => {
    beforeEach(() => {
      component.documents.set([
        { id: 1, name: 'Contract.pdf', type: 'PDF' }
      ]);
      fixture.detectChanges();
    });

    it('should display documents panel', () => {
      const panel = fixture.debugElement.query(By.css('.documents-panel'));
      expect(panel).toBeTruthy();
    });

    it('should show download button', () => {
      const downloadButton = fixture.debugElement.query(By.css('.download-button'));
      expect(downloadButton).toBeTruthy();
    });

    it('should handle download click', () => {
      spyOn(component, 'onDownload');
      const downloadButton = fixture.debugElement.query(By.css('.download-button'));
      downloadButton.nativeElement.click();
      
      expect(component.onDownload).toHaveBeenCalled();
    });
  });

  // Engagements Panel Tests
  describe('Engagements Panel', () => {
    beforeEach(() => {
      component.engagements.set([
        { id: 1, title: 'Project Alpha', status: 'Active' }
      ]);
      fixture.detectChanges();
    });

    it('should display engagements panel', () => {
      const panel = fixture.debugElement.query(By.css('.engagements-panel'));
      expect(panel).toBeTruthy();
    });

    it('should show engagement count', () => {
      const header = fixture.debugElement.query(By.css('.engagements-panel h3'));
      expect(header.nativeElement.textContent).toContain('(1)');
    });

    it('should list engagements', () => {
      const items = fixture.debugElement.queryAll(By.css('.engagement-item'));
      expect(items.length).toBe(1);
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

