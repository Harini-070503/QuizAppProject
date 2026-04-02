import { TestBed } from '@angular/core/testing';
import { Router, UrlTree } from '@angular/router';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { authGuard, creatorGuard, guestGuard } from './auth.guard';
import { AuthService } from '../service/auth.service';
import { runInInjectionContext, EnvironmentInjector } from '@angular/core';

describe('Auth Guards', () => {
  let authService: AuthService;
  let router: Router;
  let injector: EnvironmentInjector;

  const mockRoute = {} as ActivatedRouteSnapshot;
  const mockState = {} as RouterStateSnapshot;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        AuthService
      ]
    });
    authService = TestBed.inject(AuthService);
    router      = TestBed.inject(Router);
    injector    = TestBed.inject(EnvironmentInjector);
  });

  afterEach(() => localStorage.clear());

  // ── authGuard ─────────────────────────────────────────────────────────────
  describe('authGuard', () => {
    it('should return true when user is logged in', () => {
      spyOn(authService, 'isLoggedIn').and.returnValue(true as any);
      const result = runInInjectionContext(injector, () => authGuard(mockRoute, mockState));
      expect(result).toBeTrue();
    });

    it('should return UrlTree to /login when not logged in', () => {
      spyOn(authService, 'isLoggedIn').and.returnValue(false as any);
      const result = runInInjectionContext(injector, () => authGuard(mockRoute, mockState));
      expect(result instanceof UrlTree).toBeTrue();
      expect(router.serializeUrl(result as UrlTree)).toBe('/login');
    });
  });

  // ── creatorGuard ──────────────────────────────────────────────────────────
  describe('creatorGuard', () => {
    it('should return true when logged in AND Creator', () => {
      spyOn(authService, 'isLoggedIn').and.returnValue(true as any);
      spyOn(authService, 'isCreator').and.returnValue(true as any);
      const result = runInInjectionContext(injector, () => creatorGuard(mockRoute, mockState));
      expect(result).toBeTrue();
    });

    it('should return UrlTree to /dashboard when logged in but not Creator', () => {
      spyOn(authService, 'isLoggedIn').and.returnValue(true as any);
      spyOn(authService, 'isCreator').and.returnValue(false as any);
      const result = runInInjectionContext(injector, () => creatorGuard(mockRoute, mockState));
      expect(result instanceof UrlTree).toBeTrue();
      expect(router.serializeUrl(result as UrlTree)).toBe('/dashboard');
    });

    it('should return UrlTree to /dashboard when not logged in at all', () => {
      spyOn(authService, 'isLoggedIn').and.returnValue(false as any);
      spyOn(authService, 'isCreator').and.returnValue(false as any);
      const result = runInInjectionContext(injector, () => creatorGuard(mockRoute, mockState));
      expect(result instanceof UrlTree).toBeTrue();
    });
  });

  // ── guestGuard ────────────────────────────────────────────────────────────
  describe('guestGuard', () => {
    it('should return true when user is NOT logged in', () => {
      spyOn(authService, 'isLoggedIn').and.returnValue(false as any);
      const result = runInInjectionContext(injector, () => guestGuard(mockRoute, mockState));
      expect(result).toBeTrue();
    });

    it('should return UrlTree to /dashboard when already logged in', () => {
      spyOn(authService, 'isLoggedIn').and.returnValue(true as any);
      const result = runInInjectionContext(injector, () => guestGuard(mockRoute, mockState));
      expect(result instanceof UrlTree).toBeTrue();
      expect(router.serializeUrl(result as UrlTree)).toBe('/dashboard');
    });
  });
});
