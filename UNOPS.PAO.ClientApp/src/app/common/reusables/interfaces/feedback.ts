export interface FeedbackConfig {
  summary?: string;
  detail: any;
  life?: number;
  closable?: boolean;
  sticky?: boolean;
  onConfirm?: () => void;
}
