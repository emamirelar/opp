using Google.Cloud.PubSub.V1;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
using System.Reflection;
using System.Linq.Expressions;
using UNOPS.PAO.Business.Interfaces;

namespace UNOPS.PAO.UNOPSBusiness.Services
{
    public class PubSubPullService : BackgroundService
    {
        private readonly ILogger<PubSubPullService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string ProjectId;
        private readonly string SubscriptionId;
        private readonly IDbContextFactory<UNOPSAppDbContext> _dbContextFactory;
        private readonly UNOPSManagerWrapper _managerWrapper;

        public PubSubPullService(ILogger<PubSubPullService> logger, IConfiguration configuration, IDbContextFactory<UNOPSAppDbContext> dbContextFactory, UNOPSManagerWrapper managerWrapper)
        {
            _logger = logger;
            _configuration = configuration;
            var pubSubProjectId = configuration.GetSection("PubSub")["ProjectId"] ?? string.Empty;
            // Fallback: use AppConfig:ProjectId when PubSub project is empty
            if (string.IsNullOrEmpty(pubSubProjectId))
            {
                ProjectId = configuration.GetSection("AppConfig")["ProjectId"] ?? string.Empty;
                if (!string.IsNullOrEmpty(ProjectId))
                    _logger.LogInformation("PubSub: Using AppConfig:ProjectId ({ProjectId}) - PubSub:ProjectId was not configured", ProjectId);
            }
            else
            {
                ProjectId = pubSubProjectId;
            }
            SubscriptionId = configuration.GetSection("PubSub")["SubscriptionId"] ?? string.Empty;
            _dbContextFactory = dbContextFactory;
            _managerWrapper = managerWrapper;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (string.IsNullOrEmpty(ProjectId) || string.IsNullOrEmpty(SubscriptionId))
            {
                _logger.LogWarning("PubSub Pull Service disabled: ProjectId or SubscriptionId is not configured. Set PubSub:ProjectId and PubSub:SubscriptionId in appsettings.");
                await Task.Delay(Timeout.Infinite, stoppingToken);
                return;
            }
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

        private BaseUNOPSManager GetUNOPSManagerByEntityName(string entityName)
        {
            // Special handling for entity names to ensure proper singularization
            string entityType = entityName.ToLower();
            
            // Handle special cases for proper singularization
            if (entityType == "opportunities")
            {
                entityType = "opportunity";
            }
            else if (entityType.EndsWith("ies"))
            {
                // Words ending in 'ies' -> change to 'y' (e.g., "entities" -> "entity")
                entityType = entityType.Substring(0, entityType.Length - 3) + "y";
            }
            else if (entityType.EndsWith("s") && !entityType.EndsWith("ss"))
            {
                // Remove trailing 's' for regular plurals (but not words ending in 'ss')
                entityType = entityType.Substring(0, entityType.Length - 1);
            }
            
            var fieldName = $"{entityType}Manager";
            
            _logger.LogDebug($"Looking for manager field: {fieldName} (from entity name: {entityName})");
            
            // Get the private field from UNOPSManagerWrapper that contains the actual UNOPS manager instance
            var field = _managerWrapper.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (field == null)
            {
                _logger.LogError($"Manager field not found: {fieldName}. Available fields: {string.Join(", ", _managerWrapper.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Select(f => f.Name))}");
                throw new ArgumentException($"Manager field not found: {fieldName} (from entity name: {entityName})");
            }
            
            var manager = field?.GetValue(_managerWrapper) as BaseUNOPSManager;
            
            return manager ?? throw new ArgumentException($"Manager not found or doesn't inherit from BaseUNOPSManager: {fieldName}");
        }

        private async Task<string> ConvertEntityDataToReadableStringAsync(object entityData, UNOPSAppDbContext dbContext)
        {
            if (entityData == null) return string.Empty;

            try
            {
                var properties = entityData.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var readableLines = new List<string>();

                foreach (var property in properties)
                {
                    try
                    {
                        // Skip properties that might cause circular references or complex navigation
                        if (property.PropertyType.IsClass && 
                            property.PropertyType != typeof(string) && 
                            !property.PropertyType.IsValueType &&
                            !property.PropertyType.IsEnum)
                        {
                            continue; // Skip complex objects to avoid circular references
                        }

                        var value = property.GetValue(entityData);
                        
                        // Skip null values, empty collections, and complex navigation properties
                        if (value == null) continue;
                        
                        // Handle different value types with enhanced foreign key resolution
                        string formattedValue = await ResolvePropertyValueAsync(property, value, dbContext);

                        // Add to readable format if we have a meaningful value
                        if (!string.IsNullOrWhiteSpace(formattedValue))
                        {
                            // Convert property name from PascalCase to readable format
                            var readablePropertyName = Regex.Replace(property.Name, "([a-z])([A-Z])", "$1 $2");
                            readableLines.Add($"{readablePropertyName}: {formattedValue}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Error processing property {property.Name}: {ex.Message}");
                        // Continue processing other properties
                    }
                }

                return string.Join("\n", readableLines);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ConvertEntityDataToReadableStringAsync: {ex.Message}");
                return string.Empty;
            }
        }

        private async Task<string> ResolvePropertyValueAsync(PropertyInfo property, object value, UNOPSAppDbContext dbContext)
        {
            try
            {
                // Handle collections (arrays, lists) - CRITICAL for embeddings
                if (value is System.Collections.IEnumerable enumerable && value is not string)
                {
                    var items = new List<string>();
                    foreach (var item in enumerable)
                    {
                        if (item == null) continue;
                        
                        // For complex objects (like OpportunityDeliverableModel, OpportunityCountryModel, etc.)
                        if (item.GetType().IsClass && item.GetType() != typeof(string))
                        {
                            // Try to get the most meaningful property (Name, Title, Description)
                            var nameProperty = item.GetType().GetProperty("Name") 
                                ?? item.GetType().GetProperty("Title")
                                ?? item.GetType().GetProperty("Description");
                            
                            if (nameProperty != null)
                            {
                                var nameValue = nameProperty.GetValue(item);
                                if (nameValue != null && !string.IsNullOrWhiteSpace(nameValue.ToString()))
                                {
                                    items.Add(nameValue.ToString());
                                }
                            }
                            else
                            {
                                // For objects without Name/Title/Description, use ToString()
                                var stringValue = item.ToString();
                                if (!stringValue.StartsWith(item.GetType().FullName)) // Skip default ToString() output
                                {
                                    items.Add(stringValue);
                                }
                            }
                        }
                        else
                        {
                            // For primitive types in collections
                            items.Add(item.ToString());
                        }
                    }
                    
                    // Return comma-separated list if we have items
                    return items.Count > 0 ? string.Join(", ", items) : null;
                }
                
                // Handle different value types
                string formattedValue = value switch
                {
                    string str when string.IsNullOrWhiteSpace(str) => null, // Skip empty strings
                    string str => str,
                    DateTime dateTime when dateTime == DateTime.MinValue => null, // Skip default dates
                    DateTime dateTime => dateTime.ToString("yyyy-MM-dd"),
                    bool boolean => boolean.ToString(),
                    int number when number == 0 => null, // Skip zero values
                    int number => await ResolveIdToNameAsync(property.Name, number, dbContext) ?? number.ToString(),
                    decimal dec when dec == 0 => null, // Skip zero values  
                    decimal dec => dec.ToString("0.##"),
                    Enum enumValue => enumValue.ToString(),
                    // Skip complex objects (but collections are already handled above)
                    _ when value.GetType().IsClass && value.GetType() != typeof(string) => null,
                    _ => value.ToString()
                };

                return formattedValue;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error resolving property {property.Name}: {ex.Message}");
                return value?.ToString();
            }
        }

        private async Task<string> ResolveIdToNameAsync(string propertyName, int id, UNOPSAppDbContext dbContext)
        {
            try
            {
                _logger.LogDebug($"Attempting to resolve {propertyName} ID {id}");
                
                // Use the enhanced AiContextualService method that leverages the same mapping strategy
                // as GetEntityIdFromText but in reverse
                var contextService = new AiContextualService(_configuration, dbContext, null);
                var lookupResult = await contextService.GetEntityNameFromId(id, propertyName);

                _logger.LogDebug($"Lookup result for {propertyName} ID {id}: '{lookupResult}'");

                // If we found a name, return "Name (ID)" format for better context
                if (!string.IsNullOrEmpty(lookupResult))
                {
                    var result = $"{lookupResult} (ID: {id})";
                    _logger.LogDebug($"Returning resolved name: {result}");
                    return result;
                }

                _logger.LogDebug($"No name found for {propertyName} ID {id}, returning null");
                return null; // Return null so the original ID will be used
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error resolving {propertyName} ID {id}: {ex.Message}");
                _logger.LogWarning($"Exception details: {ex}");
                return null;
            }
        }

        private async Task ProcessEntityMessage(MyPubSubMessage msg, UNOPSAppDbContext dbContext, AiContextualService contextService)
        {
            if (!msg.EntityId.HasValue)
            {
                _logger.LogWarning("EntityId is required for entity processing");
                return;
            }

            try
            {
                // Get the appropriate UNOPS manager for this entity type
                var manager = GetUNOPSManagerByEntityName(msg.EntityName);
                
                _logger.LogDebug($"Processing entity {msg.EntityName} with ID {msg.EntityId.Value} using manager {manager.GetType().Name}");
                
                // Call GetBasicEntityDataAsync to get the entity data
                var entityData = await manager.GetBasicEntityDataAsync(msg.EntityId.Value);
                
                if (entityData != null)
                {
                    _logger.LogDebug($"Successfully retrieved entity data for {msg.EntityName} with ID {msg.EntityId.Value}");
                    
                    // Convert entity data to human-readable format with resolved names for better embeddings
                    var readableContent = await ConvertEntityDataToReadableStringAsync(entityData, dbContext);
                    
                    if (!string.IsNullOrWhiteSpace(readableContent))
                    {
                        // Generate embedding with human-readable content including resolved names
                        await contextService.GenerateEmbeddingAsync(msg.EntityName, msg.EntityId.Value, readableContent);
                        
                        _logger.LogInformation($"Generated embedding for {msg.EntityName} with ID {msg.EntityId.Value}");
                        _logger.LogDebug($"Enhanced embedding content with resolved names: {readableContent}");
                        
                        // Add a delay of 1 second after each embedding generation
                        await Task.Delay(1000); // 1 second delay
                    }
                    else
                    {
                        _logger.LogWarning($"No meaningful content found for {msg.EntityName} with ID {msg.EntityId.Value}");
                    }
                }
                else
                {
                    _logger.LogWarning($"Entity data not found for {msg.EntityName} with ID {msg.EntityId.Value}");
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"No manager available for entity {msg.EntityName}: {ex.Message}");
            }
            catch (NullReferenceException ex)
            {
                _logger.LogError($"Null reference exception processing entity {msg.EntityName} with ID {msg.EntityId.Value}: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing entity {msg.EntityName} with ID {msg.EntityId.Value}: {ex.Message}");
                _logger.LogError($"Exception type: {ex.GetType().Name}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
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
                    Message = "Analyzing file... 0% complete",
                    Category = promptData?.Type ?? "BulkImport",
                    ResponseType = "Progress",
                    RecordData = JsonConvert.SerializeObject(new List<object> { msg.BatchData }),
                    IsRead = false,
                    Status = NotificationStatus.Progress,
                    CreatedAt = DateTime.UtcNow
                };

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
                    
                    return true; // Continue processing
                }
                
                // Call the processing method with progress tracking
                // Pass the notification ID so it can be updated instead of creating a new notification
                var results = await contextService.ProcessBulkImportWithProgress(
                    msg.BatchData,
                    promptData,
                    msg.UserId,
                    msg.EntityName,
                    true, 
                    progressCallback,
                    msg.FileId, // Pass the Google Sheet ID for identification
                    notification.Id // Pass notification ID to update the same notification
                );
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
