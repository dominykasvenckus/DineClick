export interface CustomJwtPayload {
  jti: string;
  iat: number;
  sub: string;
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": string;
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": string;
  exp: number;
  iss: string;
  aud: string;
}

export interface City {
  cityId: number;
  latitude: number;
  longitude: number;
  name: string;
}

export interface Restaurant {
  restaurantId: number;
  name: string;
  description: string;
  streetAddress: string;
  websiteUrl: string;
  restaurantManagerId?: number;
}

export interface Reservation {
  reservationId: number;
  date: string;
  time: string;
  partySize: number;
  status: number;
  createdAt: string;
  restaurantId?: number;
  reservingUserId: number;
}

export interface User {
  userId: string;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  role: number;
  isBanned: boolean;
  tokenValidityThreshold: string;
}

export interface Error {
  error: string;
}
