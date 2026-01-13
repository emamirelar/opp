# Services Tests

## Overview

This folder contains test documentation and C# implementations for all shared services in the UNOPS Opportunity+ system. These tests cover external integrations, utilities, and cross-cutting concerns.

**Total Test Cases**: ~200+
**Status**: ✅ Complete - All tests converted to C#

---

## Test Coverage

| Service | Documentation | C# Tests | Test Count |
|---------|--------------|----------|------------|
| GoogleCloudStorageService | ✅ | ✅ | 35+ |
| GoogleDriveDocumentManager | ✅ | ✅ | 25+ |
| GoogleTextToSpeechService | ✅ | ✅ | 18+ |
| TextExtractionService | ✅ | ✅ | 20+ |
| AiContextualService | ✅ | ✅ | 20+ |
| OrganizationHierarchyLookupService | ✅ | ✅ | 22+ |
| CountryService | ✅ | ✅ | 15+ |
| SavedFilterService | ✅ | ✅ | 20+ |
| AuthenticationService | ✅ | ✅ | 25+ |
| EmailService | ✅ | ✅ | 20+ |

---

## C# Test File Location

Located in: `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Services/`

| File | Services Covered |
|------|------------------|
| AllServicesFullTests.cs | All services |
| GoogleCloudStorageServiceTests.cs | GCS operations |
| GoogleDriveDocumentManagerTests.cs | Google Drive operations |
| GoogleTextToSpeechServiceTests.cs | TTS operations |
| TextExtractionServiceTests.cs | Document text extraction |
| AiContextualServiceTests.cs | AI context building |
| OrganizationHierarchyLookupServiceTests.cs | Org hierarchy lookups |
| CountryServiceTests.cs | Country data |
| SavedFilterServiceTests.cs | Saved filters |

---

## Service Categories

### Storage Services
- **GoogleCloudStorageService**: Upload, download, delete, signed URLs
- **GoogleDriveDocumentManager**: File operations, sharing, export

### AI/ML Services
- **AiContextualService**: Context building for AI prompts
- **GoogleTextToSpeechService**: Text-to-speech synthesis
- **TextExtractionService**: PDF/Word/Excel text extraction, OCR

### Lookup Services
- **OrganizationHierarchyLookupService**: Org hierarchy tree
- **CountryService**: Country data and validation
- **SavedFilterService**: User-saved filters

### Authentication Services
- **AuthenticationService**: Login, MFA, token management

### Communication Services
- **EmailService**: Email sending, templates

---

## Test Patterns

### External Service Tests
```csharp
// Mock external dependencies
var mockGcsClient = new Mock<StorageClient>();
mockGcsClient.Setup(x => x.UploadObjectAsync(...)).Returns(...);

// Test with mocked dependencies
var service = new GoogleCloudStorageService(mockGcsClient.Object);
var result = await service.UploadAsync(file);

// Verify behavior
mockGcsClient.Verify(x => x.UploadObjectAsync(...), Times.Once);
```

### Error Handling Tests
```csharp
// Test retry logic
mockClient.SetupSequence(x => x.UploadAsync(...))
    .ThrowsAsync(new TransientException())
    .ThrowsAsync(new TransientException())
    .ReturnsAsync(successResult);

// Verify retries and eventual success
```

---

## Running Tests

```powershell
# Run all service tests
dotnet test --filter "Namespace~Services"

# Run specific service tests
dotnet test --filter "FullyQualifiedName~GoogleCloudStorageServiceFullTests"
dotnet test --filter "FullyQualifiedName~AuthenticationServiceFullTests"

# Run by category
dotnet test --filter "Name~Upload"    # Upload operations
dotnet test --filter "Name~Download"  # Download operations
dotnet test --filter "Name~Auth"      # Authentication
```

---

*Last Updated: December 19, 2025*
