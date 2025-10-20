import { PaginationParams } from '@shared/models/pagination-params.model';
import { InteractionType } from './interaction-type.enum';

export interface InteractionFilterParams extends PaginationParams {
  contactId?: number;
  type?: InteractionType;
  fromDate?: string;
  toDate?: string;
  searchText?: string;
} 
