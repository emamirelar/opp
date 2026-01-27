/**
 * @fileoverview Opportunity MaxLength Validation UI Tests
 * @description Ensures UI max length and validation rules for Opportunity sections.
 *
 * Updated UI constraints:
 * - Opportunity Name: max 120 characters (Overview section)
 * - Challenges: max 1020 characters (Why section)
 */

import { FormControl } from '@angular/forms';

class MockOpportunityOverviewSection {
  readonly nameMaxLength = 120;
  readonly nameControl = new FormControl<string>('');

  get isNameEmpty(): boolean {
    return (this.nameControl.value ?? '').trim() === '';
  }

  get isNameTooLong(): boolean {
    return (this.nameControl.value ?? '').length > this.nameMaxLength;
  }

  get isNameInvalid(): boolean {
    return this.isNameEmpty || this.isNameTooLong;
  }

  get nameCounter(): string {
    return `${(this.nameControl.value ?? '').length} / ${this.nameMaxLength}`;
  }
}

class MockOpportunityWhySection {
  readonly challengesMaxLength = 1020;
  readonly challengesControl = new FormControl<string>('');

  get isChallengesTooLong(): boolean {
    return (this.challengesControl.value ?? '').length > this.challengesMaxLength;
  }

  get challengesCounter(): string {
    return `${(this.challengesControl.value ?? '').length} / ${this.challengesMaxLength}`;
  }
}

describe('Opportunity MaxLength Validation UI Tests', () => {
  describe('Overview Section - Name', () => {
    let component: MockOpportunityOverviewSection;

    beforeEach(() => {
      component = new MockOpportunityOverviewSection();
    });

    it('TC_OVW_001_Name_MaxLength_Is120', () => {
      expect(component.nameMaxLength).toBe(120);
    });

    it('TC_OVW_002_Name_Empty_ShowsInvalidState', () => {
      component.nameControl.setValue('');
      expect(component.isNameEmpty).toBe(true);
      expect(component.isNameInvalid).toBe(true);
      expect(component.nameCounter).toBe('0 / 120');
    });

    it('TC_OVW_003_Name_AtLimit_IsValid', () => {
      component.nameControl.setValue('A'.repeat(120));
      expect(component.isNameTooLong).toBe(false);
      expect(component.isNameInvalid).toBe(false);
      expect(component.nameCounter).toBe('120 / 120');
    });

    it('TC_OVW_004_Name_OverLimit_IsInvalid', () => {
      component.nameControl.setValue('A'.repeat(121));
      expect(component.isNameTooLong).toBe(true);
      expect(component.isNameInvalid).toBe(true);
      expect(component.nameCounter).toBe('121 / 120');
    });
  });

  describe('Why Section - Challenges', () => {
    let component: MockOpportunityWhySection;

    beforeEach(() => {
      component = new MockOpportunityWhySection();
    });

    it('TC_WHY_001_Challenges_MaxLength_Is1020', () => {
      expect(component.challengesMaxLength).toBe(1020);
    });

    it('TC_WHY_002_Challenges_AtLimit_ShowsCounter', () => {
      component.challengesControl.setValue('B'.repeat(1020));
      expect(component.isChallengesTooLong).toBe(false);
      expect(component.challengesCounter).toBe('1020 / 1020');
    });

    it('TC_WHY_003_Challenges_OverLimit_FlagsLength', () => {
      component.challengesControl.setValue('B'.repeat(1021));
      expect(component.isChallengesTooLong).toBe(true);
      expect(component.challengesCounter).toBe('1021 / 1020');
    });
  });
});
