export interface DocumentLinkModel {
    link: string;
    name: string;
    type: string;
    parentEntityType: DocumentType;
    parentEntityId: number;
}

export enum DocumentType {
    FundingOpportunity = 0,
    Proposal = 1,
    Archive = 99
}