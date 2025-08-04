import { Injectable, inject } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface MissingTranslation {
  key: string;
  language: string;
  timestamp: Date;
  component?: string;
  context?: string;
}

export interface TranslationStats {
  totalKeys: number;
  missingKeys: number;
  languages: string[];
  completionPercentage: number;
}

export interface LanguageStats {
  language: string;
  totalKeys: number;
  missingKeys: number;
  completionPercentage: number;
  missingKeysList: string[];
}

@Injectable({ 
  providedIn: 'root' 
})
export class TranslationCheckerService {
  private translate = inject(TranslateService);
  
  private missingTranslations = new Map<string, MissingTranslation>();
  private checkedKeys = new Set<string>();
  private isEnabled = true;
  
  // Observable streams for reactive updates
  private missingTranslationsSubject = new BehaviorSubject<MissingTranslation[]>([]);
  private statsSubject = new BehaviorSubject<TranslationStats | null>(null);
  
  public readonly missingTranslations$ = this.missingTranslationsSubject.asObservable();
  public readonly stats$ = this.statsSubject.asObservable();
  
  // Supported languages from your language service
  private readonly supportedLanguages = ['en', 'fr', 'span', 'pt'];
  private readonly referenceLanguage = 'en';

  constructor() {
    this.initializeTranslationMonitoring();
  }

  /**
   * Initialize translation monitoring and missing key detection
   */
  private initializeTranslationMonitoring(): void {
    // Only enable in development mode
    this.isEnabled = !this.isProduction();
    
    if (!this.isEnabled) {
      console.log('🔍 TranslationChecker: Disabled in production mode');
      return;
    }

    console.log('🔍 TranslationChecker: Monitoring enabled for development');

    // Listen for missing translation keys
    // Note: onMissingKey is not available in current ngx-translate version
    // Manual checking will be used instead via checkKey() method

    // Listen for language changes to validate translations
    this.translate.onLangChange.subscribe((event) => {
      console.log(`🌐 TranslationChecker: Language changed to ${event.lang}`);
      this.validateCurrentLanguageTranslations();
    });
  }

  /**
   * Record a missing translation
   */
  private recordMissingTranslation(key: string, context?: string): void {
    if (!this.isEnabled) return;

    const currentLang = this.translate.currentLang;
    const translationKey = `${key}_${currentLang}`;
    
    // Avoid duplicate entries
    if (this.missingTranslations.has(translationKey)) {
      return;
    }

    const missingTranslation: MissingTranslation = {
      key,
      language: currentLang,
      timestamp: new Date(),
      context
    };

    this.missingTranslations.set(translationKey, missingTranslation);
    
    // Log missing translation
    console.warn(`🚨 TranslationChecker: Missing translation for "${key}" in language "${currentLang}"`);
    
    // Update observable
    this.updateMissingTranslationsSubject();
  }

  /**
   * Check if a specific key exists in all languages
   */
  public checkKeyInAllLanguages(key: string): Observable<LanguageStats[]> {
    return new Observable(observer => {
      const results: LanguageStats[] = [];
      const currentLang = this.translate.currentLang;
      let processed = 0;

      for (const lang of this.supportedLanguages) {
        this.translate.use(lang).subscribe(() => {
          const translation = this.translate.instant(key);
          const langTranslations = this.translate.store.translations[lang] || {};
          const isMissing = !this.hasNestedKey(langTranslations, key); // Check if key actually exists in translations

          const stats: LanguageStats = {
            language: lang,
            totalKeys: 1,
            missingKeys: isMissing ? 1 : 0,
            completionPercentage: isMissing ? 0 : 100,
            missingKeysList: isMissing ? [key] : []
          };

          results.push(stats);
          processed++;

          if (processed === this.supportedLanguages.length) {
            // Restore original language
            this.translate.use(currentLang);
            observer.next(results);
            observer.complete();
          }
        });
      }
    });
  }

  /**
   * Validate translations for current language
   */
  public validateCurrentLanguageTranslations(): void {
    if (!this.isEnabled) return;

    const currentLang = this.translate.currentLang;
    const translations = this.translate.store.translations[currentLang] || {};
    
    this.validateTranslationObject(translations, currentLang);
  }

  /**
   * Recursively validate translation object
   */
  private validateTranslationObject(obj: any, language: string, prefix: string = ''): void {
    for (const key in obj) {
      const fullKey = prefix ? `${prefix}.${key}` : key;
      
      if (typeof obj[key] === 'object' && obj[key] !== null) {
        this.validateTranslationObject(obj[key], language, fullKey);
      } else {
        // For reference language, we don't need to check against itself
        // The key exists if we're iterating over it
        // No need to record missing translations during validation of existing keys
      }
    }
  }

