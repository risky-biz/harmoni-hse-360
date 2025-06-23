import { UserStatus } from '../types/enums';

export interface UserRole {
  roleId: number;
  roleName: string;
  roleType: string;
  description: string;
  assignedAt: string;
}

export interface User {
  id: number;
  email: string;
  name: string;
  employeeId: string;
  department: string;
  position: string;
  isActive: boolean;
  status: UserStatus;
  phoneNumber?: string;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  supervisorEmployeeId?: string;
  hireDate?: string;
  workLocation?: string;
  costCenter?: string;
  requiresMFA: boolean;
  lastPasswordChange?: string;
  lastLoginAt?: string;
  failedLoginAttempts: number;
  accountLockedUntil?: string;
  preferredLanguage?: string;
  timeZone?: string;
  createdAt: string;
  createdBy: string;
  lastModifiedAt?: string;
  lastModifiedBy?: string;
  roles: UserRole[];
}

export interface UserListResponse {
  users: User[];
  totalCount: number;
  totalPages: number;
  currentPage: number;
  pageSize: number;
}

export interface GetUsersParams {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
  department?: string;
  workLocation?: string;
  isActive?: boolean;
  status?: UserStatus;
  roleId?: number;
  requiresMFA?: boolean;
  isLocked?: boolean;
  hiredAfter?: string;
  hiredBefore?: string;
  supervisorEmployeeId?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface Role {
  id: number;
  name: string;
  roleType: string;
  description: string;
  isActive: boolean;
}

export interface CreateUserRequest {
  email: string;
  name: string;
  employeeId: string;
  department: string;
  position: string;
  password: string;
  phoneNumber?: string;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  supervisorEmployeeId?: string;
  hireDate?: string;
  workLocation?: string;
  costCenter?: string;
  preferredLanguage?: string;
  timeZone?: string;
  roleIds: number[];
}

export interface UserActivityLog {
  id: number;
  userId: number;
  activityType: string;
  description: string;
  entityType?: string;
  entityId?: number;
  ipAddress?: string;
  userAgent?: string;
  timestamp: string;
}

const API_BASE = '/api/User';

class UserManagementApi {
  private getAuthHeaders(): HeadersInit {
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
    };
    
    const token = localStorage.getItem('token');
    console.log('UserManagementApi: Token from localStorage:', token ? `${token.substring(0, 20)}...` : 'null');
    
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
      console.log('UserManagementApi: Added Authorization header');
    } else {
      console.log('UserManagementApi: No token found in localStorage');
    }
    
