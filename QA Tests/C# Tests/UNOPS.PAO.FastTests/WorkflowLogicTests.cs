/**
 * @fileoverview Fast standalone tests for Workflow business logic
 * @author UNOPS Opportunity+ System Development Team
 */

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.FastTests;

/// <summary>
/// Tests for Workflow state transition logic
/// </summary>
public class WorkflowLogicTests
{
    public enum WorkflowStatus
    {
        Draft = 0,
        PendingReview = 1,
        UnderReview = 2,
        Approved = 3,
        Rejected = 4,
        Cancelled = 5
    }

    /// <summary>
    /// Validates if a workflow transition is allowed
    /// </summary>
    private static bool IsTransitionAllowed(WorkflowStatus from, WorkflowStatus to)
    {
        return (from, to) switch
        {
            (WorkflowStatus.Draft, WorkflowStatus.PendingReview) => true,
            (WorkflowStatus.Draft, WorkflowStatus.Cancelled) => true,
            (WorkflowStatus.PendingReview, WorkflowStatus.UnderReview) => true,
            (WorkflowStatus.PendingReview, WorkflowStatus.Cancelled) => true,
            (WorkflowStatus.UnderReview, WorkflowStatus.Approved) => true,
            (WorkflowStatus.UnderReview, WorkflowStatus.Rejected) => true,
            (WorkflowStatus.Rejected, WorkflowStatus.Draft) => true, // Allow resubmission
            _ => false
        };
    }

    [Theory]
    [InlineData(WorkflowStatus.Draft, WorkflowStatus.PendingReview, true)]
    [InlineData(WorkflowStatus.PendingReview, WorkflowStatus.UnderReview, true)]
    [InlineData(WorkflowStatus.UnderReview, WorkflowStatus.Approved, true)]
    [InlineData(WorkflowStatus.UnderReview, WorkflowStatus.Rejected, true)]
    public void IsTransitionAllowed_ValidTransitions_ReturnsTrue(
        WorkflowStatus from, WorkflowStatus to, bool expected)
    {
        // Act
        var result = IsTransitionAllowed(from, to);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(WorkflowStatus.Draft, WorkflowStatus.Approved)]
    [InlineData(WorkflowStatus.Approved, WorkflowStatus.Draft)]
    [InlineData(WorkflowStatus.Cancelled, WorkflowStatus.Approved)]
    [InlineData(WorkflowStatus.PendingReview, WorkflowStatus.Approved)]
    public void IsTransitionAllowed_InvalidTransitions_ReturnsFalse(
        WorkflowStatus from, WorkflowStatus to)
    {
        // Act
        var result = IsTransitionAllowed(from, to);

        // Assert
        result.Should().BeFalse(
            $"transition from {from} to {to} should not be allowed");
    }

    [Fact]
    public void IsTransitionAllowed_CannotSkipReviewProcess()
    {
        // Act & Assert - Cannot go directly from Draft to Approved
        IsTransitionAllowed(WorkflowStatus.Draft, WorkflowStatus.Approved)
            .Should().BeFalse("must go through review process");
    }

    [Fact]
    public void IsTransitionAllowed_RejectedCanResubmit()
    {
        // Act & Assert - Rejected items can go back to Draft
        IsTransitionAllowed(WorkflowStatus.Rejected, WorkflowStatus.Draft)
            .Should().BeTrue("rejected items should be able to be resubmitted");
    }

    [Fact]
    public void IsTransitionAllowed_CancelledIsFinalState()
    {
        // Act & Assert - Cannot transition out of Cancelled
        var possibleTransitions = Enum.GetValues<WorkflowStatus>();
        foreach (var to in possibleTransitions)
        {
            if (to != WorkflowStatus.Cancelled)
            {
                IsTransitionAllowed(WorkflowStatus.Cancelled, to)
                    .Should().BeFalse($"Cancelled is final, cannot transition to {to}");
            }
        }
    }
}

/// <summary>
/// Tests for Notification logic
/// </summary>
public class NotificationLogicTests
{
    public enum NotificationType
    {
        Info,
        Warning,
        Error,
        Success,
        TaskAssignment,
        WorkflowUpdate
    }

