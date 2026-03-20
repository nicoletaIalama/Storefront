import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, catchError, throwError, tap } from 'rxjs';

import { LoginRequest, LoginResponse } from '../models/auth';
import { getApiErrorMessage } from '../utils/api-error';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/auth';
  private readonly tokenStorageKey = 'storefront.auth.token';

  login(userName: string, password: string): Observable<LoginResponse> {
    const request: LoginRequest = { userName, password };
    return this.http.post<LoginResponse>(`${this.baseUrl}/login`, request).pipe(
      tap((response) => {
        localStorage.setItem(this.tokenStorageKey, response.token);
      }),
      catchError((error: unknown) => {
        if (error instanceof HttpErrorResponse && error.status === 401) {
          const fromBody = getApiErrorMessage(error, '');
          return throwError(() => new Error(fromBody || 'Invalid username or password.'));
        }
        const message = getApiErrorMessage(error, 'Could not login. Please try again.');
        return throwError(() => new Error(message));
      })
    );
  }

  logout(): void {
    localStorage.removeItem(this.tokenStorageKey);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenStorageKey);
  }

  getRole(): string | null {
    const token = this.getToken();
    if (!token) {
      return null;
    }

    const parts = token.split('.');
    if (parts.length < 2) {
      return null;
    }

    try {
      const payload = JSON.parse(this.base64UrlDecode(parts[1])) as Record<string, unknown>;
      return (
        (typeof payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] === 'string'
          ? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
          : null) ??
        (typeof payload['role'] === 'string' ? payload['role'] : null)
      );
    } catch {
      return null;
    }
  }

  isAdmin(): boolean {
    return this.getRole() === 'Admin';
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  private base64UrlDecode(value: string): string {
    const normalized = value.replace(/-/g, '+').replace(/_/g, '/');
    const pad = normalized.length % 4;
    const padded = pad > 0 ? normalized + '='.repeat(4 - pad) : normalized;
    return atob(padded);
  }
}
