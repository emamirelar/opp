/**
 * @fileoverview Test Data Seeder
 * API-based test data seeding and cleanup utilities
 * Creates and manages test data for E2E tests
 */

import { Page } from '@playwright/test';
import { 
  TestPartner, 
  TestContact, 
  TestInteraction, 
  TestOpportunity,
  TestDataBuilder 
} from './test-data-builder';

/**
 * Test Data Seeder - Creates and cleans up test data via API
 */
export class TestDataSeeder {
  private static createdPartners: number[] = [];
  private static createdContacts: number[] = [];
  private static createdInteractions: number[] = [];
  private static createdOpportunities: number[] = [];
  
  /**
   * Create a partner via API mock
   * @param partner - Partner data (optional, uses default if not provided)
   * @returns Created partner with ID
   */
  static async createPartner(partner?: Partial<TestPartner>): Promise<TestPartner> {
    const partnerData = partner ? 
      { ...TestDataBuilder.defaultPartner(), ...partner } : 
      TestDataBuilder.defaultPartner();
    
    // Mock ID generation (in real implementation, this would call API)
    const id = Math.floor(Math.random() * 10000) + 1;
    const createdPartner = { ...partnerData, id };
    
    // Track for cleanup
    this.createdPartners.push(id);
    
    console.log(`[TestDataSeeder] Created partner: ${createdPartner.name} (ID: ${id})`);
    return createdPartner;
  }
  
  /**
   * Create a contact via API mock
   * @param contact - Contact data (optional, uses default if not provided)
   * @returns Created contact with ID
   */
  static async createContact(contact?: Partial<TestContact>): Promise<TestContact> {
    // If no partner ID provided, create a partner first
    let partnerId = contact?.partnerId;
    if (!partnerId) {
      const partner = await this.createPartner();
      partnerId = partner.id!;
    }
    
    const contactData = contact ? 
      { ...TestDataBuilder.defaultContact(partnerId), ...contact, partnerId } : 
      TestDataBuilder.defaultContact(partnerId);
    
    // Mock ID generation
    const id = Math.floor(Math.random() * 10000) + 1;
    const createdContact = { ...contactData, id };
    
    // Track for cleanup
    this.createdContacts.push(id);
    
    console.log(`[TestDataSeeder] Created contact: ${createdContact.name} (ID: ${id})`);
    return createdContact;
  }
  
  /**
   * Create an interaction via API mock
   * @param interaction - Interaction data (optional, uses default if not provided)
   * @returns Created interaction with ID
   */
  static async createInteraction(interaction?: Partial<TestInteraction>): Promise<TestInteraction> {
    const interactionData = interaction ? 
      { ...TestDataBuilder.defaultInteraction(), ...interaction } : 
      TestDataBuilder.defaultInteraction();
    
    // Mock ID generation
    const id = Math.floor(Math.random() * 10000) + 1;
    const createdInteraction = { ...interactionData, id };
    
    // Track for cleanup
    this.createdInteractions.push(id);
    
    console.log(`[TestDataSeeder] Created interaction: ${createdInteraction.type} (ID: ${id})`);
    return createdInteraction;
  }
  
  /**
   * Create an opportunity via API mock
   * @param opportunity - Opportunity data (optional, uses default if not provided)
   * @returns Created opportunity with ID
   */
  static async createOpportunity(opportunity?: Partial<TestOpportunity>): Promise<TestOpportunity> {
    const opportunityData = opportunity ? 
      { ...TestDataBuilder.defaultOpportunity(), ...opportunity } : 
      TestDataBuilder.defaultOpportunity();
    
    // Mock ID generation
    const id = Math.floor(Math.random() * 10000) + 1;
    const createdOpportunity = { ...opportunityData, id };
    
    // Track for cleanup
    this.createdOpportunities.push(id);
    
    console.log(`[TestDataSeeder] Created opportunity: ${createdOpportunity.title} (ID: ${id})`);
    return createdOpportunity;
  }
  
  /**
   * Create a complete test scenario with partner, contacts, interactions
   * @returns Object with all created entities
   */
  static async createCompleteScenario(): Promise<{
    partner: TestPartner;
    contacts: TestContact[];
    interactions: TestInteraction[];
    opportunities: TestOpportunity[];
  }> {
    console.log('[TestDataSeeder] Creating complete test scenario...');
    
    // Create partner
    const partner = await this.createPartner({
      name: 'Complete Scenario Partner',
      type: 'Organization',
      description: 'Test partner for complete scenario'
    });
    
    // Create 2 contacts for the partner
    const contact1 = await this.createContact({
      partnerId: partner.id!,
      name: 'Primary Contact',
      email: 'primary@test.com',
      title: 'Director'
    });
    
    const contact2 = await this.createContact({
      partnerId: partner.id!,
      name: 'Secondary Contact',
      email: 'secondary@test.com',
      title: 'Manager'
    });
    
    // Create an interaction
    const interaction = await this.createInteraction({
      type: 'Meeting',
      description: 'Initial partnership discussion',
      participantIds: [contact1.id!, contact2.id!]
    });
    
    // Create an opportunity
    const opportunity = await this.createOpportunity({
      title: 'Partnership Opportunity',
      value: 500000,
      partnerIds: [partner.id!],
      contactIds: [contact1.id!, contact2.id!]
    });
    
    console.log('[TestDataSeeder] Complete scenario created successfully');
    
    return {
      partner,
      contacts: [contact1, contact2],
      interactions: [interaction],
      opportunities: [opportunity]
    };
  }
  
