import type {
  Room,
  Booking,
  CreateBookingDto,
  RoomAvailabilityResponse,
  BookingStatus,
  UserAccount,
  CreateUserPayload,
  UpdateUserPayload,
} from '../types';

const API_BASE = '/api';

export async function fetchApi<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
  const url = `${API_BASE}${endpoint}`;
  const headers = {
    'Content-Type': 'application/json',
    ...(options.headers || {}),
  };

  const response = await fetch(url, { ...options, headers });
  if (!response.ok) {
    let errorDetail = '';
    try {
      const errJson = await response.json();
      errorDetail = errJson.message || errJson.error || JSON.stringify(errJson);
    } catch {
      errorDetail = await response.text();
    }
    const error = new Error(errorDetail || `HTTP ${response.status}: ${response.statusText}`);
    (error as any).status = response.status;
    throw error;
  }

  // Handle empty responses
  if (response.status === 204) return {} as T;
  return response.json();
}

export const api = {
  // Rooms
  getRooms: (isActive?: boolean) => {
    const query = isActive !== undefined ? `?isActive=${isActive}` : '';
    return fetchApi<Room[]>(`/rooms${query}`);
  },
  getRoomById: (id: number) => fetchApi<Room>(`/rooms/${id}`),
  getRoomAvailability: (id: number, date: string) => 
    fetchApi<RoomAvailabilityResponse>(`/rooms/${id}/availability?date=${date}`),
  createRoom: (room: Partial<Room>) => 
    fetchApi<Room>('/rooms', { method: 'POST', body: JSON.stringify(room) }),
  updateRoom: (id: number, room: Partial<Room>) => 
    fetchApi<Room>(`/rooms/${id}`, { method: 'PUT', body: JSON.stringify(room) }),
  toggleRoomStatus: (id: number) => 
    fetchApi<{ message: string }>(`/rooms/${id}/toggle`, { method: 'PATCH' }),
  deleteRoom: (id: number) => 
    fetchApi<{ message: string }>(`/rooms/${id}`, { method: 'DELETE' }),
  uploadRoomImage: async (file: File): Promise<{ imageUrl: string }> => {
    const formData = new FormData();
    formData.append('file', file);
    const response = await fetch('/api/rooms/upload-image', {
      method: 'POST',
      body: formData,
    });
    if (!response.ok) {
      let errorMsg = 'Gagal mengunggah gambar';
      try {
        const errJson = await response.json();
        errorMsg = errJson.message || errorMsg;
      } catch {}
      throw new Error(errorMsg);
    }
    return response.json();
  },

  // Users & Civitas Management
  getUsers: () => fetchApi<UserAccount[]>('/users'),
  getUserById: (id: number) => fetchApi<UserAccount>(`/users/${id}`),
  createUser: (payload: CreateUserPayload) => 
    fetchApi<UserAccount>('/users', { method: 'POST', body: JSON.stringify(payload) }),
  updateUser: (id: number, payload: UpdateUserPayload) => 
    fetchApi<UserAccount>(`/users/${id}`, { method: 'PUT', body: JSON.stringify(payload) }),
  deleteUser: (id: number) => 
    fetchApi<{ message: string }>(`/users/${id}`, { method: 'DELETE' }),
  loginApi: (username: string, password: string) => 
    fetchApi<UserAccount>('/users/login', { method: 'POST', body: JSON.stringify({ username, password }) }),

  // Bookings
  getBookings: (params?: { roomId?: number; status?: BookingStatus; date?: string; borrowerEmail?: string }) => {
    const searchParams = new URLSearchParams();
    if (params?.roomId) searchParams.append('roomId', params.roomId.toString());
    if (params?.status !== undefined) searchParams.append('status', params.status.toString());
    if (params?.date) searchParams.append('date', params.date);
    if (params?.borrowerEmail) searchParams.append('borrowerEmail', params.borrowerEmail);
    const queryString = searchParams.toString() ? `?${searchParams.toString()}` : '';
    return fetchApi<Booking[]>(`/bookings${queryString}`);
  },
  getBookingById: (id: number) => fetchApi<Booking>(`/bookings/${id}`),
  getBookingByReference: (ref: string) => fetchApi<Booking>(`/bookings/reference/${ref}`),
  createBooking: (dto: CreateBookingDto) => 
    fetchApi<Booking>('/bookings', { method: 'POST', body: JSON.stringify(dto) }),
  updateBookingStatus: (id: number, status: BookingStatus, adminNotes?: string) => 
    fetchApi<Booking>(`/bookings/${id}/status`, {
      method: 'PATCH',
      body: JSON.stringify({ status, adminNotes }),
    }),
};
