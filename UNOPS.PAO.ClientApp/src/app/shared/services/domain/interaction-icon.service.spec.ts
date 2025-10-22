import { TestBed } from '@angular/core/testing';
import { InteractionIconService } from './interaction-icon.service';
import { InteractionType } from '@partnerships/interactions/models/interaction-type.enum';

describe('InteractionIconService', () => {
  let service: InteractionIconService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [InteractionIconService]
    });

    service = TestBed.inject(InteractionIconService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getInteractionIconInfo', () => {
    it('should return email icon info', () => {
      const info = service.getInteractionIconInfo(InteractionType.Email);
      expect(info.icon).toBe('pi pi-envelope');
      expect(info.materialIcon).toBe('mail');
      expect(info.color).toBe('#8b5cf6');
      expect(info.bgColor).toBe('bg-purple-50');
    });

    it('should return call icon info', () => {
      const info = service.getInteractionIconInfo(InteractionType.Call);
      expect(info.icon).toBe('pi pi-phone');
      expect(info.materialIcon).toBe('phone');
      expect(info.color).toBe('#10b981');
    });

    it('should return chat icon info', () => {
      const info = service.getInteractionIconInfo(InteractionType.Chat);
      expect(info.icon).toBe('pi pi-comments');
      expect(info.materialIcon).toBe('chat');
    });

    it('should return virtual meeting icon info', () => {
      const info = service.getInteractionIconInfo(InteractionType.VirtualMeeting);
      expect(info.icon).toBe('pi pi-video');
      expect(info.materialIcon).toBe('videocam');
    });

    it('should return in-person meeting icon info', () => {
      const info = service.getInteractionIconInfo(InteractionType.InPersonMeeting);
      expect(info.icon).toBe('pi pi-users');
      expect(info.materialIcon).toBe('group');
    });

    it('should handle note type', () => {
      const info = service.getInteractionIconInfo('note');
      expect(info.icon).toBe('pi pi-file-edit');
      expect(info.materialIcon).toBe('note');
    });

    it('should handle task type', () => {
      const info = service.getInteractionIconInfo('task');
      expect(info.icon).toBe('pi pi-check-square');
      expect(info.materialIcon).toBe('task_alt');
    });

    it('should handle appointment type', () => {
      const info = service.getInteractionIconInfo('appointment');
      expect(info.icon).toBe('pi pi-calendar');
      expect(info.materialIcon).toBe('event');
    });

    it('should handle null type', () => {
      const info = service.getInteractionIconInfo(null);
      expect(info.icon).toBe('pi pi-question-circle');
      expect(info.materialIcon).toBe('help');
    });

    it('should handle undefined type', () => {
      const info = service.getInteractionIconInfo(undefined);
      expect(info.icon).toBe('pi pi-question-circle');
    });

    it('should handle unknown type', () => {
      const info = service.getInteractionIconInfo('unknown-type');
      expect(info.icon).toBe('pi pi-question-circle');
      expect(info.color).toBe('#6b7280');
    });

    it('should be case insensitive', () => {
      const infoLower = service.getInteractionIconInfo('email');
      const infoUpper = service.getInteractionIconInfo('EMAIL');
      const infoMixed = service.getInteractionIconInfo('EmAiL');
      
      expect(infoLower.icon).toBe(infoUpper.icon);
      expect(infoLower.icon).toBe(infoMixed.icon);
    });

    it('should handle legacy "meeting" type', () => {
      const info = service.getInteractionIconInfo('meeting');
      expect(info.icon).toBe('pi pi-users');
    });

    it('should handle legacy "video call" type', () => {
      const info = service.getInteractionIconInfo('video call');
      expect(info.icon).toBe('pi pi-video');
    });
  });

  describe('helper methods', () => {
    it('should get interaction icon', () => {
      const icon = service.getInteractionIcon(InteractionType.Email);
      expect(icon).toBe('pi pi-envelope');
    });

    it('should get interaction material icon', () => {
      const icon = service.getInteractionMaterialIcon(InteractionType.Call);
      expect(icon).toBe('phone');
    });

    it('should get interaction material icon filled', () => {
      const icon = service.getInteractionMaterialIconFilled(InteractionType.Chat);
      expect(icon).toBe('chat_bubble');
    });

    it('should get interaction gradient', () => {
      const gradient = service.getInteractionGradient(InteractionType.Email);
      expect(gradient).toContain('linear-gradient');
      expect(gradient).toContain('135deg');
    });

    it('should get interaction shadow color', () => {
      const shadow = service.getInteractionShadowColor(InteractionType.Call);
      expect(shadow).toContain('rgba');
    });

    it('should get interaction color', () => {
      const color = service.getInteractionColor(InteractionType.Email);
      expect(color).toBe('#8b5cf6');
    });

    it('should get interaction bg color', () => {
      const bgColor = service.getInteractionBgColor(InteractionType.Call);
      expect(bgColor).toBe('bg-green-50');
    });

    it('should get interaction text color', () => {
      const textColor = service.getInteractionTextColor(InteractionType.Chat);
      expect(textColor).toBe('text-cyan-800');
    });
  });

  describe('getInteractionIconHtml', () => {
    it('should generate HTML with icon and type', () => {
      const html = service.getInteractionIconHtml(InteractionType.Email);
      expect(html).toContain('material-symbols-outlined');
      expect(html).toContain('mail');
      expect(html).toContain(InteractionType.Email);
    });

    it('should include color in HTML', () => {
      const html = service.getInteractionIconHtml(InteractionType.Call);
      expect(html).toContain('#10b981');
    });
  });

  describe('getAllInteractionTypes', () => {
    it('should return all interaction types', () => {
      const types = service.getAllInteractionTypes();
      expect(types.length).toBe(5);
      expect(types.map(t => t.type)).toContain(InteractionType.Email);
      expect(types.map(t => t.type)).toContain(InteractionType.Call);
      expect(types.map(t => t.type)).toContain(InteractionType.Chat);
      expect(types.map(t => t.type)).toContain(InteractionType.VirtualMeeting);
      expect(types.map(t => t.type)).toContain(InteractionType.InPersonMeeting);
    });

    it('should include icon info for each type', () => {
      const types = service.getAllInteractionTypes();
      types.forEach(type => {
        expect(type.info).toBeDefined();
        expect(type.info.icon).toBeDefined();
        expect(type.info.color).toBeDefined();
      });
    });
  });
});

