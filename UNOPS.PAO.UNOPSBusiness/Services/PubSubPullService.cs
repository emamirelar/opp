using Google.Cloud.PubSub.V1;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UNOPS.PAO.UNOPSDataAccess.Context;
using System.Text.Json;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSBusiness.Models;
using System.Linq;
using UNOPS.PAO.Models;
using Newtonsoft.Json;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using System.Globalization;
using System.Text.RegularExpressions;
using Humanizer;
using UNOPS.PAO.UNOPSBusiness.Interfaces;

namespace UNOPS.PAO.UNOPSBusiness.Services
{
    public class PubSubPullService : BackgroundService
    {
        private readonly ILogger<PubSubPullService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string ProjectId;
        private readonly string SubscriptionId;
        private readonly IDbContextFactory<UNOPSAppDbContext> _dbContextFactory;

        public PubSubPullService(ILogger<PubSubPullService> logger, IConfiguration configuration, IDbContextFactory<UNOPSAppDbContext> dbContextFactory)
        {
            _logger = logger;
            _configuration = configuration;
            ProjectId = configuration.GetSection("PubSub")["ProjectId"];
            SubscriptionId = configuration.GetSection("PubSub")["SubscriptionId"];
            _dbContextFactory = dbContextFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var subscriptionName = SubscriptionName.FromProjectSubscription(ProjectId, SubscriptionId);
            var subscriber = await SubscriberClient.CreateAsync(subscriptionName);

            _logger.LogInformation("Pub/Sub Pull Service started. Listening for messages...");

            // Start receiving messages
            await subscriber.StartAsync((PubsubMessage message, CancellationToken ct) =>
            {
                return Task.Run(async () =>
                {
                    if (ct.IsCancellationRequested)
                    {
                        return SubscriberClient.Reply.Nack;
                    }

                    try
                    {
                        // Convert the message data from byte array to string
                        string messageText = System.Text.Encoding.UTF8.GetString(message.Data.ToArray());

                        List<MyPubSubMessage>? messages = System.Text.Json.JsonSerializer.Deserialize<List<MyPubSubMessage>>(messageText);

                        if (messages != null)
                        {
                            foreach (var msg in messages)
                            {
                                // Use the factory to create a new DbContext instance
                                using (var dbContext = _dbContextFactory.CreateDbContext())
                                {
                                    var contextService = new AiContextualService(_configuration, dbContext, null);
                                    
                                    switch (msg.MessageType)
                                    {
                                        case "EntityProcessing":
                                            await ProcessEntityMessage(msg, dbContext, contextService);
                                            break;
                                        case "BulkImport":
                                            await ProcessBulkImportMessage(msg, dbContext, contextService);
                                            break;
                                        default:
                                            _logger.LogWarning($"Unknown message type: {msg.MessageType}");
                                            break;
                                    }
                                }
                            }
                        }

                        return SubscriberClient.Reply.Ack;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing message: {ex.Message}");
                        return SubscriberClient.Reply.Ack;
                    }
                }, ct);
            });

            // Keep the service running until cancellation is requested
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task ProcessEntityMessage(MyPubSubMessage msg, UNOPSAppDbContext dbContext, AiContextualService contextService)
        {
            if (!msg.EntityId.HasValue)
            {
                _logger.LogWarning("EntityId is required for entity processing");
                return;
            }

            // Get the entity type and fetch the content
            var entityType = dbContext.Model.GetEntityTypes()
                .FirstOrDefault(e => e.GetTableName().Equals(msg.EntityName, StringComparison.OrdinalIgnoreCase));
            
            if (entityType != null)
            {
                var dbSetProperty = dbContext.GetType()
                    .GetProperty(msg.EntityName);
                    
                if (dbSetProperty != null)
                {
                    var dbSet = dbSetProperty.GetValue(dbContext) as IQueryable<object>;
                    if (dbSet != null)
                    {
                        var entities = await dbSet.ToListAsync();
                        var entity = entities.Where(e => 
                        {
                            var idProperty = e.GetType().GetProperty("Id");
                            return idProperty != null && (int)idProperty.GetValue(e) == msg.EntityId.Value;
                        }).FirstOrDefault();
                        
                        if (entity != null)
                        {
                            // Serialize the entity to JSON
                            var content = JsonConvert.SerializeObject(entity, Formatting.Indented);
                            
                            // Get the prompt data for summarization
                            var promptData = (await contextService.GetPromptData("summarize_information")).FirstOrDefault();
                            if (promptData != null)
                            {
                                // Summarize the content using Gemini
                                string response = await contextService.FetchResultFromGemini((AiPromptModel)promptData, content);
                                var responseMessage = contextService.GetDetailsFromGeminiResponse(response)["Message"]?.ToString() ?? string.Empty;
                                
                                // Generate embedding with the summarized content
                                await contextService.GenerateEmbeddingAsync(msg.EntityName, msg.EntityId.Value, responseMessage);
                                // Add a delay of 1 second after each embedding generation
                                await Task.Delay(1000); // 1 second delay
                            }
                        }
                    }
                }
            }
        }

        private async Task ProcessBulkImportMessage(MyPubSubMessage msg, UNOPSAppDbContext dbContext, AiContextualService contextService)
        {
            if (msg.BatchData == null || !msg.BatchData.Any())
            {
                _logger.LogWarning("BatchData is required for bulk import processing");
                return;
            }

            var promptData = (await contextService.GetPromptData(msg.PromptType)).FirstOrDefault();

            // Create an initial notification
            Notification notification = new Notification
            {
                UserId = msg.UserId,
                Message = "Starting import process... 0% complete",
                Category = promptData?.Type ?? "BulkImport",
                ResponseType = "Progress",
                RecordData = JsonConvert.SerializeObject(new List<object> { msg.BatchData }),
                IsRead = false,
                Status = NotificationStatus.Progress,
                CreatedAt = DateTime.UtcNow
            }; ;

            try
            {   
                await dbContext.Notifications.AddAsync(notification);
                await dbContext.SaveChangesAsync();
                
                // Parse the batch data to calculate total size
                string unescapedJson = msg.BatchData.Replace("\\\"", "\"").Trim('"');
                var batchData = JsonConvert.DeserializeObject<List<object>>(unescapedJson);
                int totalRecords = batchData?.Count ?? 0;
                int processedRecords = 0;
                int lastProgressPercentage = 0;
                
                // Store notification ID for error handling
                int notificationId = notification.Id;
                
                // Create a progress tracking callback
                async Task<bool> progressCallback(int currentBatch, int totalItems, List<dynamic> currentResults)
                {
                    processedRecords = currentBatch;
                    int progressPercentage = totalRecords > 0 ? (int)((processedRecords * 100.0) / totalRecords) : 0;
                    
                    // Only update every 5% to avoid too many database writes
                    if (progressPercentage >= lastProgressPercentage + 5 || progressPercentage == 100)
                    {
                        lastProgressPercentage = progressPercentage;
                        
                        // Create a progress message with a visual indicator
                        string progressBar = "[" + new string('■', progressPercentage / 5) + new string('□', 20 - (progressPercentage / 5)) + "]";
                        string progressMessage = $"Processing import... {progressPercentage}% complete {progressBar}";
                        
                        // Update the notification with progress
                        notification.Message = progressMessage;
                        notification.Status = progressPercentage < 100 ? NotificationStatus.Progress : NotificationStatus.Done;
                        
                        // If we're at 100%, store the results
                        if (progressPercentage == 100 && currentResults != null)
                        {
                            notification.RecordData = JsonConvert.SerializeObject(currentResults);
                            notification.Message = $"Import complete! {totalItems} records processed and ready to import.";
                        }
                        
                        await dbContext.SaveChangesAsync();
                    }
                    
                    return true; // Continue processing
                }
                
                // Call the processing method with progress tracking
                var results = await contextService.ProcessBulkImportWithProgress(
                    msg.BatchData,
                    promptData,
                    msg.UserId,
                    msg.EntityName,
                    true, 
                    progressCallback
                );
                
                // Final update if it wasn't already updated at 100%
                if (lastProgressPercentage < 100 && results != null && results.Count > 0)
                {
                    notification.Status = NotificationStatus.Done;
                    notification.Message = $"Import complete! {results.Count} records are ready to import.";
                    notification.RecordData = JsonConvert.SerializeObject(results);
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing bulk import: {ex.Message}");
                var category = promptData?.Type ?? "BulkImport";

                if (notification == null)
                {
                    // Fallback to creating a new notification if we can't find the original one
                    var errorNotification = new Notification
                    {
                        UserId = msg.UserId,
                        Message = $"Error processing bulk import: {ex.Message}",
                        Category = category,
                        ResponseType = "Error",
                        RecordData = JsonConvert.SerializeObject(new List<object> { msg.BatchData }),
                        IsRead = false,
                        Status = NotificationStatus.Done,
                        CreatedAt = DateTime.UtcNow
                    };

                    await dbContext.Notifications.AddAsync(errorNotification);
                    await dbContext.SaveChangesAsync();
                }
                else
                {
                    // Update the existing notification with error information
                    notification.Message = $"Error processing bulk import: {ex.Message}";
                    notification.ResponseType = "Error";
                    notification.Status = NotificationStatus.Done;

                    await dbContext.SaveChangesAsync();
                }
            }
        }
    }
}
