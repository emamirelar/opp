/**
 * @fileoverview Test Data Builder
 * Fluent builder pattern for creating test data entities
 * Provides consistent, repeatable test data generation
 */

/**
 * Partner test data interface
 */
export interface TestPartner {
  id?: number;
  name: string;
  type: string;
  status?: string;
  description?: string;
  website?: string;
}

/**
 * Contact test data interface
 */
export interface TestContact {
  id?: number;
  name: string;
  email: string;
  phone?: string;
  title?: string;
  partnerId: number;
  partnerName?: string;
  department?: string;
}

/**
 * Interaction test data interface
 */
export interface TestInteraction {
  id?: number;
  type: string;
  date: string;
  description?: string;
  location?: string;
  participantIds?: number[];
  partnerIds?: number[];
}

/**
 * Opportunity test data interface
 */
export interface TestOpportunity {
  id?: number;
  title: string;
  value?: number;
  stage?: string;
  startDate?: string;
  endDate?: string;
  description?: string;
  partnerIds?: number[];
  contactIds?: number[];
}

/**
 * Partner Builder - Fluent interface for building partner test data
 */
export class PartnerBuilder {
  private partner: TestPartner;
  
  constructor() {
    // Default partner data
    this.partner = {
      name: `Test Partner ${Date.now()}`,
      type: 'Organization',
      status: 'Active',
    };
  }
  
  withId(id: number): PartnerBuilder {
    this.partner.id = id;
    return this;
  }
  
  withName(name: string): PartnerBuilder {
    this.partner.name = name;
    return this;
  }
  
  withType(type: string): PartnerBuilder {
    this.partner.type = type;
    return this;
  }
  
  withStatus(status: string): PartnerBuilder {
    this.partner.status = status;
    return this;
  }
  
  withDescription(description: string): PartnerBuilder {
    this.partner.description = description;
    return this;
  }
  
  withWebsite(website: string): PartnerBuilder {
    this.partner.website = website;
    return this;
  }
  
  build(): TestPartner {
    return { ...this.partner };
  }
}

/**
 * Contact Builder - Fluent interface for building contact test data
 */
export class ContactBuilder {
  private contact: TestContact;
  
  constructor(partnerId: number = 1) {
    const timestamp = Date.now();
    // Default contact data
    this.contact = {
      name: `Test Contact ${timestamp}`,
      email: `testcontact${timestamp}@example.com`,
      partnerId: partnerId,
    };
  }
  
  withId(id: number): ContactBuilder {
    this.contact.id = id;
    return this;
  }
  
  withName(name: string): ContactBuilder {
    this.contact.name = name;
    return this;
  }
  
  withEmail(email: string): ContactBuilder {
    this.contact.email = email;
    return this;
  }
  
  withPhone(phone: string): ContactBuilder {
    this.contact.phone = phone;
    return this;
  }
  
  withTitle(title: string): ContactBuilder {
    this.contact.title = title;
    return this;
  }
  
  withPartner(partnerId: number, partnerName?: string): ContactBuilder {
    this.contact.partnerId = partnerId;
    if (partnerName) {
      this.contact.partnerName = partnerName;
    }
    return this;
  }
  
  withDepartment(department: string): ContactBuilder {
    this.contact.department = department;
    return this;
  }
  
  build(): TestContact {
    return { ...this.contact };
  }
}

/**
 * Interaction Builder - Fluent interface for building interaction test data
 */
export class InteractionBuilder {
  private interaction: TestInteraction;
  
  constructor() {
    // Default interaction data
    this.interaction = {
      type: 'Meeting',
      date: new Date().toISOString().split('T')[0],
    };
  }
  
  withId(id: number): InteractionBuilder {
    this.interaction.id = id;
    return this;
  }
  
  withType(type: string): InteractionBuilder {
    this.interaction.type = type;
    return this;
  }
  
  withDate(date: string): InteractionBuilder {
    this.interaction.date = date;
    return this;
  }
  
  withDescription(description: string): InteractionBuilder {
    this.interaction.description = description;
    return this;
  }
  
