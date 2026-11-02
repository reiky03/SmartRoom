export interface Room {
  id: number;
  roomCode: string;
  name: string;
  building: string;
  capacity: number;
  facilities?: string;
  isActive: boolean;
  createdAt: string;
}

export enum BookingStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
  Cancelled = 3,
}

export interface Booking {
  id: number;
  bookingReference: string;
  roomId: number;
  room?: Room;
  borrowerName: string;
  borrowerEmail: string;
  purpose: string;
  startTime: string;
  endTime: string;
  status: BookingStatus;
  adminNotes?: string;
  createdAt: string;
}

export interface CreateBookingDto {
  roomId: number;
  borrowerName: string;
  borrowerEmail: string;
  purpose: string;
  startTime: string;
  endTime: string;
}

export interface RoomAvailabilityResponse {
  roomId: number;
  roomCode: string;
  roomName: string;
  date: string;
  bookedSlots: {
    bookingReference: string;
    borrowerName: string;
    startTime: string;
    endTime: string;
    status: string;
  }[];
}

export interface UserProfile {
  username: string;
  fullName: string;
  email: string;
  role: 'Admin' | 'Staff' | 'Student';
  department: string;
  avatarInitials: string;
}
