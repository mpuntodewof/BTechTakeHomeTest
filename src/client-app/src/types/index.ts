// Auth types
export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
  fullName: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: UserInfo;
}

export interface UserInfo {
  id: string;
  email: string;
  fullName: string;
  role: string;
  balance: number;
}

// Transaction types
export interface TransferRequest {
  recipientEmail: string;
  amount: number;
  notes?: string;
}

export interface TransactionResponse {
  id: string;
  senderEmail: string;
  senderName: string;
  recipientEmail: string;
  recipientName: string;
  amount: number;
  notes: string | null;
  createdAt: string;
}

export interface TransactionListResponse {
  items: TransactionResponse[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// User types
export interface UserResponse {
  id: string;
  email: string;
  fullName: string;
  role: string;
  balance: number;
  createdAt: string;
}

export interface UserListResponse {
  items: UserResponse[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
