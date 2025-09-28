import { Component, Input, forwardRef, signal, OnInit } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, NG_VALIDATORS, Validator, AbstractControl, ValidationErrors, FormControl, ReactiveFormsModule } from '@angular/forms';
import { parsePhoneNumber, isValidPhoneNumber, isPossiblePhoneNumber } from 'libphonenumber-js';
import { CommonModule } from '@angular/common';
import { InputTextModule } from 'primeng/inputtext';
import { ChipModule } from 'primeng/chip';
import { ButtonModule } from 'primeng/button';

export interface PhoneNumber {
  formatted: string;
  countryCode: string;
}

@Component({
  selector: 'app-phone-input',
  templateUrl: './phone-input.component.html',
  styleUrl: './phone-input.component.scss',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    InputTextModule,
    ChipModule,
    ButtonModule
  ],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => PhoneInputComponent),
      multi: true
    },
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => PhoneInputComponent),
      multi: true
    }
  ]
})
export class PhoneInputComponent implements ControlValueAccessor, Validator, OnInit {
  @Input() placeholder: string = '';
  @Input() label: string = '';
  @Input() helperText: string = 'You can paste multiple numbers';
  @Input() required: boolean = false;
  @Input() disabled: boolean = false;
  @Input() defaultCountry: string = 'DK';
  @Input() showClear: boolean = true;
  @Input() showAddButton: boolean = false;
  @Input() minPhones: number = 0;
  @Input() maxPhones: number = 100;
  @Input() allowDuplicates: boolean = false;
  @Input() autoProcess: boolean = true;
  @Input() requiredMessage: string = '';
  @Input() invalidPhoneMessage: string = '';

  phoneControl = new FormControl('');

  phoneNumbers = signal<PhoneNumber[]>([]);
  inputValue = signal<string>('');
  touched = signal<boolean>(false);
  hasErrors = signal<boolean>(false);
  validationErrors = signal<ValidationErrors | null>(null);
  hasInvalidNumbers = signal<boolean>(false);
  invalidNumbersInInput = signal<PhoneNumber[]>([]);
  inputId = `phone-input-${Math.random().toString(36).substr(2, 9)}`;

  private onChange = (value: string[]) => {};
  private onTouched = () => {};

  ngOnInit() {
    this.setupFormControls();
  }

  private setupFormControls() {
    this.phoneControl.valueChanges.subscribe(value => {
      this.inputValue.set(value || '');

      // Reset invalid state when user starts typing new content
      // Only reset if the current input doesn't match the invalid numbers
      const currentInvalidInputs = this.invalidNumbersInInput().map(p => p.formatted).join(', ');
      if (this.hasInvalidNumbers() && value !== currentInvalidInputs) {
        this.hasInvalidNumbers.set(false);
        this.invalidNumbersInInput.set([]);
      }
    });
  }

  // ControlValueAccessor implementation
  writeValue(value: string[]): void {
    if (value && Array.isArray(value)) {
      const parsedPhones = value.map(phone => this.parsePhone(phone));
      const cleanedPhones = this.removeDuplicates(parsedPhones);
      this.phoneNumbers.set(cleanedPhones);
    } else {
      this.phoneNumbers.set([]);
    }
  }

  registerOnChange(fn: (value: string[]) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
    if (isDisabled) {
      this.phoneControl.disable({ emitEvent: false });
    } else {
      this.phoneControl.enable({ emitEvent: false });
    }
  }

  onBlur(): void {
    this.touched.set(true);
    this.onTouched();

    if (this.autoProcess && this.inputValue().trim()) {
      this.processInput();
    }
  }

  onPaste(event: ClipboardEvent): void {
    if (this.autoProcess) {
      setTimeout(() => this.processInput(), 100);
    }
  }

  onEnterKey(event: KeyboardEvent): void {
    if (!event.shiftKey) {
      event.preventDefault();
      this.processInput();
    }
  }

  processInput(): void {
    const input = this.inputValue().trim();
    if (!input) return;

    const newNumbers = this.parseMultiplePhones(input);
    const currentPhones = [...this.phoneNumbers()];
    const invalidNumbers: PhoneNumber[] = [];
    const validNumbers: PhoneNumber[] = [];

    // Separate possible and impossible numbers using libphonenumber-js
    newNumbers.forEach(phone => {
      const isPossible = isPossiblePhoneNumber(phone.formatted);

      if (!isPossible) {
        invalidNumbers.push(phone);
      } else if (!this.allowDuplicates && this.isDuplicate(phone, currentPhones)) {
        // Skip duplicates but don't mark as invalid
        console.log(`Skipping duplicate: ${phone.formatted}`);
        return;
      } else if (!this.allowDuplicates && validNumbers.some(existing => this.isDuplicate(phone, [existing]))) {
        // Also check for duplicates within the new numbers being added
        console.log(`Skipping duplicate within new numbers: ${phone.formatted}`);
        return;
      } else {
        validNumbers.push(phone);
      }
    });

    if (invalidNumbers.length > 0) {
      // Keep invalid numbers in the input field
      this.hasInvalidNumbers.set(true);
      this.invalidNumbersInInput.set(invalidNumbers);

      // Update input with invalid numbers only
      const invalidInputs = invalidNumbers.map(phone => phone.formatted).join(', ');
      this.phoneControl.setValue(invalidInputs, { emitEvent: false });
      this.inputValue.set(invalidInputs);

      // Add only valid numbers to the list
      validNumbers.forEach(phone => currentPhones.push(phone));
      this.phoneNumbers.set(currentPhones);

      // Trigger validation to show error
      this.updateValue();
    } else {
      // All numbers are valid, clear input and add to list
      this.hasInvalidNumbers.set(false);
      this.invalidNumbersInInput.set([]);

      validNumbers.forEach(phone => currentPhones.push(phone));
      this.phoneNumbers.set(currentPhones);

      this.phoneControl.setValue('', { emitEvent: false });
      this.inputValue.set('');
      this.updateValue();
    }
  }

