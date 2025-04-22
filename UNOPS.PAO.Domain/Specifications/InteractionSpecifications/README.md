# Interaction Specifications

This folder contains specifications for the `Interaction` entity using the Specification pattern. These specifications encapsulate reusable query logic for filtering, sorting, and including related data.

## Available Specifications

### Basic Filtering

- **InteractionByTypeSpecification**: Filter interactions by their type
- **InteractionByDateRangeSpecification**: Filter interactions by date range
- **InteractionByContactSpecification**: Filter interactions by contact ID

### Including Related Data

- **InteractionWithContactSpecification**: Include the Contact navigation property

### Complex Filtering

- **InteractionCompositeSpecification**: Combine multiple filtering criteria
- **PagedInteractionSpecification**: Apply paging to interaction queries

## Usage Examples

### Filtering by Interaction Type

```csharp
// Get all email interactions
var repository = serviceProvider.GetRequiredService<IGenericDataRepository<Interaction>>();
var emailSpec = new InteractionByTypeSpecification(InteractionType.Email);
var emailInteractions = await repository.GetBySpecification<InteractionDto>(emailSpec);
```

### Filtering by Date Range

```csharp
// Get interactions from last 7 days
var lastWeekSpec = InteractionByDateRangeSpecification.LastDays(7);
var recentInteractions = await repository.GetBySpecification<InteractionDto>(lastWeekSpec);

// Get interactions for a specific date range
var dateRangeSpec = new InteractionByDateRangeSpecification(
    fromDate: new DateTime(2023, 1, 1),
    toDate: new DateTime(2023, 12, 31)
);
var yearlyInteractions = await repository.GetBySpecification<InteractionDto>(dateRangeSpec);
```

### Filtering by Contact

```csharp
// Get all interactions for a specific contact
var contactSpec = new InteractionByContactSpecification(contactId: 123);
var contactInteractions = await repository.GetBySpecification<InteractionDto>(contactSpec);
```

### Including Contact Details

```csharp
// Get a specific interaction with its contact details
var detailSpec = new InteractionWithContactSpecification(interactionId: 456);
var interaction = await repository.GetSingleBySpecification(detailSpec);
```

### Composite Filtering

```csharp
// Get email interactions from a specific contact in the last month
var compositeSpec = new InteractionCompositeSpecification(
    contactId: 123,
    type: InteractionType.Email,
    fromDate: DateTime.UtcNow.AddMonths(-1)
);
var filteredInteractions = await repository.GetBySpecification<InteractionDto>(compositeSpec);
```

### Paging

```csharp
// Get the second page of interactions with 10 items per page
var pagedSpec = new PagedInteractionSpecification(pageIndex: 2, pageSize: 10);
var pagedInteractions = await repository.GetBySpecification<InteractionDto>(pagedSpec);
```

## Creating Custom Specifications

To create your own interaction specifications, inherit from `BaseSpecification<Interaction>`:

```csharp
public class MyCustomInteractionSpec : BaseSpecification<Interaction>
{
    public MyCustomInteractionSpec(string searchTerm)
        : base(i => i.Data != null && Encoding.UTF8.GetString(i.Data).Contains(searchTerm))
    {
        // Add ordering, includes, etc.
        ApplyOrderByDescending(i => i.Date);
        AddInclude(i => i.Contact);
    }
}
``` 