import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import {
  PaymentInitiateDto, MonthlySubscriptionInitiateDto,
  PaymentConfirmDto, PaymentResponseDto
} from '../models/models';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private readonly BASE = 'http://localhost:5137/api/payments';
  private http = inject(HttpClient);

  initiate(dto: PaymentInitiateDto): Observable<PaymentResponseDto> {
    return this.http.post<PaymentResponseDto>(`${this.BASE}/initiate`, dto)
      .pipe(catchError(e => throwError(() => e)));
  }

  initiateMonthly(dto: MonthlySubscriptionInitiateDto): Observable<PaymentResponseDto> {
    return this.http.post<PaymentResponseDto>(`${this.BASE}/initiate-monthly`, dto)
      .pipe(catchError(e => throwError(() => e)));
  }

  confirm(dto: PaymentConfirmDto): Observable<PaymentResponseDto> {
    return this.http.post<PaymentResponseDto>(`${this.BASE}/confirm`, dto)
      .pipe(catchError(e => throwError(() => e)));
  }

  getMyPayments(userId: string): Observable<PaymentResponseDto[]> {
    const params = new HttpParams().set('userId', userId);
    return this.http.get<PaymentResponseDto[]>(`${this.BASE}/mine`, { params })
      .pipe(catchError(e => throwError(() => e)));
  }

  getSubscriptionStatus(userId: string): Observable<{ isActive: boolean }> {
    const params = new HttpParams().set('userId', userId);
    return this.http.get<{ isActive: boolean }>(`${this.BASE}/subscription-status`, { params })
      .pipe(catchError(e => throwError(() => e)));
  }
}
