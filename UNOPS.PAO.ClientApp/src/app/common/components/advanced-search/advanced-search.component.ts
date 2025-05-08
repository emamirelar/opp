import { Component, EventEmitter, Input, Output, ViewChild, ElementRef } from '@angular/core';
import { SearchParserService, SearchField, SearchCriterion } from '../../services/search-parser.service';
import { FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { DropdownModule } from 'primeng/dropdown';
import { ChipModule } from 'primeng/chip';

interface Operator {
  label: string;
  value: string;
}

@Component({
  selector: 'app-advanced-search',
  templateUrl: './advanced-search.component.html',
  styleUrls: ['./advanced-search.component.scss'],
  imports: [
    FormsModule,
    InputTextModule,
    IconField,
    InputIcon,
    DropdownModule,
    ChipModule
  ],
  standalone: true
})
export class AdvancedSearchComponent {
  private _searchableFields: SearchField[] = [];
  
  @Input()
  set searchableFields(value: SearchField[]) {
    console.log('Advanced search received searchable fields:', value);
    this._searchableFields = value;
    // Update suggestions if needed
    if (this.searchInput) {
      this.updateSuggestions();
    }
  }
  get searchableFields(): SearchField[] {
    return this._searchableFields;
  }

  @Input() advancedSearchEnabled = false;
  @Output() searchChange = new EventEmitter<{ criteria: SearchCriterion[] }>();
  
  @ViewChild('searchInput') searchInput!: ElementRef;
  @ViewChild('suggestionsContainer') suggestionsContainer!: ElementRef;

  selectedField: SearchField | null = null;
  selectedOperator: string | null = null;
  searchText = '';
  suggestions: string[] = [];
  showSuggestions = false;
  selectedSuggestionIndex = -1;
  activeCriteria: SearchCriterion[] = [];
  suggestionType: 'field' | 'operator' | 'value' | 'logical' = 'field';

  availableOperators: Operator[] = [];

  constructor(private searchParser: SearchParserService) {}

  // Transform searchable fields to handle nested properties
  get formattedSearchableFields(): SearchField[] {
    console.log('Getting formatted searchable fields from:', this._searchableFields);
    const formatted = this._searchableFields.map(field => ({
      ...field,
      label: this.formatFieldLabel(field),
      field: field.field // Keep the original field path (e.g., 'partner.name')
    }));
    console.log('Formatted fields:', formatted);
    return formatted;
  }

  private formatFieldLabel(field: SearchField): string {
    // Convert dot notation to readable format (e.g., 'partner.name' -> 'Partner Name')
    if (field.label) {
      console.log(`Using provided label for ${field.field}: ${field.label}`);
      return field.label;
    }
    
    const formattedLabel = field.field
      .split('.')
      .map(part => part.charAt(0).toUpperCase() + part.slice(1))
      .join(' › ');
    console.log(`Generated label for ${field.field}: ${formattedLabel}`);
    return formattedLabel;
  }

  onFieldChange(): void {
    this.selectedOperator = null;
    this.searchText = '';
    this.updateAvailableOperators();
  }

  updateAvailableOperators(): void {
    if (!this.selectedField) {
      this.availableOperators = [];
      return;
    }

    const operators = this.getOperatorsForType(this.selectedField.type);
    this.availableOperators = operators.map(op => ({
      label: this.getOperatorLabel(op),
      value: op
    }));
  }

  getOperatorLabel(operator: string): string {
    const labels: { [key: string]: string } = {
      'is': 'is',
      'is not': 'is not',
      'like': 'contains',
      'not like': 'does not contain',
      '>': 'greater than',
      '<': 'less than',
      '>=': 'greater than or equal to',
      '<=': 'less than or equal to'
    };
    return labels[operator] || operator;
  }

  getOperatorsForType(type: string): string[] {
    switch (type) {
      case 'string':
        return ['is', 'is not', 'like', 'not like'];
      case 'number':
        return ['is', 'is not', '>', '<', '>=', '<='];
      case 'date':
        return ['is', 'is not', '>', '<', '>=', '<='];
      case 'boolean':
        return ['is', 'is not'];
      default:
        return ['is', 'is not'];
    }
  }

  getPlaceholder(): string {
    if (!this.selectedField) {
      return 'Select a field first';
    }
    if (!this.selectedOperator) {
      return 'Select an operator';
    }
    return `Enter value for ${this.formatFieldLabel(this.selectedField)}`;
  }

  onInput(event: any): void {
    const input = event.target as HTMLInputElement;
    this.searchText = input.value;
    
    // Always update suggestions on input
    this.updateSuggestions();
  }

  onKeydown(event: KeyboardEvent): void {
    console.log('Keydown event:', event.key);
    console.log('Current state:', {
      searchText: this.searchText,
      selectedSuggestionIndex: this.selectedSuggestionIndex,
      suggestions: this.suggestions,
      showSuggestions: this.showSuggestions
    });
    
    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        if (this.suggestions.length > 0) {
          this.selectedSuggestionIndex = Math.min(
            this.selectedSuggestionIndex + 1,
            this.suggestions.length - 1
          );
          this.scrollToSelectedSuggestion();
        }
        break;

      case 'ArrowUp':
        event.preventDefault();
        if (this.suggestions.length > 0) {
          this.selectedSuggestionIndex = Math.max(this.selectedSuggestionIndex - 1, -1);
          this.scrollToSelectedSuggestion();
        }
        break;

      case 'Enter':
        event.preventDefault();
        console.log('Enter pressed. Processing...');
        
        if (this.showSuggestions && this.selectedSuggestionIndex >= 0) {
          console.log('Applying selected suggestion:', this.suggestions[this.selectedSuggestionIndex]);
          this.applySuggestion(this.suggestions[this.selectedSuggestionIndex]);
        } else if (this.searchText.trim()) {
          console.log('No suggestion selected, parsing search text:', this.searchText);
          this.parseAndAddCriteria();
        } else {
          console.log('No action taken: empty search text');
        }
        break;

      case 'Escape':
        this.showSuggestions = false;
        break;

      case 'Tab':
        if (this.showSuggestions && this.suggestions.length > 0) {
          event.preventDefault();
          this.applySuggestion(this.suggestions[0]);
        }
        break;
    }
  }

  private updateSuggestions(): void {
    // Split the search text into parts
    const parts = this.searchText.trim().split(/\s+/);
    const lastSpaceIndex = this.searchText.lastIndexOf(' ');
    const currentWord = lastSpaceIndex >= 0 ? 
      this.searchText.substring(lastSpaceIndex + 1).trim() : 
      this.searchText.trim();

    console.log('Update suggestions state:', {
      searchText: this.searchText,
      parts,
      lastSpaceIndex,
      currentWord,
      endsWithSpace: this.searchText.endsWith(' ')
    });

    // If we have a complete field-operator-value sequence, don't show suggestions
    if (parts.length >= 3 && !this.searchText.endsWith(' ')) {
      this.showSuggestions = false;
      return;
    }

    const { suggestions, type } = this.searchParser.getSuggestions(
      this.searchText,
      this.searchInput.nativeElement.selectionStart,
      this.searchableFields
    );

    console.log('Parser suggestions:', { suggestions, type });

    // Determine if we should show suggestions based on context
    let shouldShowSuggestions = false;

    if (suggestions.length > 0) {
      if (type === 'field') {
        // Show field suggestions when typing a new word
        shouldShowSuggestions = currentWord.length > 0;
      } else if (type === 'operator') {
        // Show operator suggestions after a field and space
        shouldShowSuggestions = parts.length === 1 && this.searchText.endsWith(' ');
      } else if (type === 'logical') {
        // Show logical operators after a complete criterion and space
        shouldShowSuggestions = parts.length >= 3 && this.searchText.endsWith(' ');
      }
    }

    this.suggestions = suggestions;
    this.suggestionType = type;
    this.showSuggestions = shouldShowSuggestions;
    this.selectedSuggestionIndex = -1;

    console.log('Final suggestion state:', {
      shouldShowSuggestions,
      suggestionType: type,
      suggestionsCount: suggestions.length
    });
  }

  private scrollToSelectedSuggestion(): void {
    if (this.selectedSuggestionIndex >= 0 && this.suggestionsContainer) {
      const container = this.suggestionsContainer.nativeElement;
      const selectedElement = container.children[this.selectedSuggestionIndex];
      if (selectedElement) {
        selectedElement.scrollIntoView({ block: 'nearest' });
      }
    }
  }

  applySuggestion(suggestion: string): void {
    const input = this.searchInput.nativeElement;
    const cursorPosition = input.selectionStart;
    const textBeforeCursor = this.searchText.substring(0, cursorPosition);
    const textAfterCursor = this.searchText.substring(input.selectionEnd);

    // Find the last word boundary before cursor
    const lastSpaceIndex = textBeforeCursor.lastIndexOf(' ');
    const prefix = lastSpaceIndex >= 0 ? textBeforeCursor.substring(0, lastSpaceIndex + 1) : '';
    
    // If it's a value suggestion, wrap it in quotes
    const suggestionText = this.suggestionType === 'value' ? `"${suggestion}"` : suggestion;
    
    // Add appropriate spacing
    const suffix = this.suggestionType === 'value' ? '' : ' ';
    this.searchText = prefix + suggestionText + suffix + textAfterCursor;
    
    // Update cursor position
    const newCursorPosition = prefix.length + suggestionText.length + suffix.length;
    setTimeout(() => {
      input.setSelectionRange(newCursorPosition, newCursorPosition);
      input.focus();
    });

    // Hide suggestions temporarily
    this.showSuggestions = false;
    
    // Update suggestions after a brief delay to allow for the next context
    setTimeout(() => {
      this.updateSuggestions();
    }, 50);
  }

  parseAndAddCriteria(): void {
    console.log('Attempting to parse criteria from:', this.searchText);
    
    if (!this.searchText.trim()) {
      console.log('Search text is empty, skipping parse');
      return;
    }
    
    try {
      console.log(this.searchableFields);
      const criteria = this.searchParser.parseQuery(this.searchText.trim(), this.searchableFields);
      console.log('Parsed criteria:', criteria);
      
      if (criteria && criteria.length > 0) {
        this.activeCriteria = [...this.activeCriteria, ...criteria];
        console.log('Updated active criteria:', this.activeCriteria);
        this.searchChange.emit({ criteria: this.activeCriteria });
        this.searchText = '';
        this.showSuggestions = false;
        this.selectedSuggestionIndex = -1;
      } else {
        console.log('No valid criteria parsed from:', this.searchText);
      }
    } catch (error) {
      console.error('Error parsing criteria:', error);
      console.error('Search text that caused error:', this.searchText);
    }
  }

  onBlur(event: FocusEvent): void {
    // Check if the related target is within the suggestions container
    if (!event.relatedTarget || 
        !this.suggestionsContainer?.nativeElement.contains(event.relatedTarget)) {
      setTimeout(() => {
        this.showSuggestions = false;
      }, 200);
    }
  }

  removeCriterion(criterion: SearchCriterion): void {
    this.activeCriteria = this.activeCriteria.filter(c => c !== criterion);
    this.searchChange.emit({ criteria: this.activeCriteria });
  }

  getCriterionLabel(criterion: SearchCriterion): string {
    const field = this.searchableFields.find(f => f.field === criterion.field);
    const fieldLabel = field ? this.formatFieldLabel(field) : criterion.field;
    const operatorLabel = this.getOperatorLabel(criterion.operator);
    return `${fieldLabel} ${operatorLabel} ${criterion.value}`;
  }
} 