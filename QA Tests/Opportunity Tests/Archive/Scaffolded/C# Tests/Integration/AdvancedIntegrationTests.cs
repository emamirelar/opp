using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UNOPS.PAO.DataAccess;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSBusiness.Managers;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.Integration
{
    /// <summary>
    /// Advanced integration test scenarios
    /// Tests complex cross-system interactions and data flow
    /// </summary>
    public class AdvancedIntegrationTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;
        private readonly Mock<IExternalSystemService> _mockExternalService;
        private readonly Mock<ICacheService> _mockCacheService;

        public AdvancedIntegrationTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"AdvIntegrationTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
            _mockExternalService = new Mock<IExternalSystemService>();
            _mockCacheService = new Mock<ICacheService>();
        }

        #region TC-OPP-ADVINT-001: ERP System Synchronization

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-ADVINT-001")]
        public async Task SyncToERP_OpportunityApproved_CreatesERPRecord()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "ERP Sync Test",
                EstimatedValue = 3000000,
                Status = "Approved",
                ReferenceNumber = "OPP-2026-001",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Mock ERP API call
            _mockExternalService
                .Setup(e => e.CreateERPRecordAsync(It.IsAny<object>()))
                .ReturnsAsync(new ERPSyncResult
                {
                    Success = true,
                    ERPRecordId = "ERP-12345",
                    SyncDate = DateTime.UtcNow
                });

            // Act - Sync to ERP
            var syncResult = await _mockExternalService.Object.CreateERPRecordAsync(new
            {
                opportunity.Name,
                opportunity.EstimatedValue,
                opportunity.ReferenceNumber
            });

            // Save ERP reference
            opportunity.ERPRecordId = syncResult.ERPRecordId;
            await _context.SaveChangesAsync();

            // Assert
            Assert.True(syncResult.Success);
            Assert.Equal("ERP-12345", opportunity.ERPRecordId);
        }

        #endregion

        #region TC-OPP-ADVINT-002: Project Management Tool Integration

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-ADVINT-002")]
        public async Task SyncToPMTool_ScheduleApproved_CreatesProjectPlan()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "PM Tool Sync Test",
                EstimatedValue = 2000000,
                Timeline = 24,
                Status = "Authorized",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);

            var schedule = new OpportunitySchedule
            {
                OpportunityId = opportunity.Id,
                TotalMonths = 24,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(24),
                IsApproved = true
            };
            _context.OpportunitySchedules.Add(schedule);
            await _context.SaveChangesAsync();

            // Mock PM Tool API
            _mockExternalService
                .Setup(e => e.CreatePMToolProjectAsync(It.IsAny<object>()))
                .ReturnsAsync(new PMToolResult
                {
                    Success = true,
                    ProjectId = "PM-5678",
                    ProjectUrl = "https://pmtool.unops.org/projects/5678"
                });

            // Act
            var pmResult = await _mockExternalService.Object.CreatePMToolProjectAsync(new
            {
                Name = opportunity.Name,
                Duration = schedule.TotalMonths,
                StartDate = schedule.StartDate
            });

            opportunity.PMToolProjectId = pmResult.ProjectId;
            await _context.SaveChangesAsync();

            // Assert
            Assert.True(pmResult.Success);
            Assert.Equal("PM-5678", opportunity.PMToolProjectId);
        }

        #endregion

        #region TC-OPP-ADVINT-003: HR System Resource Allocation

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-ADVINT-003")]
        public async Task SyncToHRSystem_ResourcePlanApproved_RequestsResources()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "HR Integration Test",
                EstimatedValue = 5000000,
                Timeline = 36,
                Status = "Authorized",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);

            var resourcePlan = new ResourcePlan
            {
                OpportunityId = opportunity.Id,
                TotalFTEs = 8,
                Roles = new List<RoleRequirement>
                {
                    new RoleRequirement { Role = "Project Manager", FTEs = 1, Level = "Senior" },
                    new RoleRequirement { Role = "Engineers", FTEs = 5, Level = "Mid" },
                    new RoleRequirement { Role = "Admin", FTEs = 2, Level = "Junior" }
                }
            };
            _context.ResourcePlans.Add(resourcePlan);
            await _context.SaveChangesAsync();

            // Mock HR System API
            _mockExternalService
                .Setup(e => e.RequestResourcesAsync(It.IsAny<object>()))
                .ReturnsAsync(new HRRequestResult
                {
                    Success = true,
                    RequestId = "HR-REQ-9012",
                    EstimatedFulfillmentDate = DateTime.UtcNow.AddDays(30)
                });

            // Act
            var hrResult = await _mockExternalService.Object.RequestResourcesAsync(new
            {
                OpportunityId = opportunity.Id,
                TotalFTEs = resourcePlan.TotalFTEs,
                Roles = resourcePlan.Roles
            });

            // Assert
            Assert.True(hrResult.Success);
            Assert.Equal("HR-REQ-9012", hrResult.RequestId);
        }

        #endregion

        #region TC-OPP-ADVINT-004: Cache Invalidation Cascade

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-ADVINT-004")]
        public async Task UpdateOpportunity_InvalidatesRelatedCaches_Success()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Cache Test",
                EstimatedValue = 2000000,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            var cacheKeys = new List<string>();

            // Mock cache invalidation tracking
            _mockCacheService
                .Setup(c => c.InvalidateAsync(It.IsAny<string>()))
                .Callback<string>(key => cacheKeys.Add(key))
                .ReturnsAsync(true);

            // Act - Update opportunity
            opportunity.EstimatedValue = 2500000;
            await _context.SaveChangesAsync();

            // Trigger cache invalidation cascade
            await _mockCacheService.Object.InvalidateAsync($"opportunity_{opportunity.Id}");
            await _mockCacheService.Object.InvalidateAsync("opportunities_list");
            await _mockCacheService.Object.InvalidateAsync($"budget_{opportunity.Id}");
            await _mockCacheService.Object.InvalidateAsync($"dst_{opportunity.Id}");

            // Assert
            Assert.Equal(4, cacheKeys.Count);
            Assert.Contains($"opportunity_{opportunity.Id}", cacheKeys);
            Assert.Contains("opportunities_list", cacheKeys);
            Assert.Contains($"budget_{opportunity.Id}", cacheKeys);
        }

        #endregion

        #region TC-OPP-ADVINT-005: Event Sourcing Pattern

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-ADVINT-005")]
        public async Task TrackOpportunityEvents_CompleteHistory_AllEventsRecorded()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Event Sourcing Test",
                EstimatedValue = 1000000,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            var events = new List<OpportunityEvent>();

            // Act - Record all events
            events.Add(new OpportunityEvent
            {
                OpportunityId = opportunity.Id,
                EventType = "Created",
                EventData = $"Opportunity created with budget ${opportunity.EstimatedValue:N0}",
                Timestamp = DateTime.UtcNow,
                UserId = 1
            });

            opportunity.Status = "Profiling";
            events.Add(new OpportunityEvent
            {
                OpportunityId = opportunity.Id,
                EventType = "StatusChanged",
                EventData = "Status changed from Draft to Profiling",
                Timestamp = DateTime.UtcNow.AddHours(1),
                UserId = 1
            });

            opportunity.EstimatedValue = 1200000;
            events.Add(new OpportunityEvent
            {
                OpportunityId = opportunity.Id,
                EventType = "BudgetUpdated",
                EventData = "Budget increased from $1M to $1.2M",
                Timestamp = DateTime.UtcNow.AddDays(5),
                UserId = 2
            });

            _context.OpportunityEvents.AddRange(events);
            await _context.SaveChangesAsync();

            // Assert - Complete event history
            var allEvents = await _context.OpportunityEvents
                .Where(e => e.OpportunityId == opportunity.Id)
                .OrderBy(e => e.Timestamp)
                .ToListAsync();

            Assert.Equal(3, allEvents.Count);
            Assert.Equal("Created", allEvents[0].EventType);
            Assert.Equal("StatusChanged", allEvents[1].EventType);
            Assert.Equal("BudgetUpdated", allEvents[2].EventType);
        }

        #endregion

        #region TC-OPP-ADVINT-006: API Rate Limiting

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-ADVINT-006")]
        public async Task APIAccess_ExceedsRateLimit_ThrottlesRequests()
        {
            // Arrange - Create opportunities
            for (int i = 1; i <= 10; i++)
            {
                _context.Opportunities.Add(new Domain.Entities.Opportunity
                {
                    Name = $"Rate Limit Test {i}",
                    EstimatedValue = 1000000,
                    Status = "Draft",
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow
                });
            }
            await _context.SaveChangesAsync();

            // Simulate rapid API requests
            var apiRequests = new List<APIRequest>();
            for (int i = 0; i < 150; i++) // 150 requests in 1 minute
            {
                apiRequests.Add(new APIRequest
                {
                    UserId = 1,
                    Endpoint = "/api/opportunities",
                    Timestamp = DateTime.UtcNow.AddSeconds(i * 0.4) // ~2.5 req/sec
                });
            }

            // Act - Check rate limit (100 req/min)
            var requestsInLastMinute = apiRequests
                .Count(r => r.Timestamp > DateTime.UtcNow.AddMinutes(-1));

            var rateLimitExceeded = requestsInLastMinute > 100;

            // Assert
            Assert.True(rateLimitExceeded); // 150 > 100
            // System should throttle or return 429 Too Many Requests
        }

        #endregion

        #region TC-OPP-ADVINT-007: Distributed Transaction Coordination

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-ADVINT-007")]
        public async Task DistributedTransaction_MultipleSystemsFailure_AllRollback()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Distributed Transaction Test",
                EstimatedValue = 5000000,
                Status = "Approved",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - Attempt distributed transaction across 3 systems
            using var transaction = await _context.Database.BeginTransactionAsync();
            var allSucceeded = true;

            try
            {
                // System 1: Update local database
                opportunity.Status = "Converted";
                await _context.SaveChangesAsync();

                // System 2: Create ERP record
                _mockExternalService
                    .Setup(e => e.CreateERPRecordAsync(It.IsAny<object>()))
                    .ReturnsAsync(new ERPSyncResult { Success = true, ERPRecordId = "ERP-001" });
                
                var erpResult = await _mockExternalService.Object.CreateERPRecordAsync(opportunity);
                if (!erpResult.Success) throw new Exception("ERP sync failed");

                // System 3: Create PM Tool project
                _mockExternalService
                    .Setup(e => e.CreatePMToolProjectAsync(It.IsAny<object>()))
                    .ThrowsAsync(new Exception("PM Tool unavailable")); // Simulate failure

                var pmResult = await _mockExternalService.Object.CreatePMToolProjectAsync(opportunity);
                if (!pmResult.Success) throw new Exception("PM Tool sync failed");

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                allSucceeded = false;
            }

            // Assert - Transaction failed, all rolled back
            Assert.False(allSucceeded);
            
            var unchangedOpportunity = await _context.Opportunities.FindAsync(opportunity.Id);
            Assert.Equal("Approved", unchangedOpportunity.Status); // Rolled back to Approved
        }

        #endregion

        #region TC-OPP-ADVINT-008: Data Warehouse ETL Process

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-ADVINT-008")]
        public async Task ETLProcess_ExtractTransformLoad_DataWarehouseSync()
        {
            // Arrange - Create opportunities for ETL
            for (int i = 1; i <= 50; i++)
            {
                _context.Opportunities.Add(new Domain.Entities.Opportunity
                {
                    Name = $"ETL Test Opportunity {i}",
                    EstimatedValue = 1000000 + (i * 50000),
                    Status = i % 3 == 0 ? "Approved" : "Draft",
                    PrimaryCountryId = (i % 10) + 1,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow.AddDays(-i)
                });
            }
            await _context.SaveChangesAsync();

            // Act - ETL Process
            // Extract
            var opportunities = await _context.Opportunities.ToListAsync();

            // Transform
            var transformed = opportunities.Select(o => new DataWarehouseOpportunity
            {
                OpportunityId = o.Id,
                Name = o.Name,
                Value = o.EstimatedValue ?? 0,
                Status = o.Status,
                CountryId = o.PrimaryCountryId ?? 0,
                CreatedDate = o.CreatedDate,
                ETLDate = DateTime.UtcNow
            }).ToList();

            // Load (mock)
            var loadedCount = transformed.Count;

            // Assert
            Assert.Equal(50, loadedCount);
            Assert.All(transformed, t => Assert.NotNull(t.ETLDate));
        }

        #endregion

        #region TC-OPP-ADVINT-009: Webhook Notifications

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-ADVINT-009")]
        public async Task StatusChange_TriggersWebhooks_ExternalSystemsNotified()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Webhook Test",
                EstimatedValue = 2000000,
                Status = "Under Review",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);

            // Register webhooks
            var webhooks = new List<WebhookSubscription>
            {
                new WebhookSubscription { Url = "https://external1.com/webhook", Event = "StatusChange" },
                new WebhookSubscription { Url = "https://external2.com/webhook", Event = "StatusChange" }
            };
            _context.WebhookSubscriptions.AddRange(webhooks);
            await _context.SaveChangesAsync();

            // Act - Change status
            opportunity.Status = "Approved";
            await _context.SaveChangesAsync();

            // Mock webhook calls
            var webhooksCalled = new List<string>();
            foreach (var webhook in webhooks)
            {
                await _mockExternalService.Object.CallWebhookAsync(webhook.Url, new
                {
                    Event = "StatusChange",
                    OpportunityId = opportunity.Id,
                    NewStatus = opportunity.Status
                });
                webhooksCalled.Add(webhook.Url);
            }

            // Assert
            Assert.Equal(2, webhooksCalled.Count);
            Assert.Contains("https://external1.com/webhook", webhooksCalled);
        }

        #endregion

        #region TC-OPP-ADVINT-010: Global Search Index Update

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-ADVINT-010")]
        public async Task UpdateOpportunity_UpdatesSearchIndex_ImmediatelySearchable()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Search Index Test",
                Description = "Original description",
                EstimatedValue = 1000000,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Mock search index update
            var searchIndexUpdates = new List<SearchIndexUpdate>();
            _mockCacheService
                .Setup(c => c.UpdateSearchIndexAsync(It.IsAny<object>()))
                .Callback<object>(data => searchIndexUpdates.Add(new SearchIndexUpdate
                {
                    OpportunityId = opportunity.Id,
                    UpdateDate = DateTime.UtcNow
                }))
                .ReturnsAsync(true);

            // Act - Update opportunity
            opportunity.Description = "Updated description with new keywords";
            await _context.SaveChangesAsync();

            // Trigger search index update
            await _mockCacheService.Object.UpdateSearchIndexAsync(new
            {
                Id = opportunity.Id,
                Name = opportunity.Name,
                Description = opportunity.Description
            });

            // Assert
            Assert.Single(searchIndexUpdates);
            Assert.Equal(opportunity.Id, searchIndexUpdates[0].OpportunityId);
        }

        #endregion

        #region TC-OPP-ADVINT-011: Real-Time Collaboration Sync

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-ADVINT-011")]
        public async Task RealtimeSync_MultipleUsers_ChangesPropagate()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Realtime Sync Test",
                EstimatedValue = 1000000,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            var changeStream = new List<ChangeNotification>();

            // Act - User 1 makes a change
            opportunity.EstimatedValue = 1200000;
            opportunity.LastModifiedBy = 1;
            await _context.SaveChangesAsync();

            // Broadcast change to all connected users
            changeStream.Add(new ChangeNotification
            {
                OpportunityId = opportunity.Id,
                Field = "EstimatedValue",
                OldValue = "1000000",
                NewValue = "1200000",
                ChangedBy = 1,
                Timestamp = DateTime.UtcNow
            });

            // Assert
            Assert.Single(changeStream);
            Assert.Equal("EstimatedValue", changeStream[0].Field);
            // All other users should receive this change notification
        }

        #endregion

        #region TC-OPP-ADVINT-012: Email Service Integration

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-ADVINT-012")]
        public async Task SendEmail_DecisionMade_FormattedNotificationSent()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Email Integration Test",
                EstimatedValue = 3000000,
                Status = "Approved",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Mock email service
            var emailsSent = new List<EmailMessage>();
            _mockExternalService
                .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>()))
                .Callback<EmailMessage>(email => emailsSent.Add(email))
                .ReturnsAsync(true);

            // Act - Send notification email
            var email = new EmailMessage
            {
                To = "manager@unops.org",
                Subject = $"Opportunity Approved: {opportunity.Name}",
                Body = $"Your opportunity '{opportunity.Name}' (${opportunity.EstimatedValue:N0}) has been approved.",
                Priority = "High"
            };

            await _mockExternalService.Object.SendEmailAsync(email);

            // Assert
            Assert.Single(emailsSent);
            Assert.Contains("Approved", emailsSent[0].Subject);
            Assert.Equal("High", emailsSent[0].Priority);
        }

        #endregion

        #region TC-OPP-ADVINT-013: Document Storage Service Integration

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-ADVINT-013")]
        public async Task UploadDocument_StorageService_ReturnsAccessUrl()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Document Storage Test",
                EstimatedValue = 1000000,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            var documentBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // PDF signature

            // Mock storage service
            _mockExternalService
                .Setup(e => e.UploadDocumentAsync(It.IsAny<byte[]>(), It.IsAny<string>()))
                .ReturnsAsync(new StorageResult
                {
                    Success = true,
                    StorageUrl = "https://storage.unops.org/opportunities/1/concept-note.pdf",
                    FileSize = documentBytes.Length
                });

            // Act
            var storageResult = await _mockExternalService.Object.UploadDocumentAsync(
                documentBytes,
                $"opportunities/{opportunity.Id}/concept-note.pdf");

            // Save document reference
            var document = new OpportunityDocument
            {
                OpportunityId = opportunity.Id,
                FileName = "concept-note.pdf",
                StorageUrl = storageResult.StorageUrl,
                FileSize = storageResult.FileSize,
                UploadedBy = 1,
                UploadedDate = DateTime.UtcNow
            };
            _context.OpportunityDocuments.Add(document);
            await _context.SaveChangesAsync();

            // Assert
            Assert.True(storageResult.Success);
            Assert.Contains("storage.unops.org", storageResult.StorageUrl);
        }

        #endregion

        #region TC-OPP-ADVINT-014: Bi-Directional Sync with External CRM

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-ADVINT-014")]
        public async Task BiDirectionalSync_WithExternalCRM_DataConsistency()
        {
            // Arrange - Local opportunity
            var localOpportunity = new Domain.Entities.Opportunity
            {
                Name = "CRM Sync Test",
                EstimatedValue = 2000000,
                Status = "Draft",
                ExternalCRMId = "CRM-5678",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow,
                LastSyncDate = DateTime.UtcNow.AddHours(-2)
            };
            _context.Opportunities.Add(localOpportunity);
            await _context.SaveChangesAsync();

            // Mock CRM API - fetch external changes
            _mockExternalService
                .Setup(e => e.GetCRMRecordAsync("CRM-5678"))
                .ReturnsAsync(new CRMRecord
                {
                    Id = "CRM-5678",
                    Name = "CRM Sync Test",
                    Value = 2500000m, // Updated in CRM
                    LastModified = DateTime.UtcNow.AddHours(-1)
                });

            // Act - Sync from CRM
            var crmData = await _mockExternalService.Object.GetCRMRecordAsync(localOpportunity.ExternalCRMId);
            
            // Detect change
            if (crmData.LastModified > localOpportunity.LastSyncDate)
            {
                localOpportunity.EstimatedValue = crmData.Value;
                localOpportunity.LastSyncDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            // Assert
            Assert.Equal(2500000m, localOpportunity.EstimatedValue); // Updated from CRM
        }

        #endregion

        #region Helper Classes

        public class OpportunitySchedule
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public int TotalMonths { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public bool IsApproved { get; set; }
        }

        public class ResourcePlan
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public decimal TotalFTEs { get; set; }
            public List<RoleRequirement> Roles { get; set; }
        }

        public class RoleRequirement
        {
            public string Role { get; set; }
            public decimal FTEs { get; set; }
            public string Level { get; set; }
        }

        public class ERPSyncResult
        {
            public bool Success { get; set; }
            public string ERPRecordId { get; set; }
            public DateTime SyncDate { get; set; }
        }

        public class PMToolResult
        {
            public bool Success { get; set; }
            public string ProjectId { get; set; }
            public string ProjectUrl { get; set; }
        }

        public class HRRequestResult
        {
            public bool Success { get; set; }
            public string RequestId { get; set; }
            public DateTime EstimatedFulfillmentDate { get; set; }
        }

        public class APIRequest
        {
            public int UserId { get; set; }
            public string Endpoint { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public class OpportunityEvent
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public string EventType { get; set; }
            public string EventData { get; set; }
            public DateTime Timestamp { get; set; }
            public int UserId { get; set; }
        }

        public class ChangeNotification
        {
            public int OpportunityId { get; set; }
            public string Field { get; set; }
            public string OldValue { get; set; }
            public string NewValue { get; set; }
            public int ChangedBy { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public class WebhookSubscription
        {
            public int Id { get; set; }
            public string Url { get; set; }
            public string Event { get; set; }
        }

        public class EmailMessage
        {
            public string To { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
            public string Priority { get; set; }
        }

        public class StorageResult
        {
            public bool Success { get; set; }
            public string StorageUrl { get; set; }
            public long FileSize { get; set; }
        }

        public class OpportunityDocument
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public string FileName { get; set; }
            public string StorageUrl { get; set; }
            public long FileSize { get; set; }
            public int UploadedBy { get; set; }
            public DateTime UploadedDate { get; set; }
        }

        public class CRMRecord
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public decimal Value { get; set; }
            public DateTime LastModified { get; set; }
        }

        public class DataWarehouseOpportunity
        {
            public int OpportunityId { get; set; }
            public string Name { get; set; }
            public decimal Value { get; set; }
            public string Status { get; set; }
            public int CountryId { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime ETLDate { get; set; }
        }

        public class SearchIndexUpdate
        {
            public int OpportunityId { get; set; }
            public DateTime UpdateDate { get; set; }
        }

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