  /**
   * Delete a partner
   * @param id - Partner ID
   */
  static async deletePartner(id: number): Promise<void> {
    console.log(`[TestDataSeeder] Deleting partner ID: ${id}`);
    // In real implementation, this would call API
    this.createdPartners = this.createdPartners.filter(pid => pid !== id);
  }
  
  /**
   * Delete a contact
   * @param id - Contact ID
   */
  static async deleteContact(id: number): Promise<void> {
    console.log(`[TestDataSeeder] Deleting contact ID: ${id}`);
    // In real implementation, this would call API
    this.createdContacts = this.createdContacts.filter(cid => cid !== id);
  }
  
  /**
   * Delete an interaction
   * @param id - Interaction ID
   */
  static async deleteInteraction(id: number): Promise<void> {
    console.log(`[TestDataSeeder] Deleting interaction ID: ${id}`);
    // In real implementation, this would call API
    this.createdInteractions = this.createdInteractions.filter(iid => iid !== id);
  }
  
  /**
   * Delete an opportunity
   * @param id - Opportunity ID
   */
  static async deleteOpportunity(id: number): Promise<void> {
    console.log(`[TestDataSeeder] Deleting opportunity ID: ${id}`);
    // In real implementation, this would call API
    this.createdOpportunities = this.createdOpportunities.filter(oid => oid !== id);
  }
  
  /**
   * Clean up all created test data
   * Should be called in test.afterAll() or global teardown
   */
  static async cleanupAll(): Promise<void> {
    console.log('[TestDataSeeder] Cleaning up all test data...');
    
    // Delete in reverse order of creation (opportunities, interactions, contacts, partners)
    for (const id of this.createdOpportunities) {
      await this.deleteOpportunity(id);
    }
    
    for (const id of this.createdInteractions) {
      await this.deleteInteraction(id);
    }
    
    for (const id of this.createdContacts) {
      await this.deleteContact(id);
    }
    
    for (const id of this.createdPartners) {
      await this.deletePartner(id);
    }
    
    // Clear tracking arrays
    this.createdPartners = [];
    this.createdContacts = [];
    this.createdInteractions = [];
    this.createdOpportunities = [];
    
    console.log('[TestDataSeeder] Cleanup complete');
  }
  
  /**
   * Setup API route interception for test data
   * Call this in test.beforeEach() to mock API responses
   * @param page - Playwright page object
   */
  static async setupTestDataMocks(page: Page): Promise<void> {
    console.log('[TestDataSeeder] Setting up test data API mocks...');
    
    // Mock GET requests for detail pages (singular endpoints match the real API)
    await page.route('**/api/partner/*', async (route) => {
      const url = route.request().url();
      const idMatch = url.match(/\/partner\/(\d+)/);
      
      if (idMatch && route.request().method() === 'GET') {
        const partnerId = parseInt(idMatch[1]);
        const partner = TestDataBuilder.partner()
          .withId(partnerId)
          .withName(`Partner ${partnerId}`)
          .build();
        
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(partner)
        });
      } else {
        await route.continue();
      }
    });
    
    await page.route('**/api/contact/*', async (route) => {
      const url = route.request().url();
      const idMatch = url.match(/\/contact\/(\d+)/);
      
      if (idMatch && route.request().method() === 'GET') {
        const contactId = parseInt(idMatch[1]);
        const contact = TestDataBuilder.contact()
          .withId(contactId)
          .withName(`Contact ${contactId}`)
          .build();
        
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(contact)
        });
      } else {
        await route.continue();
      }
    });
    
    console.log('[TestDataSeeder] Test data mocks configured');
  }
  
  /**
   * Get statistics about created test data
   */
  static getStats(): {
    partners: number;
    contacts: number;
    interactions: number;
    opportunities: number;
    total: number;
  } {
    return {
      partners: this.createdPartners.length,
      contacts: this.createdContacts.length,
      interactions: this.createdInteractions.length,
      opportunities: this.createdOpportunities.length,
      total: this.createdPartners.length + 
             this.createdContacts.length + 
             this.createdInteractions.length + 
             this.createdOpportunities.length
    };
  }
}
