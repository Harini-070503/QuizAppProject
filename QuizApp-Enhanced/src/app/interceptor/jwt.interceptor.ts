import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../service/auth.service';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const auth   = inject(AuthService);
  const router = inject(Router);
  const token  = auth.token();

  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((err: HttpErrorResponse) => {
      switch (err.status) {
        case 401:
          // Only auto-logout on 401 from auth endpoints, not from business logic
          if (!err.url?.includes('/api/attempts')) {
            auth.logout();
            router.navigate(['/login']);
          }
          break;
        case 403:
          // Don't redirect — let the component handle it and show the message
          break;
        case 500:
          console.error('[QuizZap] Server error:', err.message);
          break;
      }
      return throwError(() => err);
    })
  );
};
