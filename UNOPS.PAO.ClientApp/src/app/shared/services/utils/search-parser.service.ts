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
    const tokens = this.tokenize(query, availableFields);
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
      const fieldName = f.field.toLowerCase().replace(/\s+/g, '');
      const fieldLabel = (f.label || '').toLowerCase().replace(/\s+/g, '');
      return fieldName === normalizedInput || fieldLabel === normalizedInput;
    });
    return field ? field.field : null;
  }

  private tokenize(query: string, availableFields: SearchField[]): SearchToken[] {
    const tokens: SearchToken[] = [];
    const parts = this.splitQuery(query, availableFields);
    
    for (let i = 0; i < parts.length; i++) {
      const part = parts[i].trim();
      if (part) {
        const type = this.getTokenType(part, i, parts, availableFields);
        tokens.push({ type, value: part });
      }
    }

    return tokens;
  }

  private splitQuery(query: string, availableFields: SearchField[]): string[] {
    const parts: string[] = [];
    let current = '';
    let inQuotes = false;
    
    for (let i = 0; i < query.length; i++) {
      const char = query[i];
      
      if (char === '"') {
        if (inQuotes) {
          parts.push(current);
          current = '';
        }
        inQuotes = !inQuotes;
        continue;
      }
      
      if (inQuotes) {
        current += char;
        continue;
      }
      
      if (char === ' ') {
        if (current.trim()) {
          // Check for multi-word operators first
          const remaining = query.substring(i + 1).trim();
          const nextWord = remaining.split(' ')[0];
          if (nextWord) {
            const potentialOperator = (current + ' ' + nextWord).toLowerCase();
            
            if (this.operators.some(op => op.toLowerCase() === potentialOperator)) {
              current += ' ' + nextWord;
              // Skip over the next word and any spaces
              i += nextWord.length + 1;
              // Skip any additional spaces
              while (i < query.length && query[i] === ' ') {
                i++;
              }
              i--; // Adjust for the loop increment
              continue;
            }
            
            // Check for multi-word field labels
            const potentialField = (current + ' ' + nextWord);
            if (this.isValidField(potentialField, availableFields)) {
              current += ' ' + nextWord;
              i += nextWord.length + 1;
              while (i < query.length && query[i] === ' ') {
                i++;
              }
              i--; // Adjust for the loop increment
              continue;
            }
          }
          
          parts.push(current);
          current = '';
        }
        continue;
      }
      
      current += char;
    }
    
    // Handle unclosed quotes - if we're still in quotes at the end, it's malformed
    if (inQuotes) {
      // Return empty array to indicate malformed query
      return [];
    }
    
    if (current.trim()) {
      parts.push(current);
    }
    
    return parts;
  }

  private getTokenType(value: string, index: number, allParts: string[], availableFields: SearchField[]): SearchToken['type'] {
    const trimmedValue = value.trim();
    
    if (this.logicalOperators.includes(trimmedValue.toUpperCase())) return 'logical';
    if (this.operators.includes(trimmedValue.toLowerCase())) return 'operator';
    
    // If this is after an operator, it's likely a value
    if (index > 0) {
      const previousPart = allParts[index - 1];
      if (this.operators.includes(previousPart.toLowerCase())) {
        return 'value';
      }
    }
    
    // Check if it's a valid field
    if (this.isValidField(trimmedValue, availableFields)) {
      return 'field';
    }
    
    // Default to field for unknown values
    return 'field';
  }

  private parseTokens(tokens: SearchToken[], availableFields: SearchField[]): SearchCriterion[] {
    const criteria: SearchCriterion[] = [];
    let currentCriterion: Partial<SearchCriterion> = {};
    let nextLogicalOperator: 'AND' | 'OR' = 'AND';

    for (let i = 0; i < tokens.length; i++) {
      const token = tokens[i];

      switch (token.type) {
        case 'field': {
          const actualField = this.getFieldFromInput(token.value, availableFields);
          if (actualField) {
            // Complete previous criterion if exists
            if (currentCriterion.field && currentCriterion.operator && currentCriterion.value) {
              criteria.push({ ...currentCriterion as SearchCriterion, logicalOperator: criteria.length === 0 ? 'AND' : nextLogicalOperator });
              nextLogicalOperator = 'AND'; // Reset for next criterion
            }
            currentCriterion = { field: actualField };
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
          nextLogicalOperator = token.value.toUpperCase() as 'AND' | 'OR';
          break;
      }
    }

    // Add the last criterion
    if (currentCriterion.field && currentCriterion.operator && currentCriterion.value) {
      criteria.push({ ...currentCriterion as SearchCriterion, logicalOperator: criteria.length === 0 ? 'AND' : nextLogicalOperator });
    }

    return criteria;
  }

  getSuggestions(
    query: string, 
    cursorPosition: number, 
    availableFields: SearchField[]
  ): { suggestions: string[], type: 'field' | 'operator' | 'value' | 'logical' } {
    const textBeforeCursor = query.substring(0, cursorPosition);
    const tokens = this.tokenize(textBeforeCursor, availableFields);
    
    // If no tokens or cursor is at the start, suggest fields
    if (tokens.length === 0) {
      return {
        suggestions: availableFields.map(f => f.field),
        type: 'field'
      };
    }

    const lastToken = tokens[tokens.length - 1];
    const secondLastToken = tokens[tokens.length - 2];

    // If we have a complete criterion (field + operator + value), suggest logical operators
    const hasCompleteCriterion = tokens.length >= 3 && 
      tokens.some(t => t.type === 'field') && 
      tokens.some(t => t.type === 'operator') && 
      tokens.some(t => t.type === 'value');

    if (hasCompleteCriterion && lastToken.type === 'value') {
      return {
        suggestions: this.logicalOperators,
        type: 'logical'
      };
    }

    // If last token is a logical operator, suggest fields
    if (lastToken.type === 'logical') {
      return {
        suggestions: availableFields.map(f => f.field),
        type: 'field'
      };
    }

    // If we're typing a field name, suggest matching fields
    if (lastToken.type === 'field') {
      const matchingFields = availableFields
        .map(f => f.field)
        .filter(f => f.toLowerCase().startsWith(lastToken.value.toLowerCase()));
      
      // If there's a space after the field, suggest operators
      if (textBeforeCursor.endsWith(' ')) {
        const field = availableFields.find(f => 
          this.getFieldFromInput(lastToken.value, availableFields) === f.field
        );
        return { 
          suggestions: field?.operators || this.operators,
          type: 'operator'
        };
      }
      
      return { suggestions: matchingFields, type: 'field' };
    }

    // If we have a field and now need an operator
    if (secondLastToken?.type === 'field' && lastToken.type !== 'operator') {
      const fieldName = this.getFieldFromInput(secondLastToken.value, availableFields);
      const field = availableFields.find(f => f.field === fieldName);
      return { 
        suggestions: field?.operators || this.operators,
        type: 'operator'
      };
    }

    // If we have field + operator, we're expecting a value (no suggestions)
    if (tokens.length >= 2 && tokens[tokens.length - 2].type === 'operator') {
      return {
        suggestions: [],
        type: 'value'
      };
    }

    // Default: suggest fields
    return {
      suggestions: availableFields.map(f => f.field),
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
