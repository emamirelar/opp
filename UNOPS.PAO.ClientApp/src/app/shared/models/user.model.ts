export interface UserValueModel {
  id: number;
  email: string;
  name: string;
  userProfile?: {
    userId: number;
    firstName?: string;
    lastName?: string;
    name: string;
  };
}
