using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Business;

namespace UNOPS.PAO.UNOPSBusiness.Services;

public class DueDiligenceNotificationService : BackgroundService
{
    private readonly ILogger<DueDiligenceNotificationService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly int _checkIntervalHours;
    private readonly int _warningMonths;
    private readonly bool _enabled;
    private readonly bool _testMode;

    public DueDiligenceNotificationService(
        ILogger<DueDiligenceNotificationService> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;

        var notificationSettings = configuration.GetSection("DueDiligenceNotifications");
        _checkIntervalHours = int.Parse(notificationSettings["CheckIntervalHours"] ?? "24");
        _warningMonths = int.Parse(notificationSettings["WarningMonths"] ?? "6");
        _enabled = bool.Parse(notificationSettings["Enabled"] ?? "true");
        _testMode = bool.Parse(notificationSettings["TestMode"] ?? "false");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_enabled)
        {
            _logger.LogInformation("Due Diligence Notification Service is disabled in configuration");
            return;
        }

        _logger.LogInformation("Due Diligence Notification Service started. Check interval: {CheckIntervalHours} hours, Warning period: {WarningMonths} months", 
            _checkIntervalHours, _warningMonths);

        // Run immediately on startup, then on interval
        await CheckAndSendNotifications();

        using var timer = new PeriodicTimer(TimeSpan.FromHours(_checkIntervalHours));
        
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            if (stoppingToken.IsCancellationRequested)
                break;
                
