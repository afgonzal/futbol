export interface IUserModel {
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  name?: string;
  pictureUrl?: string;
  idToken?: string;
  expiration?: number;
  apiToken?: string;
  role?: string;
}
