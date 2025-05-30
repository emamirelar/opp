import { Injectable } from '@angular/core';

export interface SearchToken {
  type: 'field' | 'operator' | 'value' | 'logical' | 'parenthesis';
  value: string;
}

export interface SearchCriterion {
  field: string;
  operator: string;
  value: string;
  logicalOperator?: 'AND' | 'OR';
}

export interface SearchField {
  field: string;
  label: string;
  type: 'string' | 'number' | 'date' | 'boolean';
  operators: string[];
}

@Injectable({
  providedIn: 'root'
})
export class SearchParserService {
  private readonly operators = ['is', 'is not', 'like', 'not like', '>', '<', '>=', '<=', 'after', 'before', 'between', 'in'];
  private readonly logicalOperators = ['AND', 'OR'];

  constructor() {}

  parseQuery(query: string, availableFields: SearchField[]): SearchCriterion[] {
    const tokens = this.tokenize(query);
    return this.parseTokens(tokens, availableFields);
  }

  private isValidField(field: string, availableFields: SearchField[]): boolean {
    const normalizedField = field.toLowerCase().replace(/\s+/g, '');
    return availableFields.some(f => {
      const fieldName = f.field.toLowerCase();
      const fieldLabel = (f.label || '').toLowerCase().replace(/\s+/g, '');
      return fieldName === normalizedField || fieldLabel === normalizedField;
    });
  }

  private getFieldFromInput(input: string, availableFields: SearchField[]): string | null {
    const normalizedInput = input.toLowerCase().replace(/\s+/g, '');
    const field = availableFields.find(f => {
      const fieldName = f.field.toLowerCase();
      const fieldLabel = (f.label || '').toLowerCase().replace(/\s+/g, '');
      return fieldName === normalizedInput || fieldLabel === normalizedInput;
    });
    return field ? field.field : null;
  }

  private tokenize(query: string): SearchToken[] {
    const tokens: SearchToken[] = [];
    let current = '';
    let inQuotes = false;
    let isMultiWordOperator = false;
    
    const addToken = (value: string, type: SearchToken['type']) => {
      if (value.trim()) {
        tokens.push({ type, value: value.trim() });
      }
    };

    for (let i = 0; i < query.length; i++) {
      const char = query[i];

      if (char === '"') {
        if (inQuotes) {
          addToken(current, 'value');
          current = '';
        }
        inQuotes = !inQuotes;
        continue;
      }

      if (inQuotes) {
        current += char;
        continue;
      }

      // Check for multi-word operators
      if (char === ' ') {
        const potentialOperator = (current + ' ' + query.substring(i + 1).split(' ')[0]).toLowerCase();
        isMultiWordOperator = this.operators.some(op => op.toLowerCase().startsWith(potentialOperator));
        
        if (!isMultiWordOperator) {
          addToken(current, this.getTokenType(current));
          current = '';
          continue;
        }
      }

      if (char === '(' || char === ')') {
        addToken(current, this.getTokenType(current));
        addToken(char, 'parenthesis');
        current = '';
        continue;
      }

      current += char;
    }

    if (current.trim()) {
      addToken(current, this.getTokenType(current));
    }

    return tokens;
  }

  private getTokenType(value: string): SearchToken['type'] {
    if (this.operators.includes(value.toLowerCase())) return 'operator';
    if (this.logicalOperators.includes(value.toUpperCase())) return 'logical';
    return 'field';
  }

  private parseTokens(tokens: SearchToken[], availableFields: SearchField[]): SearchCriterion[] {
    const criteria: SearchCriterion[] = [];
    let currentCriterion: Partial<SearchCriterion> = {};
    let lastLogicalOperator: 'AND' | 'OR' = 'AND';

    for (let i = 0; i < tokens.length; i++) {
      const token = tokens[i];
      console.log('Processing token:', token);

      switch (token.type) {
        case 'field': {
          const actualField = this.getFieldFromInput(token.value, availableFields);
          if (actualField) {
            if (Object.keys(currentCriterion).length > 0) {
              criteria.push({ ...currentCriterion as SearchCriterion, logicalOperator: lastLogicalOperator });
              currentCriterion = {};
            }
            currentCriterion.field = actualField;
          }
          break;
        }

        case 'operator':
          if (currentCriterion.field) {
            currentCriterion.operator = token.value.toLowerCase();
          }
          break;

        case 'value':
          if (currentCriterion.field && currentCriterion.operator) {
            currentCriterion.value = token.value;
          }
          break;

        case 'logical':
          lastLogicalOperator = token.value.toUpperCase() as 'AND' | 'OR';
          break;
      }
    }

    if (currentCriterion.field && currentCriterion.operator && currentCriterion.value) {
      criteria.push({ ...currentCriterion as SearchCriterion, logicalOperator: lastLogicalOperator });
    }

    console.log('Parsed criteria:', criteria);
    return criteria;
  }

  getSuggestions(
    query: string, 
    cursorPosition: number, 
    availableFields: SearchField[]
  ): { suggestions: string[], type: 'field' | 'operator' | 'value' | 'logical' } {
    const textBeforeCursor = query.substring(0, cursorPosition);
    const tokens = this.tokenize(textBeforeCursor);
    
    if (tokens.length === 0 || this.isLastTokenComplete(tokens)) {
      return {
        suggestions: availableFields.map(f => f.field),
        type: 'field'
      };
    }

    const lastToken = tokens[tokens.length - 1];
    const secondLastToken = tokens[tokens.length - 2];

    if (lastToken.type === 'field') {
      const matchingFields = availableFields
        .map(f => f.field)
        .filter(f => f.toLowerCase().startsWith(lastToken.value.toLowerCase()));
      return { suggestions: matchingFields, type: 'field' };
    }

    if (lastToken.type === 'operator' || (secondLastToken?.type === 'field' && !lastToken.value)) {
      const field = availableFields.find(f => f.field === secondLastToken?.value);
      return { 
        suggestions: field?.operators || this.operators,
        type: 'operator'
      };
    }

    if (lastToken.type === 'value' && tokens.length > 2) {
      return {
        suggestions: this.logicalOperators,
        type: 'logical'
      };
    }

    return {
      suggestions: [],
      type: 'field'
    };
  }

  private isLastTokenComplete(tokens: SearchToken[]): boolean {
    if (tokens.length === 0) return true;
    const lastToken = tokens[tokens.length - 1];
    return lastToken.type === 'logical' || 
           (lastToken.type === 'value' && tokens.length >= 3);
  }
} 