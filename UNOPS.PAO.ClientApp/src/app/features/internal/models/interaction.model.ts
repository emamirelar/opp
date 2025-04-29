import { InteractionType } from './interaction-type.enum';

export interface Interaction {
  id: number;
  type: InteractionType;
  date: string;
  data?: string;
  contactId: number;
  contactIds: number[];
  contactName?: string;
  description?: string;
  status: string;
}
