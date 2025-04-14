# Specification Pattern Implementation Guide

This document explains how to use the Specification pattern that has been implemented in the application.

## What is the Specification Pattern?

The Specification pattern is a pattern that allows you to define reusable, composable query fragments to:

1. Filter collections of objects based on business rules
2. Make complex queries more maintainable and testable
3. Separate the query logic from the application logic

## Components

- `ISpecification<T>`: The interface for specifications
- `BaseSpecification<T>`: Base class that implements the interface
- `SpecificationEvaluator`: Static class that applies specifications to IQueryable
- `SpecificationPaginationRequest<T>`: PaginationRequest that includes a specification

## How to Use

### 1. Create a Specification

Inherit from `BaseSpecification<T>` to create specifications for your entity:

```csharp
public class ProductByNameSpecification : BaseSpecification<Product>
{
    public ProductByNameSpecification(string name)
        : base(p => p.Name.Contains(name))
    {
        // Add includes
        AddInclude(p => p.Category);
        
        // Add ordering
        ApplyOrderBy(p => p.Name);
    }
}
```

### 2. Use Specification in a Repository

```csharp
// Inject the repository
private readonly IGenericDataRepository<Product> _repository;

// Use the repository with a specification
public async Task<PaginationResponse<ProductDto>> GetProductsByName(string name, int pageIndex, int pageSize)
{
    var specification = new ProductByNameSpecification(name);
    
    // Option 1: Use the GetBySpecification method
    var result = await _repository.GetBySpecification<ProductDto>(specification);
    
    // Option 2: Create a SpecificationPaginationRequest
    var request = new SpecificationPaginationRequest<Product>(
        specification, pageIndex, pageSize, "Name", true);
    
    // Then use with existing repository methods that accept PaginationRequest
    var queryable = _repository.GetAllWithIncludeAndConditions();
    return queryable.PaginateWithSpecification(p => _mapper.Map<ProductDto>(p), request);
}
```

### 3. Combine Specifications

To combine specifications, you can create a CompositeSpecification:

```csharp
public class ProductByCategoryAndNameSpecification : BaseSpecification<Product>
{
    public ProductByCategoryAndNameSpecification(int categoryId, string name)
        : base(p => p.CategoryId == categoryId && p.Name.Contains(name))
    {
        AddInclude(p => p.Category);
        ApplyOrderBy(p => p.Name);
    }
}
```

## Advanced Features

### Pagination

You can apply pagination in your specification:

```csharp
public class PagedProductSpecification : BaseSpecification<Product>
{
    public PagedProductSpecification(int skip, int take)
        : base(p => true)  // Match all
    {
        ApplyPaging(skip, take);
    }
}
```

### Include Related Entities

You can include related entities:

```csharp
public class ProductWithDetailsSpecification : BaseSpecification<Product>
{
    public ProductWithDetailsSpecification(int productId)
        : base(p => p.Id == productId)
    {
        AddInclude(p => p.Category);
        AddInclude(p => p.Supplier);
        AddInclude(p => p.OrderDetails);
    }
}
```

### Multiple Ordering

You can apply multiple orderings:

```csharp
public class SortedProductsSpecification : BaseSpecification<Product>
{
    public SortedProductsSpecification()
        : base(p => true)
    {
        ApplyOrderBy(p => p.Category.Name);
        AddOrderBy(p => p.Name);
    }
}
```

## Best Practices

1. Keep specifications focused on a single responsibility
2. Reuse specifications where possible
3. Test specifications in isolation
4. Consider creating a base class for each entity type with common specifications
5. Use meaningful names for specifications that reflect their purpose

## Interaction Specifications Examples

The project includes several specifications for the `Interaction` entity that demonstrate how to use the Specification pattern effectively:

### Basic Filtering Specifications

- `InteractionByTypeSpecification`: Filters interactions by their type (enum)
  ```csharp
  // Get all email interactions
  var emailSpec = new InteractionByTypeSpecification(InteractionType.Email);
  var emailInteractions = await _repository.GetBySpecification<InteractionDto>(emailSpec);
  ```

- `InteractionByDateRangeSpecification`: Filters interactions by date range
  ```csharp
  // Get interactions from last week
  var lastWeekSpec = InteractionByDateRangeSpecification.LastDays(7);
  var recentInteractions = await _repository.GetBySpecification<InteractionDto>(lastWeekSpec);
  
  // Get interactions between specific dates
  var dateRangeSpec = new InteractionByDateRangeSpecification(
      fromDate: new DateTime(2023, 1, 1),
      toDate: new DateTime(2023, 12, 31)
  );
  var yearlyInteractions = await _repository.GetBySpecification<InteractionDto>(dateRangeSpec);
  ```

- `InteractionByContactSpecification`: Filters interactions by contact ID
  ```csharp
  // Get all interactions for a specific contact
  var contactSpec = new InteractionByContactSpecification(contactId: 123);
  var contactInteractions = await _repository.GetBySpecification<InteractionDto>(contactSpec);
  ```

### Include Specifications

- `InteractionWithContactSpecification`: Includes the related Contact entity
  ```csharp
  // Get a specific interaction with its contact details
  var detailSpec = new InteractionWithContactSpecification(interactionId: 456);
  var interaction = await _repository.GetSingleBySpecification(detailSpec);
  
  // Get all interactions with contact details
  var allWithContactsSpec = new InteractionWithContactSpecification();
  var allInteractions = await _repository.GetBySpecification<InteractionDto>(allWithContactsSpec);
  ```

### Composite Specification

- `InteractionCompositeSpecification`: Combines multiple criteria for complex filtering
  ```csharp
  // Get email interactions from a specific contact in the last month
  var compositeSpec = new InteractionCompositeSpecification(
      contactId: 123,
      type: InteractionType.Email,
      fromDate: DateTime.UtcNow.AddMonths(-1)
  );
  var filteredInteractions = await _repository.GetBySpecification<InteractionDto>(compositeSpec);
  ```

### Paging Specification

- `PagedInteractionSpecification`: Applies paging to interaction queries
  ```csharp
  // Get the second page of interactions with 10 items per page
  var pagedSpec = new PagedInteractionSpecification(pageIndex: 2, pageSize: 10);
  var pagedInteractions = await _repository.GetBySpecification<InteractionDto>(pagedSpec);
  ```

This set of specifications demonstrates various filtering techniques and can serve as a foundation for building more complex business-specific queries. 