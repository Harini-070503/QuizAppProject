import { TestBed, ComponentFixture } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';
import { RegisterComponent } from './register.component';
import { AuthService } from '../service/auth.service';

describe('RegisterComponent', () => {
  let component: RegisterComponent;
  let fixture: ComponentFixture<RegisterComponent>;
  let authService: jasmine.SpyObj<AuthService>;
  let router: Router;

  beforeEach(async () => {
    const authSpy = jasmine.createSpyObj('AuthService', ['register', 'isLoggedIn', 'currentUser', 'token', 'isCreator']);
    authSpy.isLoggedIn.and.returnValue(false);
    authSpy.currentUser.and.returnValue(null);

    await TestBed.configureTestingModule({
      imports: [RegisterComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: authSpy }
      ]
    }).compileComponents();

    authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    router = TestBed.inject(Router);
    fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => expect(component).toBeTruthy());

  it('should default role to Taker', () => expect(component.role).toBe('Taker'));

  it('should initialise all fields as empty', () => {
    expect(component.username).toBe('');
    expect(component.email).toBe('');
    expect(component.password).toBe('');
    expect(component.name).toBe('');
  });

  it('should initialise loading and error signals', () => {
    expect(component.loading()).toBeFalse();
    expect(component.error()).toBe('');
  });

  it('should allow switching role to Creator', () => {
    component.role = 'Creator';
    expect(component.role).toBe('Creator');
  });

  describe('onRegister()', () => {
    it('should call auth.register with all fields', () => {
      authService.register.and.returnValue(of({ token: 'tok' }));
      component.username = 'newuser';
      component.email = 'new@test.com';
      component.password = 'pass123';
      component.name = 'New User';
      component.role = 'Taker';
      component.onRegister();
      expect(authService.register).toHaveBeenCalledWith({
        username: 'newuser',
        email: 'new@test.com',
        password: 'pass123',
        role: 'Taker',
        name: 'New User'
      });
    });

    it('should pass undefined for name when name is empty', () => {
      authService.register.and.returnValue(of({ token: 'tok' }));
      component.username = 'u';
      component.email = 'e@t.com';
      component.password = 'p';
      component.name = '';
      component.onRegister();
      const callArgs = authService.register.calls.mostRecent().args[0];
      expect(callArgs.name).toBeUndefined();
    });

    it('should navigate to /dashboard on success', () => {
      spyOn(router, 'navigate');
      authService.register.and.returnValue(of({ token: 'tok' }));
      component.username = 'u';
      component.email = 'e@t.com';
      component.password = 'p';
      component.onRegister();
      expect(router.navigate).toHaveBeenCalledWith(['/dashboard']);
    });

    it('should set error message on failure', () => {
      authService.register.and.returnValue(
        throwError(() => ({ error: { message: 'Username already exists.' } }))
      );
      component.username = 'u';
      component.email = 'e@t.com';
      component.password = 'p';
      component.onRegister();
      expect(component.error()).toBe('Username already exists.');
      expect(component.loading()).toBeFalse();
    });
  });

  describe('template', () => {
    it('should render username, email, password inputs', () => {
      const el = fixture.nativeElement as HTMLElement;
      expect(el.querySelector('input[name="username"]')).not.toBeNull();
      expect(el.querySelector('input[name="email"]')).not.toBeNull();
      expect(el.querySelector('input[name="password"]')).not.toBeNull();
    });
  });
});
