import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { UserDto, UpdateUserDto } from '../models/models';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly BASE = 'http://localhost:5137/api/user';
  private http = inject(HttpClient);

  getById(id: string): Observable<UserDto> {
    return this.http.get<UserDto>(`${this.BASE}/${id}`).pipe(catchError(e => throwError(() => e)));
  }
  getByUsername(username: string): Observable<UserDto> {
    return this.http.get<UserDto>(`${this.BASE}/username/${username}`).pipe(catchError(e => throwError(() => e)));
  }
  update(id: string, dto: UpdateUserDto): Observable<UserDto> {
    return this.http.put<UserDto>(`${this.BASE}/${id}`, dto).pipe(catchError(e => throwError(() => e)));
  }

  getTakers(search?: string): Observable<UserDto[]> {
    const url = search ? `${this.BASE}/takers?search=${encodeURIComponent(search)}` : `${this.BASE}/takers`;
    return this.http.get<UserDto[]>(url).pipe(catchError(e => throwError(() => e)));
  }
}
