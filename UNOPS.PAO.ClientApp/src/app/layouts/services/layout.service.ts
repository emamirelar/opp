import { Injectable, effect, signal, computed } from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';

export interface layoutConfig {
    preset?: string;
    primary?: string;
    surface?: string | undefined | null;
    menuMode?: string;
}

interface LayoutState {
    staticMenuDesktopInactive?: boolean;
    overlayMenuActive?: boolean;
    configSidebarVisible?: boolean;
    staticMenuMobileActive?: boolean;
    menuHoverActive?: boolean;
    aiAssistantActive?: boolean;
    aiAssistantPanelSize?: number;
}

interface MenuChangeEvent {
    key: string;
    routeEvent?: boolean;
}

@Injectable({
    providedIn: 'root'
})
export class LayoutService {
    private readonly AI_ASSISTANT_ACTIVE_KEY = 'aiAssistantActive';
    private readonly AI_ASSISTANT_PANEL_SIZE_KEY = 'aiAssistantPanelSize';

    _config: layoutConfig = {
        preset: 'UnopsPreset',
        surface: null,
        menuMode: 'static'
    };

    _state: LayoutState = {
        staticMenuDesktopInactive: false,
        overlayMenuActive: false,
        configSidebarVisible: false,
        staticMenuMobileActive: false,
        menuHoverActive: false,
        aiAssistantActive: this.getStoredAiAssistantActive(),
        aiAssistantPanelSize: this.getStoredAiAssistantPanelSize()
    };

    layoutConfig = signal<layoutConfig>(this._config);

    layoutState = signal<LayoutState>(this._state);

    private configUpdateSignal = signal<layoutConfig | undefined>(undefined);

    private overlayOpenSignal = signal<any>(undefined);

    private menuSourceSignal = signal<MenuChangeEvent | undefined>(undefined);

    private resetSourceSignal = signal<boolean>(false);

    menuSource$ = toObservable(this.menuSourceSignal);

    resetSource$ = toObservable(this.resetSourceSignal);

    configUpdate$ = toObservable(this.configUpdateSignal);

    overlayOpen$ = toObservable(this.overlayOpenSignal);

    isSidebarActive = computed(() => this.layoutState().overlayMenuActive || this.layoutState().staticMenuMobileActive);

    getPrimary = computed(() => this.layoutConfig().primary);

    getSurface = computed(() => this.layoutConfig().surface);

    isOverlay = computed(() => this.layoutConfig().menuMode === 'overlay');

    private initialized = false;

    constructor() {
        effect(() => {
            const config = this.layoutConfig();
            if (config) {
                this.onConfigUpdate();
            }
        });

        // Effect to automatically save AI assistant state
        effect(() => {
            const state = this.layoutState();
            if (this.initialized && state) {
                this.saveAiAssistantState(state);
            }
        });
    }

    private getStoredAiAssistantActive(): boolean {
        try {
            const stored = localStorage.getItem(this.AI_ASSISTANT_ACTIVE_KEY);
            return stored ? JSON.parse(stored) : true; // true par défaut
        } catch {
            return true;
        }
    }

    private getStoredAiAssistantPanelSize(): number {
        const stored = localStorage.getItem(this.AI_ASSISTANT_PANEL_SIZE_KEY);
        return stored ? JSON.parse(stored) : 30; // 30 par défaut
    }



    private saveAiAssistantState(state: LayoutState): void {
        localStorage.setItem(this.AI_ASSISTANT_ACTIVE_KEY, JSON.stringify(state.aiAssistantActive));
        localStorage.setItem(this.AI_ASSISTANT_PANEL_SIZE_KEY, JSON.stringify(state.aiAssistantPanelSize));
    }

    onMenuToggle() {
        if (this.isOverlay()) {
            this.layoutState.update((prev) => ({ ...prev, overlayMenuActive: !this.layoutState().overlayMenuActive }));

            if (this.layoutState().overlayMenuActive) {
                this.overlayOpenSignal.set(null);
            }
        }

        if (this.isDesktop()) {
            this.layoutState.update((prev) => ({ ...prev, staticMenuDesktopInactive: !this.layoutState().staticMenuDesktopInactive }));
        } else {
            this.layoutState.update((prev) => ({ ...prev, staticMenuMobileActive: !this.layoutState().staticMenuMobileActive }));

            if (this.layoutState().staticMenuMobileActive) {
                this.overlayOpenSignal.set(null);
            }
        }
    }

    onAIAssistantToggle() {
        this.layoutState.update(state => ({
            ...state,
            aiAssistantActive: !state.aiAssistantActive
        }));
    }



    updateAiAssistantPanelSize(size: number) {
        this.layoutState.update((prev) => ({ 
            ...prev, 
            aiAssistantPanelSize: size,
            aiAssistantActive: size > 0
        }));
    }

    isDesktop() {
        return window.innerWidth > 991;
    }

    isMobile() {
        return !this.isDesktop();
    }

    onConfigUpdate() {
        this._config = { ...this.layoutConfig() };
        this.configUpdateSignal.set(this.layoutConfig());
    }

    onMenuStateChange(event: MenuChangeEvent) {
        this.menuSourceSignal.set(event);
    }

    reset() {
        this.resetSourceSignal.set(true);
    }
}