  parseMultiplePhones(input: string): PhoneNumber[] {
    // Improved regex patterns with better boundaries and precision
    const phonePatterns = [
      // International format with + (more permissive to capture full numbers with spaces)
      /\+[\d\s\-\.]{10,20}/g,
      // Parentheses format (XXX) XXX-XXXX (more precise)
      /\(\d{2,4}\)[\s\-\.]?\d{2,4}(?:[\s\-\.]\d{2,6})?/g,
      // Separated formats with consistent separators
      /\d{2,4}[\s\-\.]\d{2,4}[\s\-\.]\d{2,6}(?:[\s\-\.]\d{1,4})*/g,
      // Long digit sequences (8-15 digits) with word boundaries
      /(?<!\d)\d{8,15}(?!\d)/g
    ];

    const foundNumbers = new Map<string, number>(); // Track position for consistency

    // Extract numbers using regex patterns in order of preference
    for (let patternIndex = 0; patternIndex < phonePatterns.length; patternIndex++) {
      const pattern = phonePatterns[patternIndex];
      const matches = Array.from(input.matchAll(pattern));

      matches.forEach(match => {
        const candidate = match[0].trim();
        const position = match.index || 0;

        // Only add if not already found by a higher-priority pattern
        if (candidate.length >= 7 && !Array.from(foundNumbers.keys()).some(existing =>
          this.numbersOverlap(existing, candidate, input))) {
          foundNumbers.set(candidate, position);
        }
      });
    }

    // Fallback to splitting method if no patterns matched
    if (foundNumbers.size === 0) {
      const splitNumbers = input.split(/[,;\n\r\t]+/)
        .map(n => n.trim())
        .filter(n => n.length > 0);
      splitNumbers.forEach((num, index) => foundNumbers.set(num, index));
    }

    // Sort by position in original input to maintain order
    const sortedNumbers = Array.from(foundNumbers.entries())
      .sort((a, b) => a[1] - b[1])
      .map(([number]) => number);

    // Return all numbers, both valid and invalid
    return sortedNumbers.map(num => this.parsePhone(num));
  }

  private numbersOverlap(existing: string, candidate: string, originalInput: string): boolean {
    const existingIndex = originalInput.indexOf(existing);
    const candidateIndex = originalInput.indexOf(candidate);

    if (existingIndex === -1 || candidateIndex === -1) return false;

    const existingEnd = existingIndex + existing.length;
    const candidateEnd = candidateIndex + candidate.length;

    // Check if they overlap in the original input
    return !(existingEnd <= candidateIndex || candidateEnd <= existingIndex);
  }

  parsePhone(phone: string): PhoneNumber {
    const cleanedInput = phone.trim();

    // List of countries to try for parsing (most common first)
    const countriesToTry = [this.defaultCountry, 'US', 'CA', 'GB', 'DE', 'IT', 'ES'];

    // Case 1: International number with + (CERTAIN of country)
    if (cleanedInput.startsWith('+')) {
      try {
        const parsed = parsePhoneNumber(cleanedInput);
        if (parsed) {
          return {
            formatted: parsed.formatInternational(),
            countryCode: parsed.country || ''
          };
        }
      } catch (error) {
        // Continue to try with country codes
      }
    }

    // Case 2: Try with default country first (CERTAIN if matches)
    try {
      const parsed = parsePhoneNumber(cleanedInput, this.defaultCountry as any);
      if (parsed && parsed.isPossible()) {
        return {
          formatted: parsed.formatInternational(),
          countryCode: parsed.country || this.defaultCountry
        };
      }
    } catch (error) {
      // Continue to try other countries
    }

    // Case 3: Try with other countries (UNCERTAIN - could be any of them)
    const otherCountries = countriesToTry.filter(c => c !== this.defaultCountry);
    for (const country of otherCountries) {
      try {
        const parsed = parsePhoneNumber(cleanedInput, country as any);
        if (parsed && parsed.isPossible()) {
          return {
            formatted: parsed.formatInternational(),
            countryCode: parsed.country || country
          };
        }
      } catch (error) {
        // Try next country
        continue;
      }
    }

    // Case 4: Final fallback - possibly valid but can't determine country (UNCERTAIN)
    try {
      if (isPossiblePhoneNumber(cleanedInput)) {
        return {
          formatted: cleanedInput, // Keep original format as fallback
          countryCode: ''
        };
      }
    } catch (error) {
      // Final fallback failed
    }

    // Case 5: Completely invalid
    return {
      formatted: cleanedInput, // Use cleaned input for consistency
      countryCode: ''
    };
  }

