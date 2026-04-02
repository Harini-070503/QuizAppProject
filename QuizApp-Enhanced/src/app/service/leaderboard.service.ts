import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { LeaderboardDto } from '../models/models';

@Injectable({ providedIn: 'root' })
export class LeaderboardService {
  private readonly BASE = 'http://localhost:5137/api/leaderboard';
  private http = inject(HttpClient);

  getTop(categoryId?: string, take: number = 20): Observable<LeaderboardDto[]> {
    let params = new HttpParams().set('take', take);
    if (categoryId) params = params.set('categoryId', categoryId);
    return this.http.get<LeaderboardDto[]>(this.BASE, { params }).pipe(catchError(e => throwError(() => e)));
  }
}
