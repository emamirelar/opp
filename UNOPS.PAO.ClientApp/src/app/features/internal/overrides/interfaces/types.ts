export interface DocumentLinkModel {
    link: string;
    name: string;
    type: string;
    parentEntityType: DocumentType;
    parentEntityId: number;
}

export enum DocumentType {
    Archive = 99
}