  withLocation(location: string): InteractionBuilder {
    this.interaction.location = location;
    return this;
  }
  
  withParticipants(participantIds: number[]): InteractionBuilder {
    this.interaction.participantIds = participantIds;
    return this;
  }
  
  withPartners(partnerIds: number[]): InteractionBuilder {
    this.interaction.partnerIds = partnerIds;
    return this;
  }
  
  build(): TestInteraction {
    return { ...this.interaction };
  }
}

/**
 * Opportunity Builder - Fluent interface for building opportunity test data
 */
export class OpportunityBuilder {
  private opportunity: TestOpportunity;
  
  constructor() {
    // Default opportunity data
    this.opportunity = {
      title: `Test Opportunity ${Date.now()}`,
      stage: 'Draft',
    };
  }
  
  withId(id: number): OpportunityBuilder {
    this.opportunity.id = id;
    return this;
  }
  
  withTitle(title: string): OpportunityBuilder {
    this.opportunity.title = title;
    return this;
  }
  
  withValue(value: number): OpportunityBuilder {
    this.opportunity.value = value;
    return this;
  }
  
  withStage(stage: string): OpportunityBuilder {
    this.opportunity.stage = stage;
    return this;
  }
  
  withStartDate(startDate: string): OpportunityBuilder {
    this.opportunity.startDate = startDate;
    return this;
  }
  
  withEndDate(endDate: string): OpportunityBuilder {
    this.opportunity.endDate = endDate;
    return this;
  }
  
  withDescription(description: string): OpportunityBuilder {
    this.opportunity.description = description;
    return this;
  }
  
  withPartners(partnerIds: number[]): OpportunityBuilder {
    this.opportunity.partnerIds = partnerIds;
    return this;
  }
  
  withContacts(contactIds: number[]): OpportunityBuilder {
    this.opportunity.contactIds = contactIds;
    return this;
  }
  
  build(): TestOpportunity {
    return { ...this.opportunity };
  }
}

/**
 * Main Test Data Builder - Entry point for fluent test data creation
 */
export class TestDataBuilder {
  /**
   * Create a partner builder
   */
  static partner(): PartnerBuilder {
    return new PartnerBuilder();
  }
  
  /**
   * Create a contact builder
   * @param partnerId - Partner ID for the contact
   */
  static contact(partnerId: number = 1): ContactBuilder {
    return new ContactBuilder(partnerId);
  }
  
  /**
   * Create an interaction builder
   */
  static interaction(): InteractionBuilder {
    return new InteractionBuilder();
  }
  
  /**
   * Create an opportunity builder
   */
  static opportunity(): OpportunityBuilder {
    return new OpportunityBuilder();
  }
  
  /**
   * Create a default partner for testing
   */
  static defaultPartner(): TestPartner {
    return new PartnerBuilder()
      .withName('Default Test Partner')
      .withType('Organization')
      .withStatus('Active')
      .withDescription('This is a test partner for automated testing')
      .build();
  }
  
  /**
   * Create a default contact for testing
   * @param partnerId - Partner ID for the contact
   */
  static defaultContact(partnerId: number = 1): TestContact {
    const timestamp = Date.now();
    return new ContactBuilder(partnerId)
      .withName('Default Test Contact')
      .withEmail(`default${timestamp}@test.com`)
      .withPhone('+1 (555) 123-4567')
      .withTitle('Test Manager')
      .build();
  }
  
  /**
   * Create a default interaction for testing
   */
  static defaultInteraction(): TestInteraction {
    return new InteractionBuilder()
      .withType('Meeting')
      .withDate(new Date().toISOString().split('T')[0])
      .withDescription('Default test interaction for automated testing')
      .withLocation('Virtual/Online')
      .build();
  }
  
  /**
   * Create a default opportunity for testing
   */
  static defaultOpportunity(): TestOpportunity {
    return new OpportunityBuilder()
      .withTitle('Default Test Opportunity')
      .withValue(100000)
      .withStage('Draft')
      .withDescription('Default test opportunity for automated testing')
      .build();
  }
}
