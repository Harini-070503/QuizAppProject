import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { QuizDto, QuizCreateDto, QuizUpdateDto } from '../models/models';

@Injectable({ providedIn: 'root' })
export class QuizService {
  private readonly BASE = 'http://localhost:5137/api/quizzes';
  private http = inject(HttpClient);

  getAll(categoryId?: string, userId?: string): Observable<QuizDto[]> {
    let params = new HttpParams();
    if (categoryId) params = params.set('categoryId', categoryId);
    if (userId)     params = params.set('userId', userId);
    return this.http.get<QuizDto[]>(this.BASE, { params }).pipe(
      catchError(err => throwError(() => err))
    );
  }

  get(id: string): Observable<QuizDto> {
    return this.http.get<QuizDto>(`${this.BASE}/${id}`).pipe(
      catchError(err => throwError(() => err))
    );
  }

  create(dto: QuizCreateDto): Observable<QuizDto> {
    return this.http.post<QuizDto>(this.BASE, dto).pipe(
      catchError(err => throwError(() => err))
    );
  }

  update(id: string, dto: QuizUpdateDto): Observable<QuizDto> {
    return this.http.put<QuizDto>(`${this.BASE}/${id}`, dto).pipe(
      catchError(err => throwError(() => err))
    );
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.BASE}/${id}`).pipe(
      catchError(err => throwError(() => err))
    );
  }
}
