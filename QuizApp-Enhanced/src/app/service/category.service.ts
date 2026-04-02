import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { CategoryDto, CategoryCreateDto } from '../models/models';

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private readonly BASE = 'http://localhost:5137/api/categories';
  private http = inject(HttpClient);

  getAll(): Observable<CategoryDto[]> {
    return this.http.get<CategoryDto[]>(this.BASE).pipe(catchError(e => throwError(() => e)));
  }
  get(id: string): Observable<CategoryDto> {
    return this.http.get<CategoryDto>(`${this.BASE}/${id}`).pipe(catchError(e => throwError(() => e)));
  }
  create(dto: CategoryCreateDto): Observable<CategoryDto> {
    return this.http.post<CategoryDto>(this.BASE, dto).pipe(catchError(e => throwError(() => e)));
  }
  update(id: string, dto: CategoryCreateDto): Observable<CategoryDto> {
    return this.http.put<CategoryDto>(`${this.BASE}/${id}`, dto).pipe(catchError(e => throwError(() => e)));
  }
  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.BASE}/${id}`).pipe(catchError(e => throwError(() => e)));
  }
}
