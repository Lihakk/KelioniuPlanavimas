export interface PointOfInterest {
    id?: number;
    name: string;
    type: string;
    address: string;
    hasTicket: boolean;
    workingHours: string;
    rating: number;
    longitude: string;
    latitude: string;
}

export interface RoutePoint {
    id?: number;
    routeId?: number;
    name: string;
    city: string;
    latitude: string;
    longitude: string;
    order: number;
    pointOfInterestId?: number;
}

export interface TravelRoute {
    id?: number;
    name: string;
    length: number;
    startingCity: string;
    endCity: string;
    polyline: string;
    travelTime: string;
    routePoints: RoutePoint[];
}

export type TripStatus = 'Planned' | 'Confirmed' | 'Completed' | 'Cancelled';

export interface Trip {
    id?: number;
    name: string;
    tripStatus: TripStatus;
    startDate: string;
    endDate: string;
    routeId?: number;
    route?: TravelRoute;
    selectedAccommodation: string;
    selectedFlight: string;
    selectedCar: string;
}

export type ReservationStatus = 'Created' | 'WaitingForPayment' | 'Paid' | 'Cancelled';
export type ReservationType = 'Flight' | 'Accommodation' | 'Car';

export interface Reservation {
    id?: number;
    tripId: number;
    reservationDate: string;
    reservationStatus: ReservationStatus;
    reservationType: ReservationType;
    provider: string;
    description: string;
    price: number;
}

export type PaymentStatus = 'Waiting' | 'Paid' | 'Refunded' | 'Cancelled';

export interface Payment {
    id?: number;
    tripId?: number;
    reservationId?: number;
    date: string;
    amount: number;
    paymentStatus: PaymentStatus;
}

export interface Review {
    id?: number;
    tripId?: number;
    tripElementType: string;
    tripElementId: number;
    reviewText: string;
    rating: number;
    date: string;
}
export type Item = {
    id?: number;
    supplyListId?: number;
    name: string;
    type: string;
    quantity: number;
    isPacked: boolean;
    reason?: string;
}

export type SupplyList = {
    id: number;
    tripId: number;
    dateCreated: string;
    weatherSummary?: string;
    items: Item[];
}
export interface TripRecommendation {
    tripId: number;
    name: string;
    score: number;
    reason: string;
}

export interface TravelOffer {
    provider: string;
    name: string;
    description: string;
    price: number;
    latitude: string;
    longitude: string;
}

export type AccountStatus = 'Active' | 'Blocked';

export interface UserAccount {
    id?: number;
    firstName: string;
    lastName: string;
    email: string;
    isAdmin: boolean;
    accountStatus: AccountStatus;
}

export interface RecommendationPreferences {
    travelType: string;
    budget: number;
    weatherPreference: string;
}

