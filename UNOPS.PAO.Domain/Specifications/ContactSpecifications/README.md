# Contact Specifications

This folder contains specification classes for filtering and querying Contact entities in the UNOPS PAO system.

## Available Specifications

### ContactCompositeSpecification
A composite specification that combines multiple filter criteria for contacts:
- `partnerId`: Filter contacts by partner
- `status`: Filter contacts by status
- `searchText`: Search in contact name or email

Example:
```csharp
var specification = new ContactCompositeSpecification(
    partnerId: 123,
    status: "Active",
    searchText: "john"
);
```

### PagedContactSpecification
Provides paginated access to contacts:
- `pageIndex`: The page number (starts at 1)
- `pageSize`: Number of contacts per page

Example:
```csharp
var specification = new PagedContactSpecification(pageIndex: 1, pageSize: 10);
```

### ContactByIdSpecification
Filters contacts by ID:

Example:
```csharp
var specification = new ContactByIdSpecification(id: 123);
```

### ContactBySalutationSpecification
Filters contacts by salutation:

Example:
```csharp
var specification = new ContactBySalutationSpecification(salutation: "Mr.");
```

### ContactByPartnerSpecification
Filters contacts by partner ID:

Example:
```csharp
var specification = new ContactByPartnerSpecification(partnerId: 123);
```

### ContactByStatusSpecification
Filters contacts by status:

Example:
```csharp
var specification = new ContactByStatusSpecification(status: "Active");
```

### ContactByNameSpecification
Searches contacts by first or last name:

Example:
```csharp
var specification = new ContactByNameSpecification(searchText: "Smith");
```

### ContactByTitleSpecification
Filters contacts by title:

Example:
```csharp
// Exact match
var exactSpecification = new ContactByTitleSpecification(title: "Director", exactMatch: true);

// Partial match
var partialSpecification = new ContactByTitleSpecification(title: "Director");
```

### ContactByDepartmentSpecification
Filters contacts by department:

Example:
```csharp
var specification = new ContactByDepartmentSpecification(department: "Finance");
```

### ContactByDescriptionSpecification
Filters contacts by description:

Example:
```csharp
var specification = new ContactByDescriptionSpecification(description: "key contact");
```

### ContactByEmailSpecification
Filters contacts by email address:

Example:
```csharp
// Find contacts with an exact email match
var exactSpecification = new ContactByEmailSpecification(email: "john.smith@example.com", exactMatch: true);

// Find contacts with a partial email match
var partialSpecification = new ContactByEmailSpecification(email: "example.com");
```

### ContactByPhoneSpecification
Filters contacts by phone number:

Example:
```csharp
var specification = new ContactByPhoneSpecification(phone: "555-1234");
```

### ContactByMobileSpecification
Filters contacts by mobile number:

Example:
```csharp
var specification = new ContactByMobileSpecification(mobile: "555-5678");
```

### ContactByAssistantSpecification
Filters contacts by assistant name:

Example:
```csharp
var specification = new ContactByAssistantSpecification(assistant: "Jane Doe");
```

### ContactByAssistantEmailSpecification
Filters contacts by assistant email:

Example:
```csharp
var specification = new ContactByAssistantEmailSpecification(assistantEmail: "jane.doe@example.com");
```

### ContactByAssistantPhoneSpecification
Filters contacts by assistant phone:

Example:
```csharp
var specification = new ContactByAssistantPhoneSpecification(assistantPhone: "555-9012");
```

### ContactByMailingCitySpecification
Filters contacts by mailing city:

Example:
```csharp
var specification = new ContactByMailingCitySpecification(city: "New York");
```

### ContactByMailingStateProvinceSpecification
Filters contacts by mailing state/province:

Example:
```csharp
var specification = new ContactByMailingStateProvinceSpecification(stateProvince: "NY");
```

### ContactByMailingPostalCodeSpecification
Filters contacts by mailing postal code:

Example:
```csharp
var specification = new ContactByMailingPostalCodeSpecification(postalCode: "10001");
```

### ContactByMailingCountrySpecification
Filters contacts by mailing country:

Example:
```csharp
var specification = new ContactByMailingCountrySpecification(country: "USA");
```

## Usage

These specifications can be used with a repository implementation that accepts `ISpecification<Contact>`:

```csharp
// Example repository usage
var contacts = await _contactRepository.ListAsync(new ContactByPartnerSpecification(partnerId: 123));
``` 