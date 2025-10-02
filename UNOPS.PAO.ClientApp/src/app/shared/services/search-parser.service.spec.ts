import { TestBed } from '@angular/core/testing';
import { SearchParserService, SearchField, SearchCriterion } from './search-parser.service';

describe('SearchParserService', () => {
  let service: SearchParserService;
  let mockAvailableFields: SearchField[];

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SearchParserService);
    
    mockAvailableFields = [
      { field: 'name', label: 'Name', type: 'string', operators: ['is', 'is not', 'like', 'not like'] },
      { field: 'age', label: 'Age', type: 'number', operators: ['is', '>', '<', '>=', '<='] },
      { field: 'email', label: 'Email Address', type: 'string', operators: ['is', 'like'] },
      { field: 'birthDate', label: 'Birth Date', type: 'date', operators: ['after', 'before', 'between'] },
      { field: 'isActive', label: 'Active Status', type: 'boolean', operators: ['is'] }
    ];
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('parseQuery', () => {
    it('should parse simple field-operator-value query', () => {
      const query = 'name like John';
      const result = service.parseQuery(query, mockAvailableFields);
      
      expect(result).toEqual([
        { field: 'name', operator: 'like', value: 'John', logicalOperator: 'AND' }
      ]);
    });

    it('should parse query with quoted values', () => {
      const query = 'name is "John Doe"';
      const result = service.parseQuery(query, mockAvailableFields);
      
      expect(result).toEqual([
        { field: 'name', operator: 'is', value: 'John Doe', logicalOperator: 'AND' }
      ]);
    });

    it('should parse complex query with AND operator', () => {
      const query = 'name like John AND age > 25';
      const result = service.parseQuery(query, mockAvailableFields);
      
      expect(result).toEqual([
        { field: 'name', operator: 'like', value: 'John', logicalOperator: 'AND' },
        { field: 'age', operator: '>', value: '25', logicalOperator: 'AND' }
      ]);
    });




    it('should ignore invalid fields', () => {
      const query = 'invalidField like test AND name is John';
      const result = service.parseQuery(query, mockAvailableFields);
      
      expect(result).toEqual([
        { field: 'name', operator: 'is', value: 'John', logicalOperator: 'AND' }
      ]);
    });

    it('should handle case insensitive field matching', () => {
      const query = 'NAME like john';
      const result = service.parseQuery(query, mockAvailableFields);
      
      expect(result).toEqual([
        { field: 'name', operator: 'like', value: 'john', logicalOperator: 'AND' }
      ]);
    });

    it('should return empty array for invalid query', () => {
      const query = 'invalid query structure';
      const result = service.parseQuery(query, mockAvailableFields);
      
      expect(result).toEqual([]);
    });

  });

  describe('getSuggestions', () => {
    it('should suggest fields for empty query', () => {
      const result = service.getSuggestions('', 0, mockAvailableFields);
      
      expect(result.type).toBe('field');
      expect(result.suggestions).toEqual(['name', 'age', 'email', 'birthDate', 'isActive']);
    });

    it('should suggest matching fields for partial field input', () => {
      const result = service.getSuggestions('na', 2, mockAvailableFields);
      
      expect(result.type).toBe('field');
      expect(result.suggestions).toEqual(['name']);
    });

    it('should suggest operators after field input', () => {
      const result = service.getSuggestions('name ', 5, mockAvailableFields);
      
      expect(result.type).toBe('operator');
      expect(result.suggestions).toEqual(['is', 'is not', 'like', 'not like']);
    });

    it('should suggest operators specific to field type', () => {
      const result = service.getSuggestions('age ', 4, mockAvailableFields);
      
      expect(result.type).toBe('operator');
      expect(result.suggestions).toEqual(['is', '>', '<', '>=', '<=']);
    });

    it('should suggest logical operators after complete criterion', () => {
      const result = service.getSuggestions('name like John ', 15, mockAvailableFields);
      
      expect(result.type).toBe('logical');
      expect(result.suggestions).toEqual(['AND', 'OR']);
    });

    it('should suggest fields after logical operator', () => {
      const result = service.getSuggestions('name like John AND ', 20, mockAvailableFields);
      
      expect(result.type).toBe('field');
      expect(result.suggestions).toEqual(['name', 'age', 'email', 'birthDate', 'isActive']);
    });

    it('should handle suggestions with quoted values', () => {
      const result = service.getSuggestions('name is "John Doe" ', 18, mockAvailableFields);
      
      expect(result.type).toBe('logical');
      expect(result.suggestions).toEqual(['AND', 'OR']);
    });

    it('should return empty suggestions for invalid context', () => {
      const result = service.getSuggestions('invalid context', 10, mockAvailableFields);
      
      expect(result.type).toBe('field');
      expect(result.suggestions).toEqual([]);
    });
  });

  describe('field validation', () => {
    it('should validate field names correctly', () => {
      const validField = 'name';
      const invalidField = 'nonexistent';
      
      // Test private method through public interface
      const validResult = service.parseQuery(`${validField} like test`, mockAvailableFields);
      const invalidResult = service.parseQuery(`${invalidField} like test`, mockAvailableFields);
      
      expect(validResult.length).toBe(1);
      expect(invalidResult.length).toBe(0);
    });

  });

  describe('tokenization edge cases', () => {
    it('should handle empty query', () => {
      const result = service.parseQuery('', mockAvailableFields);
      expect(result).toEqual([]);
    });

    it('should handle query with only spaces', () => {
      const result = service.parseQuery('   ', mockAvailableFields);
      expect(result).toEqual([]);
    });

    it('should handle malformed quoted strings', () => {
      const query = 'name like "unclosed quote';
      const result = service.parseQuery(query, mockAvailableFields);
      
      expect(result.length).toBe(0);
    });

    it('should handle parentheses (when implemented)', () => {
      const query = '(name like John)';
      const result = service.parseQuery(query, mockAvailableFields);
      
      // Currently parentheses are tokenized but not processed in parsing logic
      // This test documents current behavior
      expect(result.length).toBe(0);
    });
  });
});