  isDuplicate(phone: PhoneNumber, phoneList: PhoneNumber[]): boolean {
    return phoneList.some(p =>
      p.formatted === phone.formatted ||
      (p.countryCode === phone.countryCode && p.formatted === phone.formatted)
    );
  }

  removePhone(index: number, event?: Event): void {
    // Stop event propagation to prevent triggering parent click handlers
    if (event) {
      event.stopPropagation();
      event.preventDefault();
    }

    const phones = [...this.phoneNumbers()];
    phones.splice(index, 1);
    this.phoneNumbers.set(phones);
    this.updateValue();
  }

  editPhone(index: number): void {
    if (this.disabled) return;

    const phones = [...this.phoneNumbers()];
    const phoneToEdit = phones[index];

    if (phoneToEdit) {

      // If re-parsing doesn't give the same formatted result, use the formatted version instead
      const inputToUse = phoneToEdit.formatted;

      // Remove the phone from the list
      phones.splice(index, 1);
      this.phoneNumbers.set(phones);

      // Put the validated input value back in the input for editing
      this.phoneControl.setValue(inputToUse, { emitEvent: false });
      this.inputValue.set(inputToUse);

      // Update the form value
      this.updateValue();

      // Focus the input field
      setTimeout(() => {
        const inputElement = document.getElementById(this.inputId);
        if (inputElement) {
          inputElement.focus();
        }
      }, 100);
    }
  }

  clearInput(): void {
    this.phoneControl.setValue('', { emitEvent: false });
    this.inputValue.set('');
    this.hasInvalidNumbers.set(false);
    this.invalidNumbersInInput.set([]);
  }

  clearAll(): void {
    this.phoneNumbers.set([]);
    this.clearInput();
    this.updateValue();
  }

  private updateValue(): void {
    // All stored phone numbers are already validated as possible
    const values = this.phoneNumbers().map(phone => phone.formatted);

    this.onChange(values);
    this.validateMultiple(values);
  }

  // Validator implementation
  validate(control: AbstractControl): ValidationErrors | null {
    return this.validateMultiple(control.value);
  }

  private validateMultiple(value: string[]): ValidationErrors | null {
    const errors: ValidationErrors = {};
    const phoneCount = value ? value.length : 0;
    const phones = this.phoneNumbers();
    const invalidNumbers = this.invalidNumbersInInput();

    // Required validation
    if (this.minPhones > 0 && phoneCount < this.minPhones) {
      if (this.minPhones === 1) {
        errors['required'] = true;
      } else {
        errors['minPhones'] = { min: this.minPhones, actual: phoneCount };
      }
    }

    // Max phones validation
    if (phoneCount > this.maxPhones) {
      errors['maxPhones'] = { max: this.maxPhones, actual: phoneCount };
    }

    // Invalid phone validation - only check invalid numbers in input (stored phones are already validated as possible)
    if (invalidNumbers.length > 0) {
      errors['invalidPhone'] = true;
    }

    const validationErrors = Object.keys(errors).length > 0 ? errors : null;
    this.validationErrors.set(validationErrors);
    this.hasErrors.set(validationErrors !== null);

    return validationErrors;
  }

  getCountryFlagEmoji(phone: PhoneNumber): string {
    // Special case for +1 numbers (US/Canada/others) - use earth emoji
    if (phone.formatted.startsWith('+1')) {
      return '🌍';
    }

    // Show flag emoji if we have a valid 2-letter country code
    if (phone.countryCode && phone.countryCode.length === 2) {
      try {
        // Convert country code to flag emoji
        const codePoints = phone.countryCode
          .toUpperCase()
          .split('')
          .map(char => 127397 + char.charCodeAt(0));

        const flagEmoji = String.fromCodePoint(...codePoints);

        // Basic validation - ensure the emoji was created successfully
        if (flagEmoji && flagEmoji.length > 0) {
          return flagEmoji;
        }
      } catch (error) {
        console.warn(`Failed to generate flag emoji for country code: ${phone.countryCode}`, error);
      }
    }

    // Fallback to world emoji for unknown countries or errors
    return '🌍';
  }

  getInvalidNumbersText(): string {
    return this.invalidNumbersInInput().map(p => p.formatted).join(', ');
  }

  private removeDuplicates(phones: PhoneNumber[]): PhoneNumber[] {
    if (this.allowDuplicates) return phones;

    const uniquePhones: PhoneNumber[] = [];
    phones.forEach(phone => {
      if (!this.isDuplicate(phone, uniquePhones)) {
        uniquePhones.push(phone);
      }
    });

    return uniquePhones;
  }
}