            await CheckAndSendNotifications();
        }
    }

    private async Task CheckAndSendNotifications()
    {
        try
        {
            _logger.LogInformation("Starting due diligence expiry check...");

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();
            var paoEmailSender = scope.ServiceProvider.GetRequiredService<PAOEmailSender>();
            var urlService = scope.ServiceProvider.GetRequiredService<IUrlService>();
            var notificationManager = scope.ServiceProvider.GetRequiredService<NotificationManager>();

            // Calculate the warning threshold date (6 months from now)
            var warningThresholdDate = DateTime.UtcNow.AddMonths(_warningMonths);

            // Find partners with expiring due diligence
            var expiringPartners = await context.Partners
                .Include(p => p.PartnerFocalPointUser) // Assuming navigation property exists
                .Where(p => p.DueDiligenceExpiryDate.HasValue &&
                           p.DueDiligenceExpiryDate.Value <= warningThresholdDate &&
                           p.DueDiligenceExpiryDate.Value > DateTime.UtcNow &&
                           p.PartnerFocalPointUserId.HasValue &&
                           !p.IsDeleted) // Assuming soft delete pattern
                .ToListAsync();

            if (!expiringPartners.Any())
            {
                _logger.LogInformation("No partners found with expiring due diligence");
                return;
            }

            _logger.LogInformation("Found {Count} partners with expiring due diligence", expiringPartners.Count);

            var successCount = 0;
            var failureCount = 0;

            foreach (var partner in expiringPartners)
            {
                try
                {
                    // Get user email from userprofile table
                    var userprofile = await context.UserProfile
                        .FirstOrDefaultAsync(ui => ui.UserId == partner.PartnerFocalPointUserId!.Value);

                    if (userprofile?.UserEmail == null)
                    {
                        _logger.LogWarning("No email found for user ID {UserId} (Partner: {PartnerName})", 
                            partner.PartnerFocalPointUserId!.Value, partner.Name);
                        failureCount++;
                        continue;
                    }

                    // Check if we already sent a notification for this specific expiry date
                    if (!_testMode && await HasNotificationForSameExpiryDate(context, partner.Id, userprofile.UserId, partner.DueDiligenceExpiryDate.Value))
                    {
                        _logger.LogDebug("Skipping notification for partner {PartnerName} - notification already sent for expiry date {ExpiryDate}", 
                            partner.Name, partner.DueDiligenceExpiryDate!.Value.ToString("yyyy-MM-dd"));
                        continue;
                    }

                    var monthsUntilExpiry = Math.Round((partner.DueDiligenceExpiryDate!.Value - DateTime.UtcNow).TotalDays / 30.44, 1);
                    var daysRemaining = (int)(partner.DueDiligenceExpiryDate!.Value - DateTime.UtcNow).TotalDays;
                    var partnerUrl = urlService.BuildEntityUrl("partner", partner.Id);

                    await paoEmailSender.SendDueDiligenceExpiryNotificationAsync(
                        userprofile.UserEmail,
                        userprofile.Name ?? "User",
                        partner.Name ?? "Unknown Partner",
                        partner.DueDiligenceExpiryDate!.Value,
                        partnerUrl,
                        (decimal)monthsUntilExpiry,
                        daysRemaining);

                    // Email sending succeeded (PAOEmailSender doesn't return bool, so we assume success if no exception)
                    var emailSent = true;

                    if (emailSent)
                    {
                        // Create in-app notification as well
                        await notificationManager.CreateNotification(
                            userprofile.UserId,
                            $"Due diligence for partner '{partner.Name}' expires on {partner.DueDiligenceExpiryDate.Value:MMM dd, yyyy}",
                            "DueDiligenceExpiry",
                            "Partner",
                            new { PartnerId = partner.Id, PartnerName = partner.Name, ExpiryDate = partner.DueDiligenceExpiryDate.Value });

                        // Record that we sent the notification
                        await RecordNotificationSent(context, partner.Id, userprofile.UserId, partner.DueDiligenceExpiryDate.Value);

                        successCount++;
                        _logger.LogInformation("Notification sent for partner {PartnerName} to {UserEmail}", 
                            partner.Name, userprofile.UserEmail);
                    }
                    else
                    {
                        failureCount++;
                        _logger.LogError("Failed to send email notification for partner {PartnerName} to {UserEmail}", 
                            partner.Name, userprofile.UserEmail);
                    }
                }
                catch (Exception ex)
                {
                    failureCount++;
                    _logger.LogError(ex, "Error processing notification for partner {PartnerName}", partner.Name);
                }
            }

            _logger.LogInformation("Due diligence notification check completed. Success: {SuccessCount}, Failed: {FailureCount}", 
                successCount, failureCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during due diligence notification check");
        }
    }

    private async Task<bool> HasNotificationForSameExpiryDate(UNOPSAppDbContext context, int partnerId, int userId, DateTime expiryDate)
    {
        // Check if we already sent a notification for this specific expiry date
        var existingNotification = await context.EmailNotificationLogs
            .AnyAsync(log => log.RelatedEntityId == partnerId &&
                           log.RecipientUserId == userId &&
                           log.RelatedEntityType == "Partner" &&
                           log.NotificationType == "DueDiligenceExpiry" &&
                           log.NotificationData != null &&
                           log.NotificationData.Contains($"\"{expiryDate.Date:yyyy-MM-dd}\""));
                           
        return existingNotification;
    }

    private async Task RecordNotificationSent(UNOPSAppDbContext context, int partnerId, int userId, DateTime dueDiligenceExpiryDate)
    {
        // Get user info for the log
        var userprofile = await context.UserProfile.FirstOrDefaultAsync(ui => ui.UserId == userId);
        var partner = await context.Partners.FirstOrDefaultAsync(p => p.Id == partnerId);

        var notificationData = System.Text.Json.JsonSerializer.Serialize(new
        {
            DueDiligenceExpiryDate = dueDiligenceExpiryDate.ToString("yyyy-MM-dd"),
            PartnerId = partnerId,
            PartnerName = partner?.Name,
            MonthsUntilExpiry = Math.Round((dueDiligenceExpiryDate - DateTime.UtcNow).TotalDays / 30.44, 1),
            DaysRemaining = (int)(dueDiligenceExpiryDate - DateTime.UtcNow).TotalDays
        });

        var log = new Domain.Entities.EmailNotificationLog
        {
            RecipientUserId = userId,
            RecipientEmail = userprofile?.UserEmail,
            RecipientName = userprofile?.Name,
            EmailSubject = $"Due Diligence Expiry Warning - {partner?.Name}",
            NotificationType = "DueDiligenceExpiry",
            SentAt = DateTime.UtcNow,
            RelatedEntityId = partnerId,
            RelatedEntityType = "Partner",
            RelatedEntityName = partner?.Name,
            NotificationData = notificationData,
            IsSuccessful = true
        };

        context.EmailNotificationLogs.Add(log);
        await context.SaveChangesAsync();
    }
}
