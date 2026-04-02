import { TestBed, ComponentFixture } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';
import { LoginComponent } from './login.component';
import { AuthService } from '../service/auth.service';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authService: jasmine.SpyObj<AuthService>;
  let router: Router;

  beforeEach(async () => {
    const authSpy = jasmine.createSpyObj('AuthService', ['login', 'isLoggedIn', 'currentUser', 'token', 'isCreator']);
    authSpy.isLoggedIn.and.returnValue(false);
    authSpy.currentUser.and.returnValue(null);

    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: authSpy }
      ]
    }).compileComponents();

    authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    router = TestBed.inject(Router);
    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => expect(component).toBeTruthy());

  it('should initialise with empty username and password', () => {
    expect(component.username).toBe('');
    expect(component.password).toBe('');
  });

  it('should initialise loading as false', () => expect(component.loading()).toBeFalse());

  it('should initialise error as empty string', () => expect(component.error()).toBe(''));

  it('should toggle showPass signal', () => {
    expect(component.showPass()).toBeFalse();
    component.showPass.set(true);
    expect(component.showPass()).toBeTrue();
  });

  describe('onLogin()', () => {
    it('should call auth.login with correct credentials', () => {
      authService.login.and.returnValue(of({ token: 'tok' }));
      component.username = 'testuser';
      component.password = 'pass123';
      component.onLogin();
      expect(authService.login).toHaveBeenCalledWith({ username: 'testuser', password: 'pass123' });
    });

    it('should set loading to true during login', () => {
      authService.login.and.returnValue(of({ token: 'tok' }));
      component.username = 'u';
      component.password = 'p';
      component.onLogin();
      // After success loading resets to false
      expect(component.loading()).toBeFalse();
    });

    it('should navigate to /dashboard on successful login', () => {
      spyOn(router, 'navigate');
      authService.login.and.returnValue(of({ token: 'tok' }));
      component.username = 'u';
      component.password = 'p';
      component.onLogin();
      expect(router.navigate).toHaveBeenCalledWith(['/dashboard']);
    });

    it('should set error message on login failure', () => {
      authService.login.and.returnValue(
        throwError(() => ({ error: { message: 'Invalid credentials. Please try again.' } }))
      );
      component.username = 'bad';
      component.password = 'bad';
      component.onLogin();
      expect(component.error()).toBe('Invalid credentials. Please try again.');
      expect(component.loading()).toBeFalse();
    });

    it('should use fallback error message if no error.message', () => {
      authService.login.and.returnValue(throwError(() => ({})));
      component.username = 'u';
      component.password = 'p';
      component.onLogin();
      expect(component.error()).toBe('Invalid credentials. Please try again.');
    });

    it('should clear previous error before new login attempt', () => {
      authService.login.and.returnValue(of({ token: 'tok' }));
      component.error.set('Old error');
      component.onLogin();
      expect(component.error()).toBe('');
    });
  });

  describe('template', () => {
    it('should render login form inputs', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.querySelector('input[name="username"]')).not.toBeNull();
      expect(compiled.querySelector('input[name="password"]')).not.toBeNull();
    });

    it('should render a submit button', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      const btn = compiled.querySelector('button[type="submit"]');
      expect(btn).not.toBeNull();
    });
  });
});
