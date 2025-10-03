export interface Documentype {
  id: number;
  name: string;
  entityType: string;
}

export interface DocumentLinkModel {
  link: string;
  googleId: string;
  name: string;
  type: string;
  documentTypeId: number;
  parentEntityName: string;
  parentEntityId: number;
}
