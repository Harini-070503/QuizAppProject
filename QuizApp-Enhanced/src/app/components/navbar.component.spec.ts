import { TestBed, ComponentFixture } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { NavbarComponent } from './navbar.component';
import { AuthService } from '../service/auth.service';

describe('NavbarComponent', () => {
  let component: NavbarComponent;
  let fixture: ComponentFixture<NavbarComponent>;
  let authSvc: jasmine.SpyObj<AuthService>;

  const mockTaker    = { userId: 'u1', username: 'alice',   email: 'a@a.com', role: 'Taker'   };
  const mockCreator  = { userId: 'u2', username: 'creator', email: 'c@c.com', role: 'Creator' };

  function createComponent(loggedIn: boolean, user: any, isCreator = false) {
    authSvc.isLoggedIn.and.returnValue(loggedIn as any);
    authSvc.currentUser.and.returnValue(user);
    authSvc.isCreator.and.returnValue(isCreator as any);
    fixture = TestBed.createComponent(NavbarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }

  beforeEach(async () => {
    const authSpy = jasmine.createSpyObj('AuthService', ['isLoggedIn', 'currentUser', 'isCreator', 'token', 'logout']);
    authSpy.token.and.returnValue(null);

    await TestBed.configureTestingModule({
      imports: [NavbarComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: authSpy }
      ]
    }).compileComponents();

    authSvc = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
  });

  it('should create', () => {
    createComponent(false, null);
    expect(component).toBeTruthy();
  });

  describe('when user is NOT logged in', () => {
    beforeEach(() => createComponent(false, null));

    it('should show Login button', () => {
      const el = fixture.nativeElement as HTMLElement;
      expect(el.textContent).toContain('Login');
    });

    it('should show Register button', () => {
      const el = fixture.nativeElement as HTMLElement;
      expect(el.textContent).toContain('Register');
    });

    it('should NOT show Logout button', () => {
      const el = fixture.nativeElement as HTMLElement;
      expect(el.textContent).not.toContain('Logout');
    });

    it('should NOT show Dashboard link', () => {
      const el = fixture.nativeElement as HTMLElement;
      expect(el.textContent).not.toContain('Dashboard');
    });
  });

  describe('when Taker is logged in', () => {
    beforeEach(() => createComponent(true, mockTaker, false));

    it('should show Logout button', () => {
      const el = fixture.nativeElement as HTMLElement;
      expect(el.textContent).toContain('Logout');
    });

    it('should show Dashboard link', () => {
      const el = fixture.nativeElement as HTMLElement;
      expect(el.textContent).toContain('Dashboard');
    });

    it('should show Leaderboard link', () => {
      const el = fixture.nativeElement as HTMLElement;
      expect(el.textContent).toContain('Leaderboard');
    });

    it('should NOT show Create Quiz link', () => {
      const el = fixture.nativeElement as HTMLElement;
      expect(el.textContent).not.toContain('Create Quiz');
    });

    it('should NOT show Categories link', () => {
      const el = fixture.nativeElement as HTMLElement;
      expect(el.textContent).not.toContain('Categories');
    });

    it('should show first letter of username in avatar', () => {
      const el = fixture.nativeElement as HTMLElement;
      const badge = el.querySelector('.user-badge');
      expect(badge?.textContent?.trim()).toBe('A');
    });
  });

  describe('when Creator is logged in', () => {
    beforeEach(() => createComponent(true, mockCreator, true));

    it('should show Create Quiz link', () => {
      const el = fixture.nativeElement as HTMLElement;
      expect(el.textContent).toContain('Create Quiz');
    });

    it('should show Categories link', () => {
      const el = fixture.nativeElement as HTMLElement;
      expect(el.textContent).toContain('Categories');
    });
  });

  describe('logout()', () => {
    it('should call authService.logout when Logout is clicked', () => {
      createComponent(true, mockTaker);
      const el = fixture.nativeElement as HTMLElement;
      const logoutBtn = el.querySelector('button') as HTMLButtonElement;
      logoutBtn?.click();
      expect(authSvc.logout).toHaveBeenCalled();
    });
  });

  it('should render brand name QuizZap', () => {
    createComponent(false, null);
    const el = fixture.nativeElement as HTMLElement;
    expect(el.textContent).toContain('QuizZap');
  });
});
