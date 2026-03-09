import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AiAssistantPanelComponent } from './ai-assistant-panel.component';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { TranslateModule } from '@ngx-translate/core';
import { Router } from '@angular/router';
import { of, Subject } from 'rxjs';
import { signal } from '@angular/core';
import { AiAssistantService } from '@ai/services/ai-assistant.service';
import { GlobalFilterService } from '@core/services/filters';
import { AuthService } from '@core/services/auth';
import { PageContextService } from '@shared/services/utils/page-context.service';
import { DrivePickerService } from '@shared/services/integration/drive-picker.service';
import { GoogleDriveService } from '@shared/services/google-drive.service';
import { LayoutService } from '@layouts/services/layout.service';
import { DynamicContentService } from './dynamic-content.service';

describe('AiAssistantPanelComponent', () => {
  let component: AiAssistantPanelComponent;
  let fixture: ComponentFixture<AiAssistantPanelComponent>;

  beforeEach(async () => {
    const mockAiAssistantService = {
      currentChatSession: signal(null),
      userSessions: signal([]),
      isLoading: signal(false),
      isLoadingSession: signal(false),
      isLoadingSessions: signal(false),
      currentSessionId: signal(null),
      sessionTitle: signal(''),
      sessionStarred: signal(false),
      sessionArchived: signal(false),
      isFirstPageLoad: signal(true),
      loadUserSessions: jasmine.createSpy('loadUserSessions').and.returnValue(of(undefined)),
      chatHistoryChanged$: new Subject<void>(),
      streamingChunk$: of(null),
      setViewContainerRef: jasmine.createSpy('setViewContainerRef'),
      sendMessage: jasmine.createSpy('sendMessage'),
      sendMessageToServer: jasmine.createSpy('sendMessageToServer').and.returnValue(of(undefined)),
      emitChatHistoryChanged: jasmine.createSpy('emitChatHistoryChanged'),
      updateTitle: jasmine.createSpy('updateTitle').and.returnValue(of(undefined)),
      toggleStar: jasmine.createSpy('toggleStar').and.returnValue(of(undefined)),
      toggleArchive: jasmine.createSpy('toggleArchive').and.returnValue(of(undefined)),
      switchToSession: jasmine.createSpy('switchToSession').and.returnValue(of(undefined)),
      clearConversation: jasmine.createSpy('clearConversation')
    };

    const mockDynamicContentService = {
      createComponent: () => {},
      clearAllComponents: () => {},
      setViewContainer: () => {},
      setViewContainerRef: () => {},
      setCardClickCallback: () => {},
      processChunk: () => {}
    };

    await TestBed.configureTestingModule({
      imports: [
        AiAssistantPanelComponent,
        HttpClientTestingModule,
        TranslateModule.forRoot()
      ],
      providers: [
        { provide: Router, useValue: { navigate: jasmine.createSpy('navigate'), url: '/dashboard' } },
        { provide: AiAssistantService, useValue: mockAiAssistantService },
        { provide: GlobalFilterService, useValue: { filters: of({}), isFilterEnabled: () => false, getActiveOrgUnitId: () => null } },
        { provide: AuthService, useValue: { user: () => of([]) } },
        { provide: PageContextService, useValue: { getContext: () => null, getPageContextForAI: () => null } },
        { provide: DrivePickerService, useValue: { pickFiles: () => of([]), openPicker: () => of([]) } },
        { provide: GoogleDriveService, useValue: { initializeAuth: () => of(false), uploadFile: () => of({}) } },
        { provide: LayoutService, useValue: { onMenuToggle: () => {}, onAIAssistantToggle: () => {}, layoutState: { update: () => {} }, isMobile: () => false } },
        { provide: DynamicContentService, useValue: mockDynamicContentService }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AiAssistantPanelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

