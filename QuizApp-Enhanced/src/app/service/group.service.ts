import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { GroupDto, GroupCreateDto, GroupMemberDto } from '../models/models';

@Injectable({ providedIn: 'root' })
export class GroupService {
  private readonly BASE = 'http://localhost:5137/api/groups';
  private http = inject(HttpClient);

  getMyGroups(): Observable<GroupDto[]> {
    return this.http.get<GroupDto[]>(this.BASE).pipe(catchError(e => throwError(() => e)));
  }

  get(id: string): Observable<GroupDto> {
    return this.http.get<GroupDto>(`${this.BASE}/${id}`).pipe(catchError(e => throwError(() => e)));
  }

  create(dto: GroupCreateDto): Observable<GroupDto> {
    return this.http.post<GroupDto>(this.BASE, dto).pipe(catchError(e => throwError(() => e)));
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.BASE}/${id}`).pipe(catchError(e => throwError(() => e)));
  }

  addMember(groupId: string, usernameOrEmail: string): Observable<GroupMemberDto> {
    return this.http.post<GroupMemberDto>(`${this.BASE}/${groupId}/members`, { usernameOrEmail }).pipe(catchError(e => throwError(() => e)));
  }

  removeMember(groupId: string, userId: string): Observable<void> {
    return this.http.delete<void>(`${this.BASE}/${groupId}/members/${userId}`).pipe(catchError(e => throwError(() => e)));
  }

  assignQuiz(groupId: string, quizId: string): Observable<void> {
    return this.http.put<void>(`${this.BASE}/${groupId}/assign-quiz/${quizId}`, {}).pipe(catchError(e => throwError(() => e)));
  }

  unassignQuiz(quizId: string): Observable<void> {
    return this.http.delete<void>(`${this.BASE}/unassign-quiz/${quizId}`).pipe(catchError(e => throwError(() => e)));
  }

  checkAccess(quizId: string): Observable<boolean> {
    return this.http.get<boolean>(`${this.BASE}/check-access/${quizId}`).pipe(catchError(e => throwError(() => e)));
  }
}
