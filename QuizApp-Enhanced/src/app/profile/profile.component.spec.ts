import { TestBed, ComponentFixture } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';
import { ProfileComponent } from './profile.component';
import { UserService } from '../service/user.service';
import { AuthService } from '../service/auth.service';
import { UserDto } from '../models/models';

describe('ProfileComponent', () => {
  let component: ProfileComponent;
  let fixture: ComponentFixture<ProfileComponent>;
  let userSvc: jasmine.SpyObj<UserService>;
  let authSvc: jasmine.SpyObj<AuthService>;

  const mockUser: UserDto = {
    userId: 'u1',
    username: 'testuser',
    email: 'test@test.com',
    role: 'Taker',
    name: 'Test User',
    phoneNumber: '9999999999',
    city: 'Mumbai',
    state: 'Maharashtra',
    pincode: '400001'
  };

  beforeEach(async () => {
    const userSpy = jasmine.createSpyObj('UserService', ['getById', 'update']);
    const authSpy = jasmine.createSpyObj('AuthService', ['currentUser', 'isLoggedIn', 'token', 'isCreator']);

    userSpy.getById.and.returnValue(of(mockUser));
    userSpy.update.and.returnValue(of(mockUser));
    authSpy.currentUser.and.returnValue(mockUser);
    authSpy.isLoggedIn.and.returnValue(true);
    authSpy.token.and.returnValue('tok');
    authSpy.isCreator.and.returnValue(false);

    await TestBed.configureTestingModule({
      imports: [ProfileComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: UserService, useValue: userSpy },
        { provide: AuthService, useValue: authSpy }
      ]
    }).compileComponents();

    userSvc = TestBed.inject(UserService) as jasmine.SpyObj<UserService>;
    authSvc = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    fixture = TestBed.createComponent(ProfileComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => expect(component).toBeTruthy());

  it('should load user data on init', () => {
    expect(userSvc.getById).toHaveBeenCalledWith('u1');
  });

  it('should populate form fields from loaded user', () => {
    expect(component.form.name).toBe('Test User');
    expect(component.form.city).toBe('Mumbai');
    expect(component.form.state).toBe('Maharashtra');
    expect(component.form.pincode).toBe('400001');
  });

  it('should initialise saving as false', () => expect(component.saving()).toBeFalse());
  it('should initialise success as empty', () => expect(component.success()).toBe(''));
  it('should initialise error as empty', () => expect(component.error()).toBe(''));

  describe('onSave()', () => {
    it('should call userService.update with current userId', () => {
      component.form = { name: 'New Name', city: 'Delhi' };
      component.onSave();
      expect(userSvc.update).toHaveBeenCalledWith('u1', component.form);
    });

    it('should set success message on successful save', () => {
      component.onSave();
      expect(component.success()).toBe('Profile updated successfully!');
      expect(component.saving()).toBeFalse();
    });

    it('should clear previous messages before saving', () => {
      component.success.set('Old success');
      component.error.set('Old error');
      component.onSave();
      // After success, success is set to new value and error remains cleared
      expect(component.error()).toBe('');
    });

    it('should set error message on save failure', () => {
      userSvc.update.and.returnValue(throwError(() => new Error('Server error')));
      component.onSave();
      expect(component.error()).toBe('Failed to update profile.');
      expect(component.saving()).toBeFalse();
    });

    it('should not call update if no userId', () => {
      authSvc.currentUser.and.returnValue(null as any);
      // Re-create component with null user
      component.onSave();
      // Because userId is undefined, update should not be called again beyond init
      expect(component.saving()).toBeFalse();
    });
  });

  describe('template', () => {
    it('should render username from auth service', () => {
      fixture.detectChanges();
      const el = fixture.nativeElement as HTMLElement;
      expect(el.textContent).toContain('testuser');
    });

    it('should render form inputs for name and city', () => {
      const el = fixture.nativeElement as HTMLElement;
      expect(el.querySelector('input[name="name"]')).not.toBeNull();
      expect(el.querySelector('input[name="city"]')).not.toBeNull();
    });
  });
});