    public record NotificationConfig(
        NotificationType Type,
        bool SendEmail,
        bool SendInApp,
        int? ExpiryDays);

    /// <summary>
    /// Gets default notification configuration based on type
    /// </summary>
    private static NotificationConfig GetDefaultConfig(NotificationType type)
    {
        return type switch
        {
            NotificationType.Error => new NotificationConfig(type, true, true, 30),
            NotificationType.TaskAssignment => new NotificationConfig(type, true, true, 7),
            NotificationType.WorkflowUpdate => new NotificationConfig(type, true, true, 14),
            NotificationType.Success => new NotificationConfig(type, false, true, 3),
            NotificationType.Info => new NotificationConfig(type, false, true, 7),
            NotificationType.Warning => new NotificationConfig(type, true, true, 14),
            _ => new NotificationConfig(type, false, true, 7)
        };
    }

    [Theory]
    [InlineData(NotificationType.Error)]
    [InlineData(NotificationType.TaskAssignment)]
    [InlineData(NotificationType.WorkflowUpdate)]
    [InlineData(NotificationType.Warning)]
    public void GetDefaultConfig_ImportantNotifications_SendEmail(NotificationType type)
    {
        // Act
        var config = GetDefaultConfig(type);

        // Assert
        config.SendEmail.Should().BeTrue(
            $"{type} notifications should send email");
    }

    [Theory]
    [InlineData(NotificationType.Success)]
    [InlineData(NotificationType.Info)]
    public void GetDefaultConfig_InfoNotifications_DoNotSendEmail(NotificationType type)
    {
        // Act
        var config = GetDefaultConfig(type);

        // Assert
        config.SendEmail.Should().BeFalse(
            $"{type} notifications should not send email by default");
    }

    [Fact]
    public void GetDefaultConfig_AllTypes_SendInApp()
    {
        // Act & Assert - All notification types should show in-app
        foreach (var type in Enum.GetValues<NotificationType>())
        {
            var config = GetDefaultConfig(type);
            config.SendInApp.Should().BeTrue(
                $"{type} should always show in-app notification");
        }
    }

    [Fact]
    public void GetDefaultConfig_ErrorNotifications_HaveLongerExpiry()
    {
        // Act
        var errorConfig = GetDefaultConfig(NotificationType.Error);
        var infoConfig = GetDefaultConfig(NotificationType.Info);

        // Assert
        errorConfig.ExpiryDays.Should().BeGreaterThan(infoConfig.ExpiryDays ?? 0,
            "error notifications should be retained longer than info");
    }
}

