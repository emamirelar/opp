import { Injectable, effect, signal, computed } from '@angular/core';
import { Subject } from 'rxjs';

export interface layoutConfig {
    preset?: string;
    primary?: string;
    surface?: string | undefined | null;
    darkTheme?: boolean;
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
    aiAssistantSidebarCollapsed?: boolean;
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
    private readonly AI_ASSISTANT_SIDEBAR_COLLAPSED_KEY = 'aiAssistantSidebarCollapsed';

    _config: layoutConfig = {
        preset: 'UnopsPreset',
        surface: null,
        darkTheme: false,
        menuMode: 'static'
    };

    _state: LayoutState = {
        staticMenuDesktopInactive: false,
        overlayMenuActive: false,
        configSidebarVisible: false,
        staticMenuMobileActive: false,
        menuHoverActive: false,
        aiAssistantActive: this.getStoredAiAssistantActive(),
        aiAssistantPanelSize: this.getStoredAiAssistantPanelSize(),
        aiAssistantSidebarCollapsed: this.getStoredAiAssistantSidebarCollapsed()
    };

    layoutConfig = signal<layoutConfig>(this._config);

    layoutState = signal<LayoutState>(this._state);

    private configUpdate = new Subject<layoutConfig>();

    private overlayOpen = new Subject<any>();

    private menuSource = new Subject<MenuChangeEvent>();

    private resetSource = new Subject();

    menuSource$ = this.menuSource.asObservable();

    resetSource$ = this.resetSource.asObservable();

    configUpdate$ = this.configUpdate.asObservable();

    overlayOpen$ = this.overlayOpen.asObservable();

    theme = computed(() => (this.layoutConfig()?.darkTheme ? 'light' : 'dark'));

    isSidebarActive = computed(() => this.layoutState().overlayMenuActive || this.layoutState().staticMenuMobileActive);

    isDarkTheme = computed(() => this.layoutConfig().darkTheme);

    getPrimary = computed(() => this.layoutConfig().primary);

    getSurface = computed(() => this.layoutConfig().surface);

    isOverlay = computed(() => this.layoutConfig().menuMode === 'overlay');

    // AI Assistant sidebar collapse state
    aiAssistantSidebarCollapsed = computed(() => this.layoutState().aiAssistantSidebarCollapsed);

    transitionComplete = signal<boolean>(false);

    private initialized = false;

    constructor() {
        effect(() => {
            const config = this.layoutConfig();
            if (config) {
                this.onConfigUpdate();
            }
        });

        effect(() => {
            const config = this.layoutConfig();

            if (!this.initialized || !config) {
                this.initialized = true;
                return;
            }

            this.handleDarkModeTransition(config);
        });

        // Effect pour sauvegarder automatiquement l'état de l'AI assistant
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

    private getStoredAiAssistantSidebarCollapsed(): boolean {
        try {
            const stored = localStorage.getItem(this.AI_ASSISTANT_SIDEBAR_COLLAPSED_KEY);
            return stored ? JSON.parse(stored) : true; // true par défaut - collapsed for Gemini-style
        } catch {
            return true;
        }
    }

    private saveAiAssistantState(state: LayoutState): void {
        localStorage.setItem(this.AI_ASSISTANT_ACTIVE_KEY, JSON.stringify(state.aiAssistantActive));
        localStorage.setItem(this.AI_ASSISTANT_PANEL_SIZE_KEY, JSON.stringify(state.aiAssistantPanelSize));
        localStorage.setItem(this.AI_ASSISTANT_SIDEBAR_COLLAPSED_KEY, JSON.stringify(state.aiAssistantSidebarCollapsed));
    }


    private handleDarkModeTransition(config: layoutConfig): void {
        if ((document as any).startViewTransition) {
            this.startViewTransition(config);
        } else {
            this.toggleDarkMode(config);
            this.onTransitionEnd();
        }
    }

    private startViewTransition(config: layoutConfig): void {
        const transition = (document as any).startViewTransition(() => {
            this.toggleDarkMode(config);
        });

        transition.ready
            .then(() => {
                this.onTransitionEnd();
            })
            .catch(() => {});
    }

    toggleDarkMode(config?: layoutConfig): void {
        const _config = config || this.layoutConfig();
        if (_config.darkTheme) {
            document.documentElement.classList.add('app-dark');
        } else {
            document.documentElement.classList.remove('app-dark');
        }
    }

    private onTransitionEnd() {
        this.transitionComplete.set(true);
        setTimeout(() => {
            this.transitionComplete.set(false);
        });
    }

    onMenuToggle() {
        if (this.isOverlay()) {
            this.layoutState.update((prev) => ({ ...prev, overlayMenuActive: !this.layoutState().overlayMenuActive }));

            if (this.layoutState().overlayMenuActive) {
                this.overlayOpen.next(null);
            }
        }

        if (this.isDesktop()) {
            this.layoutState.update((prev) => ({ ...prev, staticMenuDesktopInactive: !this.layoutState().staticMenuDesktopInactive }));
        } else {
            this.layoutState.update((prev) => ({ ...prev, staticMenuMobileActive: !this.layoutState().staticMenuMobileActive }));

            if (this.layoutState().staticMenuMobileActive) {
                this.overlayOpen.next(null);
            }
        }
    }

    onAIAssistantToggle() {
        this.layoutState.update(state => ({
            ...state,
            aiAssistantActive: !state.aiAssistantActive
        }));
    }

    onAiSidebarToggle() {
        this.layoutState.update(state => ({
            ...state,
            aiAssistantSidebarCollapsed: !state.aiAssistantSidebarCollapsed
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
        this.configUpdate.next(this.layoutConfig());
    }

    onMenuStateChange(event: MenuChangeEvent) {
        this.menuSource.next(event);
    }

    reset() {
        this.resetSource.next(true);
    }
}
