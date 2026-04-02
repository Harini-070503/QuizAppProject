import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { QuestionDto, QuestionCreateDto } from '../models/models';

@Injectable({ providedIn: 'root' })
export class QuestionService {
  private readonly BASE = 'http://localhost:5137/api/questions';
  private http = inject(HttpClient);

  getByQuiz(quizId: string): Observable<QuestionDto[]> {
    return this.http.get<QuestionDto[]>(`${this.BASE}/by-quiz/${quizId}`).pipe(catchError(e => throwError(() => e)));
  }
  get(id: string): Observable<QuestionDto> {
    return this.http.get<QuestionDto>(`${this.BASE}/${id}`).pipe(catchError(e => throwError(() => e)));
  }
  create(quizId: string, dto: QuestionCreateDto): Observable<QuestionDto> {
    return this.http.post<QuestionDto>(`${this.BASE}/${quizId}`, dto).pipe(catchError(e => throwError(() => e)));
  }
  update(id: string, dto: QuestionCreateDto): Observable<QuestionDto> {
    return this.http.put<QuestionDto>(`${this.BASE}/${id}`, dto).pipe(catchError(e => throwError(() => e)));
  }
  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.BASE}/${id}`).pipe(catchError(e => throwError(() => e)));
  }
}