/// <summary>
/// Tests for Document validation logic
/// </summary>
public class DocumentValidationTests
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".txt", ".csv", ".jpg", ".jpeg", ".png", ".gif"
    };

    private const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50MB

    private static (bool IsValid, string? Error) ValidateDocument(string fileName, long fileSize)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return (false, "File name is required");

        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(extension))
            return (false, "File must have an extension");

        if (!AllowedExtensions.Contains(extension))
            return (false, $"File type '{extension}' is not allowed");

        if (fileSize <= 0)
            return (false, "File size must be greater than 0");

        if (fileSize > MaxFileSizeBytes)
            return (false, $"File size exceeds maximum of {MaxFileSizeBytes / 1024 / 1024}MB");

        return (true, null);
    }

    [Theory]
    [InlineData("document.pdf")]
    [InlineData("spreadsheet.xlsx")]
    [InlineData("presentation.pptx")]
    [InlineData("image.jpg")]
    [InlineData("IMAGE.PNG")]
    public void ValidateDocument_ValidFiles_ReturnsValid(string fileName)
    {
        // Arrange
        long fileSize = 1024 * 1024; // 1MB

        // Act
        var (isValid, error) = ValidateDocument(fileName, fileSize);

        // Assert
        isValid.Should().BeTrue();
        error.Should().BeNull();
    }

    [Theory]
    [InlineData("script.exe")]
    [InlineData("malware.bat")]
    [InlineData("hack.js")]
    [InlineData("payload.php")]
    public void ValidateDocument_DangerousExtensions_ReturnsInvalid(string fileName)
    {
        // Arrange
        long fileSize = 1024; // 1KB

        // Act
        var (isValid, error) = ValidateDocument(fileName, fileSize);

        // Assert
        isValid.Should().BeFalse();
        error.Should().Contain("not allowed");
    }

    [Fact]
    public void ValidateDocument_FileTooLarge_ReturnsInvalid()
    {
        // Arrange
        string fileName = "large.pdf";
        long fileSize = 100 * 1024 * 1024; // 100MB

        // Act
        var (isValid, error) = ValidateDocument(fileName, fileSize);

        // Assert
        isValid.Should().BeFalse();
        error.Should().Contain("exceeds maximum");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ValidateDocument_EmptyFileName_ReturnsInvalid(string? fileName)
    {
        // Act
        var (isValid, error) = ValidateDocument(fileName!, 1024);

        // Assert
        isValid.Should().BeFalse();
        error.Should().Contain("required");
    }

    [Fact]
    public void ValidateDocument_NoExtension_ReturnsInvalid()
    {
        // Act
        var (isValid, error) = ValidateDocument("filename", 1024);

        // Assert
        isValid.Should().BeFalse();
        error.Should().Contain("extension");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ValidateDocument_InvalidFileSize_ReturnsInvalid(long fileSize)
    {
        // Act
        var (isValid, error) = ValidateDocument("test.pdf", fileSize);

        // Assert
        isValid.Should().BeFalse();
        error.Should().Contain("size");
    }
}

/// <summary>
/// Tests for User permission logic
/// </summary>
public class PermissionLogicTests
{
    [Flags]
    public enum Permissions
    {
        None = 0,
        Read = 1,
        Write = 2,
        Delete = 4,
        Admin = 8,
        All = Read | Write | Delete | Admin
    }

    private static bool HasPermission(Permissions userPermissions, Permissions required)
    {
        return (userPermissions & required) == required;
    }

    private static bool HasAnyPermission(Permissions userPermissions, Permissions anyOf)
    {
        return (userPermissions & anyOf) != 0;
    }

    [Fact]
    public void HasPermission_AdminHasAll()
    {
        // Arrange
        var adminPerms = Permissions.All;

        // Assert
        HasPermission(adminPerms, Permissions.Read).Should().BeTrue();
        HasPermission(adminPerms, Permissions.Write).Should().BeTrue();
        HasPermission(adminPerms, Permissions.Delete).Should().BeTrue();
        HasPermission(adminPerms, Permissions.Admin).Should().BeTrue();
    }

    [Fact]
    public void HasPermission_ReadOnlyCannotWrite()
    {
        // Arrange
        var readOnly = Permissions.Read;

        // Assert
        HasPermission(readOnly, Permissions.Read).Should().BeTrue();
        HasPermission(readOnly, Permissions.Write).Should().BeFalse();
        HasPermission(readOnly, Permissions.Delete).Should().BeFalse();
    }

    [Fact]
    public void HasPermission_RequiresAllSpecified()
    {
        // Arrange
        var readWrite = Permissions.Read | Permissions.Write;

        // Act & Assert
        HasPermission(readWrite, Permissions.Read | Permissions.Write).Should().BeTrue();
        HasPermission(readWrite, Permissions.Read | Permissions.Delete).Should().BeFalse(
            "user does not have Delete permission");
    }

    [Fact]
    public void HasAnyPermission_MatchesAny()
    {
        // Arrange
        var readOnly = Permissions.Read;

        // Act & Assert
        HasAnyPermission(readOnly, Permissions.Read | Permissions.Write).Should().BeTrue(
            "user has Read which is one of the requested");
    }

    [Fact]
    public void HasAnyPermission_NoMatchReturnsFalse()
    {
        // Arrange
        var readOnly = Permissions.Read;

        // Act & Assert
        HasAnyPermission(readOnly, Permissions.Write | Permissions.Delete).Should().BeFalse();
    }
}


