import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { AuthService } from './auth.service';
import { LoginRequestDto, RegisterRequestDto } from '../models/models';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  const mockToken = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.' +
    btoa(JSON.stringify({
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier': 'user-123',
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name': 'testuser',
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress': 'test@test.com',
      'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': 'Taker'
    })) + '.signature';

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        AuthService,
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([])
      ]
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should have isLoggedIn as false initially when no token', () => {
    expect(service.isLoggedIn()).toBeFalse();
  });

  it('should have currentUser as null initially', () => {
    expect(service.currentUser()).toBeNull();
  });

  it('should have token as null initially', () => {
    expect(service.token()).toBeNull();
  });

  it('should have isCreator as false initially', () => {
    expect(service.isCreator()).toBeFalse();
  });

  describe('login()', () => {
    it('should POST to /api/auth/login', () => {
      const dto: LoginRequestDto = { username: 'testuser', password: 'pass123' };
      service.login(dto).subscribe();
      const req = httpMock.expectOne('http://localhost:5137/api/auth/login');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(dto);
      req.flush({ token: mockToken });
    });

    it('should store token in localStorage on success', () => {
      const dto: LoginRequestDto = { username: 'testuser', password: 'pass123' };
      service.login(dto).subscribe();
      const req = httpMock.expectOne('http://localhost:5137/api/auth/login');
      req.flush({ token: mockToken });
      expect(localStorage.getItem('quiz_token')).toBe(mockToken);
    });

    it('should set isLoggedIn to true after successful login', () => {
      const dto: LoginRequestDto = { username: 'testuser', password: 'pass123' };
      service.login(dto).subscribe();
      const req = httpMock.expectOne('http://localhost:5137/api/auth/login');
      req.flush({ token: mockToken });
      expect(service.isLoggedIn()).toBeTrue();
    });

    it('should propagate error on login failure', () => {
      const dto: LoginRequestDto = { username: 'bad', password: 'bad' };
      let errorOccurred = false;
      service.login(dto).subscribe({ error: () => errorOccurred = true });
      const req = httpMock.expectOne('http://localhost:5137/api/auth/login');
      req.flush({ message: 'Invalid credentials' }, { status: 401, statusText: 'Unauthorized' });
      expect(errorOccurred).toBeTrue();
    });
  });

  describe('register()', () => {
    it('should POST to /api/auth/register', () => {
      const dto: RegisterRequestDto = {
        username: 'newuser', email: 'new@test.com',
        password: 'pass123', role: 'Taker'
      };
      service.register(dto).subscribe();
      const req = httpMock.expectOne('http://localhost:5137/api/auth/register');
      expect(req.request.method).toBe('POST');
      req.flush({ token: mockToken });
    });

    it('should store token after successful registration', () => {
      const dto: RegisterRequestDto = {
        username: 'newuser', email: 'new@test.com',
        password: 'pass123', role: 'Taker'
      };
      service.register(dto).subscribe();
      const req = httpMock.expectOne('http://localhost:5137/api/auth/register');
      req.flush({ token: mockToken });
      expect(service.isLoggedIn()).toBeTrue();
    });
  });

  describe('logout()', () => {
    it('should clear token from localStorage', () => {
      localStorage.setItem('quiz_token', mockToken);
      service.logout();
      expect(localStorage.getItem('quiz_token')).toBeNull();
    });

    it('should clear user from localStorage', () => {
      localStorage.setItem('quiz_user', JSON.stringify({ userId: '1' }));
      service.logout();
      expect(localStorage.getItem('quiz_user')).toBeNull();
    });

    it('should set isLoggedIn to false', () => {
      localStorage.setItem('quiz_token', mockToken);
      service.logout();
      expect(service.isLoggedIn()).toBeFalse();
    });

    it('should set currentUser to null', () => {
      service.logout();
      expect(service.currentUser()).toBeNull();
    });
  });

  describe('forgotPassword()', () => {
    it('should POST to /api/auth/forgot-password', () => {
      service.forgotPassword({ usernameOrEmail: 'test@test.com' }).subscribe();
      const req = httpMock.expectOne('http://localhost:5137/api/auth/forgot-password');
      expect(req.request.method).toBe('POST');
      req.flush({ resetToken: 'reset-token-123', expiresAtUtc: new Date().toISOString() });
    });
  });

  describe('resetPassword()', () => {
    it('should POST to /api/auth/reset-password', () => {
      service.resetPassword({ username: 'testuser', resetToken: 'tok', newPassword: 'newpass' }).subscribe();
      const req = httpMock.expectOne('http://localhost:5137/api/auth/reset-password');
      expect(req.request.method).toBe('POST');
      req.flush(null);
    });
  });

  describe('setCurrentUser()', () => {
    it('should update currentUser signal', () => {
      const user = { userId: 'u1', username: 'admin', email: 'a@b.com', role: 'Creator' };
      service.setCurrentUser(user);
      expect(service.currentUser()).toEqual(user);
    });

    it('should set isCreator true for Creator role', () => {
      service.setCurrentUser({ userId: 'u1', username: 'creator', email: 'c@b.com', role: 'Creator' });
      expect(service.isCreator()).toBeTrue();
    });

    it('should set isCreator false for Taker role', () => {
      service.setCurrentUser({ userId: 'u1', username: 'taker', email: 't@b.com', role: 'Taker' });
      expect(service.isCreator()).toBeFalse();
    });
  });
});
