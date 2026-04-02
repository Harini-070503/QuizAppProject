import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';

export interface NotificationDto {
  notificationId: string;
  type: string;
  title: string;
  message: string;
  linkUrl?: string;
  isRead: boolean;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly BASE = 'http://localhost:5137/api/notifications';
  private http = inject(HttpClient);

  notifications = signal<NotificationDto[]>([]);
  unreadCount = signal(0);

  load() {
    this.http.get<NotificationDto[]>(this.BASE).subscribe({
      next: list => {
        this.notifications.set(list);
        this.unreadCount.set(list.filter(n => !n.isRead).length);
      },
      error: () => {}
    });
  }

  markRead(id: string): Observable<void> {
    return this.http.put<void>(`${this.BASE}/${id}/read`, {}).pipe(catchError(e => throwError(() => e)));
  }

  markAllRead(): Observable<void> {
    return this.http.put<void>(`${this.BASE}/read-all`, {}).pipe(catchError(e => throwError(() => e)));
  }
}
