export interface InteractionViewModel {
  id: number;
  type: string;
  date: Date;
  description?: string;
  status: string;
  sender?: string;
  recipients?: string[];
}

export interface GroupedInteraction {
  month: string;
  year: number;
  interactions: InteractionViewModel[];
}