  /**
   * Perform comprehensive check across all languages
   */
  public performComprehensiveCheck(): Observable<LanguageStats[]> {
    return new Observable(observer => {
      if (!this.isEnabled) {
        observer.next([]);
        observer.complete();
        return;
      }

      const currentLang = this.translate.currentLang;
      const results: LanguageStats[] = [];
      let processed = 0;

      // Get reference language translations as baseline
      this.translate.use(this.referenceLanguage).subscribe(() => {
        const referenceTranslations = this.translate.store.translations[this.referenceLanguage] || {};
        const allKeys = this.getAllKeysFromObject(referenceTranslations);

        console.log(`🔍 TranslationChecker: Found ${allKeys.length} keys in reference language (${this.referenceLanguage})`);

        // Check each language against reference
        for (const lang of this.supportedLanguages) {
          this.translate.use(lang).subscribe(() => {
            const langTranslations = this.translate.store.translations[lang] || {};
            const langKeys = this.getAllKeysFromObject(langTranslations);
            const missingKeys = allKeys.filter(key => !this.hasNestedKey(langTranslations, key));

            const stats: LanguageStats = {
              language: lang,
              totalKeys: allKeys.length,
              missingKeys: missingKeys.length,
              completionPercentage: ((allKeys.length - missingKeys.length) / allKeys.length) * 100,
              missingKeysList: missingKeys
            };

            results.push(stats);
            processed++;

            // Log missing keys for non-reference languages
            if (lang !== this.referenceLanguage && missingKeys.length > 0) {
              console.warn(`🚨 TranslationChecker: ${lang} is missing ${missingKeys.length} translations`);
              missingKeys.slice(0, 5).forEach(key => {
                console.warn(`   - ${key}`);
              });
              if (missingKeys.length > 5) {
                console.warn(`   ... and ${missingKeys.length - 5} more`);
              }
            }

            if (processed === this.supportedLanguages.length) {
              // Restore original language
              this.translate.use(currentLang);
              
              // Update stats
              const overallStats: TranslationStats = {
                totalKeys: allKeys.length,
                missingKeys: results.reduce((sum, stat) => sum + stat.missingKeys, 0),
                languages: this.supportedLanguages,
                completionPercentage: results.reduce((sum, stat) => sum + stat.completionPercentage, 0) / results.length
              };
              
              this.statsSubject.next(overallStats);
              
              observer.next(results);
              observer.complete();
            }
          });
        }
      });
    });
  }

  /**
   * Get all keys from nested translation object
   */
  private getAllKeysFromObject(obj: any, prefix: string = ''): string[] {
    let keys: string[] = [];
    
    for (const key in obj) {
      const fullKey = prefix ? `${prefix}.${key}` : key;
      
      if (typeof obj[key] === 'object' && obj[key] !== null) {
        keys = keys.concat(this.getAllKeysFromObject(obj[key], fullKey));
      } else {
        keys.push(fullKey);
      }
    }
    
    return keys;
  }

  /**
   * Check if nested key exists in object
   */
  private hasNestedKey(obj: any, key: string): boolean {
    const keys = key.split('.');
    let current = obj;
    
    for (const k of keys) {
      if (current && typeof current === 'object' && k in current) {
        current = current[k];
      } else {
        return false;
      }
    }
    
    return current !== undefined;
  }

  /**
   * Get current missing translations
   */
  public getMissingTranslations(): MissingTranslation[] {
    return Array.from(this.missingTranslations.values());
  }

  /**
   * Clear all recorded missing translations
   */
  public clearMissingTranslations(): void {
    this.missingTranslations.clear();
    this.updateMissingTranslationsSubject();
    console.log('🧹 TranslationChecker: Cleared all missing translation records');
  }

  /**
   * Enable or disable translation checking
   */
  public setEnabled(enabled: boolean): void {
    this.isEnabled = enabled && !this.isProduction();
    console.log(`🔍 TranslationChecker: ${this.isEnabled ? 'Enabled' : 'Disabled'}`);
  }

  /**
   * Check if running in production
   */
  private isProduction(): boolean {
    // You can customize this based on your build configuration
    const hostname = (window as any).location.hostname;
    return hostname !== 'localhost' && 
           hostname !== '127.0.0.1' &&
           !hostname.includes('dev');
  }

  /**
   * Update missing translations observable
   */
  private updateMissingTranslationsSubject(): void {
    this.missingTranslationsSubject.next(Array.from(this.missingTranslations.values()));
  }

  /**
   * Get summary statistics
   */
  public getSummaryStats(): Observable<TranslationStats | null> {
    return this.stats$;
  }

  /**
   * Manual trigger to check a specific key
   */
  public checkKey(key: string, context?: string): boolean {
    if (!this.isEnabled) return true;

    const translation = this.translate.instant(key);
    const currentLang = this.translate.currentLang;
    const langTranslations = this.translate.store.translations[currentLang] || {};
    const isMissing = !this.hasNestedKey(langTranslations, key);
    
    if (isMissing) {
      this.recordMissingTranslation(key, context);
    }
    
    this.checkedKeys.add(key);
    return !isMissing;
  }

  /**
   * Log translation checker statistics to console
   */
  public logStatistics(): void {
    if (!this.isEnabled) {
      console.log('🔍 TranslationChecker: Statistics unavailable (checker disabled)');
      return;
    }

    const missing = this.getMissingTranslations();
    console.group('🔍 TranslationChecker Statistics');
    console.log(`Total missing translations recorded: ${missing.length}`);
    console.log(`Total keys checked: ${this.checkedKeys.size}`);
    console.log(`Current language: ${this.translate.currentLang}`);
    console.log(`Supported languages: ${this.supportedLanguages.join(', ')}`);
    
    if (missing.length > 0) {
      console.group('Missing translations by language:');
      const byLanguage = missing.reduce((acc, item) => {
        acc[item.language] = (acc[item.language] || 0) + 1;
        return acc;
      }, {} as Record<string, number>);
      
      Object.entries(byLanguage).forEach(([lang, count]) => {
        console.log(`${lang}: ${count} missing`);
      });
      console.groupEnd();
    }
    
    console.groupEnd();
  }
} 