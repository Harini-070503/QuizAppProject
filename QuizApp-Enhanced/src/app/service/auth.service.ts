import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, throwError } from 'rxjs';
import {
  LoginRequestDto,
  RegisterRequestDto,
  AuthResponseDto,
  ForgotPasswordRequestDto,
  ForgotPasswordResponseDto,
  ResetPasswordRequestDto,
  UserDto
} from '../models/models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly BASE_URL = 'http://localhost:5137/api/auth';
  private readonly TOKEN_KEY = 'quiz_token';
  private readonly USER_KEY  = 'quiz_user';

  private http   = inject(HttpClient);
  private router = inject(Router);

  // ── Signals ──────────────────────────────────────────────────────────────
  private _token       = signal<string | null>(localStorage.getItem(this.TOKEN_KEY));
  private _currentUser = signal<UserDto | null>(
    JSON.parse(localStorage.getItem(this.USER_KEY) ?? 'null')
  );

  readonly isLoggedIn    = computed(() => !!this._token());
  readonly currentUser   = computed(() => this._currentUser());
  readonly token         = computed(() => this._token());
  readonly isCreator     = computed(() => this._currentUser()?.role === 'Creator');
  readonly isEvaluator   = computed(() => this._currentUser()?.role === 'Evaluator');
  readonly isPremiumTaker = computed(() => this._currentUser()?.role === 'PremiumTaker');
  readonly isTaker       = computed(() => this._currentUser()?.role === 'Taker');

  // ── Auth Methods ─────────────────────────────────────────────────────────
  register(dto: RegisterRequestDto): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.BASE_URL}/register`, dto).pipe(
      tap(res => this.storeToken(res.token)),
      catchError(err => throwError(() => err))
    );
  }

  login(dto: LoginRequestDto): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.BASE_URL}/login`, dto).pipe(
      tap(res => this.storeToken(res.token)),
      catchError(err => throwError(() => err))
    );
  }

  forgotPassword(dto: ForgotPasswordRequestDto): Observable<ForgotPasswordResponseDto> {
    return this.http.post<ForgotPasswordResponseDto>(`${this.BASE_URL}/forgot-password`, dto).pipe(
      catchError(err => throwError(() => err))
    );
  }

  resetPassword(dto: ResetPasswordRequestDto): Observable<void> {
    return this.http.post<void>(`${this.BASE_URL}/reset-password`, dto).pipe(
      catchError(err => throwError(() => err))
    );
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this._token.set(null);
    this._currentUser.set(null);
    this.router.navigate(['/']);
  }

  setCurrentUser(user: UserDto): void {
    this._currentUser.set(user);
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
  }

  // ── Helpers ───────────────────────────────────────────────────────────────
  storeToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
    this._token.set(token);
    const decoded = this.decodeToken(token);
    if (decoded) {
      const user: UserDto = {
        userId:   (decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ?? decoded['sub']) as string,
        username: (decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name']           ?? decoded['name']) as string,
        email:    (decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress']   ?? decoded['email']) as string,
        role:     (decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']         ?? decoded['role']) as string,
      };
      this._currentUser.set(user);
      localStorage.setItem(this.USER_KEY, JSON.stringify(user));
    }
  }

  private decodeToken(token: string): Record<string, unknown> | null {
    try {
      return JSON.parse(atob(token.split('.')[1]));
    } catch {
      return null;
    }
  }
}
