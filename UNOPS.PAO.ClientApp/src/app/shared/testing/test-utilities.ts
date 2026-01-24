/**
 * @fileoverview Centralized test utilities and mock services for Angular tests
 * @author UNOPS Opportunity+ System Development Team
 */

import { EventEmitter } from '@angular/core';
import { of, Subject } from 'rxjs';

/**
 * @description Mock TranslateService for testing components that use i18n
 * Provides all required methods to prevent "translate.get is not a function" errors
 */
export const createMockTranslateService = () => ({
  get: jasmine.createSpy('get').and.returnValue(of('translated text')),
  instant: jasmine.createSpy('instant').and.returnValue('translated text'),
  onLangChange: new EventEmitter(),
  onTranslationChange: new EventEmitter(),
  onDefaultLangChange: new EventEmitter(),
  currentLang: 'en',
  defaultLang: 'en',
  use: jasmine.createSpy('use').and.returnValue(of({})),
  stream: jasmine.createSpy('stream').and.returnValue(of('translated text')),
  getBrowserLang: jasmine.createSpy('getBrowserLang').and.returnValue('en'),
  setDefaultLang: jasmine.createSpy('setDefaultLang'),
  addLangs: jasmine.createSpy('addLangs'),
  getLangs: jasmine.createSpy('getLangs').and.returnValue(['en', 'fr', 'es', 'pt'])
});

/**
 * @description Mock DialogService for PrimeNG dialog components
 * Prevents "No provider for DialogService" errors
 */
export const createMockDialogService = () => ({
  open: jasmine.createSpy('open').and.returnValue({
    onClose: new Subject(),
    onMaximize: new EventEmitter(),
    onHide: new EventEmitter(),
    onShow: new EventEmitter(),
    close: jasmine.createSpy('close'),
    destroy: jasmine.createSpy('destroy')
  }),
  dialogComponentRefMap: new Map()
});

/**
 * @description Mock MarkdownService for ngx-markdown components
 * Prevents "No provider for MarkdownService" errors
 */
export const createMockMarkdownService = () => ({
  parse: jasmine.createSpy('parse').and.returnValue(of('parsed markdown')),
  compile: jasmine.createSpy('compile').and.returnValue('compiled markdown'),
  render: jasmine.createSpy('render').and.returnValue('rendered markdown'),
  highlight: jasmine.createSpy('highlight'),
  reload: jasmine.createSpy('reload')
});

/**
 * @description Mock ConfirmationService for PrimeNG confirmation dialogs
 */
export const createMockConfirmationService = () => ({
  confirm: jasmine.createSpy('confirm'),
  close: jasmine.createSpy('close')
});

/**
 * @description Mock MessageService for PrimeNG toast messages
 */
export const createMockMessageService = () => ({
  add: jasmine.createSpy('add'),
  addAll: jasmine.createSpy('addAll'),
  clear: jasmine.createSpy('clear')
});

/**
 * @description Common test providers for Angular tests
 * Use this to quickly add all common mocks to TestBed configuration
 */
export const getCommonTestProviders = () => {
  return [
    { provide: 'TranslateService', useValue: createMockTranslateService() },
    { provide: 'DialogService', useValue: createMockDialogService() },
    { provide: 'MarkdownService', useValue: createMockMarkdownService() },
    { provide: 'ConfirmationService', useValue: createMockConfirmationService() },
    { provide: 'MessageService', useValue: createMockMessageService() }
  ];
};
