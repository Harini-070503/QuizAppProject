import { TestBed } from '@angular/core/testing';
import { HttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter, Router } from '@angular/router';
import { jwtInterceptor } from './jwt.interceptor';
import { AuthService } from '../service/auth.service';

describe('jwtInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let authService: AuthService;
  let router: Router;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([jwtInterceptor])),
        provideHttpClientTesting(),
        provideRouter([]),
        AuthService
      ]
    });
    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
    authService = TestBed.inject(AuthService);
    router = TestBed.inject(Router);
    spyOn(router, 'navigate');
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('should NOT attach Authorization header when no token', () => {
    http.get('/api/test').subscribe();
    const req = httpMock.expectOne('/api/test');
    expect(req.request.headers.has('Authorization')).toBeFalse();
    req.flush({});
  });

  it('should attach Authorization Bearer header when token exists', () => {
    spyOn(authService, 'token').and.returnValue('my-jwt-token' as any);
    http.get('/api/test').subscribe();
    const req = httpMock.expectOne('/api/test');
    expect(req.request.headers.get('Authorization')).toBe('Bearer my-jwt-token');
    req.flush({});
  });

  it('should call auth.logout() and redirect to /login on 401', () => {
    spyOn(authService, 'logout');
    spyOn(authService, 'token').and.returnValue('expired-token' as any);
    http.get('/api/secure').subscribe({ error: () => {} });
    const req = httpMock.expectOne('/api/secure');
    req.flush({}, { status: 401, statusText: 'Unauthorized' });
    expect(authService.logout).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('should redirect to /dashboard on 403 Forbidden', () => {
    spyOn(authService, 'token').and.returnValue('valid-token' as any);
    http.get('/api/admin').subscribe({ error: () => {} });
    const req = httpMock.expectOne('/api/admin');
    req.flush({}, { status: 403, statusText: 'Forbidden' });
    expect(router.navigate).toHaveBeenCalledWith(['/dashboard']);
  });

  it('should log error on 500 but not redirect', () => {
    spyOn(console, 'error');
    spyOn(authService, 'token').and.returnValue('valid-token' as any);
    http.get('/api/broken').subscribe({ error: () => {} });
    const req = httpMock.expectOne('/api/broken');
    req.flush({}, { status: 500, statusText: 'Internal Server Error' });
    expect(console.error).toHaveBeenCalled();
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('should propagate the original error to the caller', () => {
    let capturedError: any;
    spyOn(authService, 'token').and.returnValue(null as any);
    http.get('/api/test').subscribe({ error: e => capturedError = e });
    httpMock.expectOne('/api/test').flush(
      { message: 'Error' }, { status: 404, statusText: 'Not Found' }
    );
    expect(capturedError).toBeTruthy();
    expect(capturedError.status).toBe(404);
  });
});
