export interface DocumentLinkModel {
    link: string;
    name: string;
    type: string;
    parentEntityType: ParentEntityType;
    parentEntityId: number;
}

export enum ParentEntityType {
  Drive = 0,
  Contact = 1,
  Partner = 2,
  Archive = 99
}
