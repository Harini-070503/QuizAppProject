import { TestBed, ComponentFixture } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';
import { UserDetailsComponent } from './user-details.component';
import { UserService } from '../service/user.service';
import { AuthService } from '../service/auth.service';
import { UserDto } from '../models/models';

describe('UserDetailsComponent', () => {
  let component: UserDetailsComponent;
  let fixture: ComponentFixture<UserDetailsComponent>;
  let userSvc: jasmine.SpyObj<UserService>;
  let authSvc: jasmine.SpyObj<AuthService>;

  const mockUser: UserDto = {
    userId: 'u1',
    username: 'testuser',
    email: 'test@test.com',
    role: 'Taker',
    name: 'Test User',
    phoneNumber: '9876543210',
    addressLine1: '123 Main St',
    addressLine2: 'Apt 4B',
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
      imports: [UserDetailsComponent],
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
    fixture = TestBed.createComponent(UserDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => expect(component).toBeTruthy());

  it('should load user details on init', () => {
    expect(userSvc.getById).toHaveBeenCalledWith('u1');
  });

  it('should populate all form fields from loaded user', () => {
    expect(component.form.name).toBe('Test User');
    expect(component.form.phoneNumber).toBe('9876543210');
    expect(component.form.addressLine1).toBe('123 Main St');
    expect(component.form.addressLine2).toBe('Apt 4B');
    expect(component.form.city).toBe('Mumbai');
    expect(component.form.state).toBe('Maharashtra');
    expect(component.form.pincode).toBe('400001');
  });

  it('should initialise saving as false', () => expect(component.saving()).toBeFalse());
  it('should initialise success as empty', () => expect(component.success()).toBe(''));
  it('should initialise error as empty', () => expect(component.error()).toBe(''));

  describe('onSave()', () => {
    it('should call userService.update with the current form', () => {
      component.form = { name: 'Updated Name', city: 'Pune', state: 'Maharashtra' };
      component.onSave();
      expect(userSvc.update).toHaveBeenCalledWith('u1', component.form);
    });

    it('should set success message after successful save', () => {
      component.onSave();
      expect(component.success()).toBe('Details updated successfully!');
      expect(component.saving()).toBeFalse();
    });

    it('should clear previous error and success before saving', () => {
      component.success.set('Old success');
      component.error.set('Old error');
      component.onSave();
      expect(component.error()).toBe('');
    });

    it('should set error message on save failure', () => {
      userSvc.update.and.returnValue(throwError(() => new Error('Server error')));
      component.onSave();
      expect(component.error()).toBe('Failed to update details. Please try again.');
      expect(component.saving()).toBeFalse();
    });

    it('should not call update when no userId', () => {
      authSvc.currentUser.and.returnValue(null as any);
      const updateCallsBefore = userSvc.update.calls.count();
      component.onSave();
      expect(userSvc.update.calls.count()).toBe(updateCallsBefore);
    });
  });

  describe('template', () => {
    it('should render the username from auth service', () => {
      const el = fixture.nativeElement as HTMLElement;
      expect(el.textContent).toContain('testuser');
    });

    it('should render address line 1 input', () => {
      const el = fixture.nativeElement as HTMLElement;
      expect(el.querySelector('input[name="addr1"]')).not.toBeNull();
    });

    it('should render city and state inputs', () => {
      const el = fixture.nativeElement as HTMLElement;
      expect(el.querySelector('input[name="city"]')).not.toBeNull();
      expect(el.querySelector('input[name="state"]')).not.toBeNull();
    });

    it('should render pincode input', () => {
      const el = fixture.nativeElement as HTMLElement;
      expect(el.querySelector('input[name="pin"]')).not.toBeNull();
    });

    it('should render a save button', () => {
      const el = fixture.nativeElement as HTMLElement;
      const btn = el.querySelector('button[type="submit"]') as HTMLButtonElement;
      expect(btn).not.toBeNull();
      expect(btn.textContent).toContain('Save Details');
    });
  });
});
