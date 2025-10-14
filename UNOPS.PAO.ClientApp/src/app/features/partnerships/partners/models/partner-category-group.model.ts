export interface PartnerGroup {
  partnerGroupId: number;
  partnerGroupCode: string;
  partnerGroupName: string;
}

export interface PartnerCategoryGroup {
  partnerCategoryId: number;
  partnerCategoryCode: string;
  partnerCategoryName: string;
  children: PartnerGroup[];
} 
