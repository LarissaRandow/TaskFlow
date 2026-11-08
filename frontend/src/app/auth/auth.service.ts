import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';

export interface RegisterUserRequest {
  email: string;
  password: string;
}

export interface RegisterUserResponse {
  id: string;
  email: string;
}

export interface LoginUserRequest {
  email: string;
  password: string;
}

export interface LoginUserResponse {
  accessToken: string;
  tokenType: 'Bearer';
  expiresAt: string;
  user: {
    id: string;
    email: string;
  };
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private static readonly accessTokenKey = 'taskflow.accessToken';
  private readonly http = inject(HttpClient);

  register(request: RegisterUserRequest): Observable<RegisterUserResponse> {
    return this.http.post<RegisterUserResponse>('/api/auth/register', request);
  }

  login(request: LoginUserRequest): Observable<LoginUserResponse> {
    return this.http.post<LoginUserResponse>('/api/auth/login', request).pipe(
      tap((response) => {
        sessionStorage.setItem(AuthService.accessTokenKey, response.accessToken);
      }),
    );
  }

  getAccessToken(): string | null {
    return sessionStorage.getItem(AuthService.accessTokenKey);
  }
}
