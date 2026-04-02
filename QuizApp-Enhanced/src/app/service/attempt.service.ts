import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { AttemptDto, AttemptResultDto, SubmissionDetailDto, UpdateScoreDto } from '../models/models';

@Injectable({ providedIn: 'root' })
export class AttemptService {
  private readonly BASE = 'http://localhost:5137/api/attempts';
  private http = inject(HttpClient);

  submit(dto: AttemptDto): Observable<AttemptResultDto> {
    return this.http.post<AttemptResultDto>(this.BASE, dto).pipe(catchError(e => throwError(() => e)));
  }
  getMyAttempts(userId: string): Observable<AttemptResultDto[]> {
    const params = new HttpParams().set('userId', userId);
    return this.http.get<AttemptResultDto[]>(`${this.BASE}/mine`, { params }).pipe(catchError(e => throwError(() => e)));
  }
  get(attemptId: string): Observable<AttemptResultDto> {
    return this.http.get<AttemptResultDto>(`${this.BASE}/${attemptId}`).pipe(catchError(e => throwError(() => e)));
  }
  getSubmissionsByQuiz(quizId: string): Observable<SubmissionDetailDto[]> {
    return this.http.get<SubmissionDetailDto[]>(`${this.BASE}/quiz/${quizId}/submissions`).pipe(catchError(e => throwError(() => e)));
  }
  getSubmissionDetail(attemptId: string): Observable<SubmissionDetailDto> {
    return this.http.get<SubmissionDetailDto>(`${this.BASE}/${attemptId}/detail`).pipe(catchError(e => throwError(() => e)));
  }
  updateScore(attemptId: string, dto: UpdateScoreDto): Observable<void> {
    return this.http.put<void>(`${this.BASE}/${attemptId}/score`, dto).pipe(catchError(e => throwError(() => e)));
  }
}
