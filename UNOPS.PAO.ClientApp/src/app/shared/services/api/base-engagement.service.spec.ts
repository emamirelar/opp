import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { BaseEngagementService } from './base-engagement.service';
import { BaseEngagement, BaseEngagementPartner } from '../../models/base-engagement.model';

describe('BaseEngagementService', () => {
  let service: BaseEngagementService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [BaseEngagementService]
    });

    service = TestBed.inject(BaseEngagementService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get base engagements without partner filter', (done) => {
    const mockEngagements: BaseEngagement[] = [
      { id: 1, name: 'Engagement 1', stage: 'signed' } as unknown as BaseEngagement,
      { id: 2, name: 'Engagement 2', stage: 'pipeline' } as unknown as BaseEngagement
    ];

    expect(service.isLoading()).toBe(false);

    service.getBaseEngagements().subscribe(engagements => {
      expect(engagements).toEqual(mockEngagements);
      expect(service.allBaseEngagements()).toEqual(mockEngagements);
      expect(service.isLoading()).toBe(false);
      done();
    });

    expect(service.isLoading()).toBe(true);

    const req = httpMock.expectOne('/api/base-engagements');
    expect(req.request.method).toBe('GET');
    req.flush(mockEngagements);
  });

  it('should get base engagements with partner filter', (done) => {
    const partnerId = 123;
    const mockEngagements: BaseEngagement[] = [
      { id: 1, name: 'Engagement 1', stage: 'signed' } as unknown as BaseEngagement
    ];

    service.getBaseEngagements(partnerId).subscribe(engagements => {
      expect(engagements).toEqual(mockEngagements);
      expect(service.isLoading()).toBe(false);
      done();
    });

    const req = httpMock.expectOne('/api/base-engagements?partnerId=123');
    expect(req.request.method).toBe('GET');
    req.flush(mockEngagements);
  });

  it('should get base engagement by ID', (done) => {
    const mockEngagement: BaseEngagement = {
      id: 1,
      name: 'Engagement 1',
      stage: 'signed'
    } as unknown as BaseEngagement;

    service.getBaseEngagementById(1).subscribe(engagement => {
      expect(engagement).toEqual(mockEngagement);
      expect(service.isLoading()).toBe(false);
      done();
    });

    const req = httpMock.expectOne('/api/base-engagements/1');
    expect(req.request.method).toBe('GET');
    req.flush(mockEngagement);
  });

  it('should get base engagements by partner ID', (done) => {
    const partnerId = 456;
    const mockEngagements: BaseEngagement[] = [
      { id: 1, name: 'Engagement 1', stage: 'signed' } as unknown as BaseEngagement
    ];

    service.getBaseEngagementsByPartnerId(partnerId).subscribe(engagements => {
      expect(engagements).toEqual(mockEngagements);
      expect(service.isLoading()).toBe(false);
      done();
    });

    const req = httpMock.expectOne('/api/base-engagements?partnerId=456');
    expect(req.request.method).toBe('GET');
    req.flush(mockEngagements);
  });

  it('should get engagement partners', (done) => {
    const engagementId = 789;
    const mockPartners: BaseEngagementPartner[] = [
      { id: 1, name: 'Partner 1', partnerType: 'lead' } as unknown as BaseEngagementPartner
    ];

    service.getEngagementPartners(engagementId).subscribe(partners => {
      expect(partners).toEqual(mockPartners);
      expect(service.isLoading()).toBe(false);
      done();
    });

    const req = httpMock.expectOne('/api/base-engagements/789/partners');
    expect(req.request.method).toBe('GET');
    req.flush(mockPartners);
  });

  it('should get stage severity for signed', () => {
    expect(service.getStageSeverity('signed')).toBe('success');
    expect(service.getStageSeverity('SIGNED')).toBe('success');
  });

  it('should get stage severity for implementation', () => {
    expect(service.getStageSeverity('implementation')).toBe('success');
  });

  it('should get stage severity for completed', () => {
    expect(service.getStageSeverity('completed')).toBe('info');
  });

  it('should get stage severity for pipeline', () => {
    expect(service.getStageSeverity('pipeline')).toBe('warn');
  });

  it('should get stage severity for development', () => {
    expect(service.getStageSeverity('development')).toBe('warn');
  });

  it('should get stage severity for cancelled', () => {
    expect(service.getStageSeverity('cancelled')).toBe('danger');
  });

  it('should get stage severity for on hold', () => {
    expect(service.getStageSeverity('on hold')).toBe('danger');
  });

  it('should get default stage severity', () => {
    expect(service.getStageSeverity('unknown')).toBe('info');
    expect(service.getStageSeverity(null as any)).toBe('info');
  });

  it('should get partner type color for lead', () => {
    expect(service.getPartnerTypeColor('lead')).toBe('#3B82F6');
    expect(service.getPartnerTypeColor('LEAD')).toBe('#3B82F6');
  });

  it('should get partner type color for implementing', () => {
    expect(service.getPartnerTypeColor('implementing')).toBe('#10B981');
  });

  it('should get partner type color for funding', () => {
    expect(service.getPartnerTypeColor('funding')).toBe('#F59E0B');
  });

  it('should get partner type color for technical', () => {
    expect(service.getPartnerTypeColor('technical')).toBe('#8B5CF6');
  });

  it('should get partner type color for government', () => {
    expect(service.getPartnerTypeColor('government')).toBe('#EF4444');
  });

  it('should get default partner type color', () => {
    expect(service.getPartnerTypeColor('unknown')).toBe('#6B7280');
    expect(service.getPartnerTypeColor(null as any)).toBe('#6B7280');
  });

  it('should clear cache', () => {
    const mockEngagements: BaseEngagement[] = [
      { id: 1, name: 'Engagement 1', stage: 'signed' } as unknown as BaseEngagement
    ];

    service.getBaseEngagements().subscribe();
    const req = httpMock.expectOne('/api/base-engagements');
    req.flush(mockEngagements);

    expect(service.allBaseEngagements().length).toBe(1);

    service.clearCache();

    expect(service.allBaseEngagements().length).toBe(0);
  });

  it('should refresh engagements', () => {
    service.refreshEngagements();

    const req = httpMock.expectOne('/api/base-engagements');
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should set isLoading to false on error', (done) => {
    service.getBaseEngagements().subscribe({
      next: () => fail('should have errored'),
      error: () => {
        expect(service.isLoading()).toBe(false);
        done();
      }
    });

    const req = httpMock.expectOne('/api/base-engagements');
    req.error(new ProgressEvent('error'));
  });
});