    return headers;
  }

  async getUsers(params: GetUsersParams = {}): Promise<UserListResponse> {
    const searchParams = new URLSearchParams();
    
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        searchParams.append(key, value.toString());
      }
    });

    const response = await fetch(`${API_BASE}?${searchParams.toString()}`, {
      method: 'GET',
      headers: this.getAuthHeaders(),
      credentials: 'include',
    });

    if (!response.ok) {
      throw new Error(`Failed to fetch users: ${response.statusText}`);
    }

    return response.json();
  }

  async getUserById(id: number): Promise<User> {
    const response = await fetch(`${API_BASE}/${id}`, {
      method: 'GET',
      headers: this.getAuthHeaders(),
      credentials: 'include',
    });

    if (!response.ok) {
      throw new Error(`Failed to fetch user: ${response.statusText}`);
    }

    return response.json();
  }

  async createUser(userData: CreateUserRequest): Promise<User> {
    const response = await fetch(API_BASE, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      credentials: 'include',
      body: JSON.stringify(userData),
    });

    if (!response.ok) {
      let errorMessage = 'Failed to create user';
      try {
        const errorData = await response.text();
        errorMessage = errorData || errorMessage;
      } catch (e) {
        // If parsing error data fails, use status text
        errorMessage = `Failed to create user: ${response.statusText}`;
      }
      throw new Error(errorMessage);
    }

    return response.json();
  }

  async updateUser(id: number, userData: any): Promise<User> {
    const response = await fetch(`${API_BASE}/${id}`, {
      method: 'PUT',
      headers: this.getAuthHeaders(),
      credentials: 'include',
      body: JSON.stringify(userData),
    });

    if (!response.ok) {
      const errorData = await response.text();
      throw new Error(`Failed to update user: ${errorData}`);
    }

    return response.json();
  }

  async deleteUser(id: number): Promise<void> {
    const response = await fetch(`${API_BASE}/${id}`, {
      method: 'DELETE',
      headers: this.getAuthHeaders(),
      credentials: 'include',
    });

    if (!response.ok) {
      throw new Error(`Failed to delete user: ${response.statusText}`);
    }
  }

  async changeUserStatus(id: number, status: UserStatus, reason?: string): Promise<User> {
    const response = await fetch(`${API_BASE}/${id}/status`, {
      method: 'PATCH',
      headers: this.getAuthHeaders(),
      credentials: 'include',
      body: JSON.stringify({ status, reason }),
    });

    if (!response.ok) {
      const errorData = await response.text();
      throw new Error(`Failed to change user status: ${errorData}`);
    }

    return response.json();
  }

  async unlockUser(id: number, reason?: string): Promise<User> {
    const response = await fetch(`${API_BASE}/${id}/unlock`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      credentials: 'include',
      body: JSON.stringify({ reason }),
    });

    if (!response.ok) {
      const errorData = await response.text();
      throw new Error(`Failed to unlock user: ${errorData}`);
    }

    return response.json();
  }

  async resetPassword(id: number, newPassword: string, requirePasswordChange: boolean = true): Promise<void> {
    const response = await fetch(`${API_BASE}/${id}/reset-password`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      credentials: 'include',
      body: JSON.stringify({ newPassword, requirePasswordChange }),
    });

    if (!response.ok) {
      const errorData = await response.text();
      throw new Error(`Failed to reset password: ${errorData}`);
    }
  }

  async assignRole(userId: number, roleId: number): Promise<void> {
    const response = await fetch(`${API_BASE}/${userId}/roles/${roleId}`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      credentials: 'include',
    });

    if (!response.ok) {
      throw new Error(`Failed to assign role: ${response.statusText}`);
    }
  }

  async removeRole(userId: number, roleId: number): Promise<void> {
    const response = await fetch(`${API_BASE}/${userId}/roles/${roleId}`, {
      method: 'DELETE',
      headers: this.getAuthHeaders(),
      credentials: 'include',
    });

    if (!response.ok) {
      throw new Error(`Failed to remove role: ${response.statusText}`);
    }
  }

  async getUserActivity(
    id: number,
    page: number = 1,
    pageSize: number = 50,
    activityType?: string,
    fromDate?: Date,
    toDate?: Date
  ): Promise<UserActivityLog[]> {
    const searchParams = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });

    if (activityType) searchParams.append('activityType', activityType);
    if (fromDate) searchParams.append('fromDate', fromDate.toISOString());
    if (toDate) searchParams.append('toDate', toDate.toISOString());

    const response = await fetch(`${API_BASE}/${id}/activity?${searchParams.toString()}`, {
      method: 'GET',
      headers: this.getAuthHeaders(),
      credentials: 'include',
    });

    if (!response.ok) {
      throw new Error(`Failed to fetch user activity: ${response.statusText}`);
    }

    return response.json();
  }

  async getRoles(includeInactive: boolean = false): Promise<Role[]> {
    const searchParams = new URLSearchParams();
    if (includeInactive) {
      searchParams.append('includeInactive', 'true');
    }

    const response = await fetch(`${API_BASE}/roles?${searchParams.toString()}`, {
      method: 'GET',
      headers: this.getAuthHeaders(),
      credentials: 'include',
    });

    if (!response.ok) {
      throw new Error(`Failed to fetch roles: ${response.statusText}`);
    }

    return response.json();
  }

  async getUserStatistics(): Promise<any> {
    const response = await fetch(`${API_BASE}/statistics`, {
      method: 'GET',
      headers: this.getAuthHeaders(),
      credentials: 'include',
    });

    if (!response.ok) {
      throw new Error(`Failed to fetch user statistics: ${response.statusText}`);
    }

    return response.json();
  }
}

export const userManagementApi = new UserManagementApi();