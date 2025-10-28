export interface ContactViewModel {
  id: number;
  firstName: string;
  lastName: string;
  title?: string;
  email?: string;
  phone?: string;
  profilePictureUrl?: string;
}

export interface GroupedContact {
  letter: string;
  contacts: ContactViewModel[];
} 
