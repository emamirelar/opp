export interface FeedbackConfig {
    summary?: string;
    detail: string;
    life?: number;
    closable?: boolean;
    sticky?: boolean;
    showRefreshButton?: boolean;
    onConfirm?: () => void;
}
