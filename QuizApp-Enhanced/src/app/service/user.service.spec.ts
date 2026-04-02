import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { UserService } from './user.service';
import { UserDto, UpdateUserDto } from '../models/models';

describe('UserService', () => {
  let service: UserService;
  let httpMock: HttpTestingController;
  const BASE = 'http://localhost:5137/api/user';

  const mockUser: UserDto = {
    userId: 'user-1',
    username: 'testuser',
    email: 'test@test.com',
    role: 'Taker',
    name: 'Test User',
    city: 'Mumbai',
    state: 'Maharashtra'
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [UserService, provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(UserService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => expect(service).toBeTruthy());

  describe('getById()', () => {
    it('should GET user by id', () => {
      service.getById('user-1').subscribe(u => {
        expect(u.userId).toBe('user-1');
        expect(u.username).toBe('testuser');
        expect(u.role).toBe('Taker');
      });
      const req = httpMock.expectOne(`${BASE}/user-1`);
      expect(req.request.method).toBe('GET');
      req.flush(mockUser);
    });

    it('should propagate 404 when user not found', () => {
      let errorOccurred = false;
      service.getById('bad-id').subscribe({ error: () => errorOccurred = true });
      httpMock.expectOne(`${BASE}/bad-id`).flush(
        {}, { status: 404, statusText: 'Not Found' }
      );
      expect(errorOccurred).toBeTrue();
    });
  });

  describe('getByUsername()', () => {
    it('should GET user by username', () => {
      service.getByUsername('testuser').subscribe(u => {
        expect(u.username).toBe('testuser');
      });
      const req = httpMock.expectOne(`${BASE}/username/testuser`);
      expect(req.request.method).toBe('GET');
      req.flush(mockUser);
    });
  });

  describe('update()', () => {
    it('should PUT updated user details', () => {
      const dto: UpdateUserDto = { name: 'New Name', city: 'Delhi', state: 'Delhi' };
      service.update('user-1', dto).subscribe(u => expect(u.userId).toBe('user-1'));
      const req = httpMock.expectOne(`${BASE}/user-1`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(dto);
      req.flush({ ...mockUser, name: 'New Name' });
    });

    it('should propagate error on update failure', () => {
      let errorOccurred = false;
      service.update('user-1', {}).subscribe({ error: () => errorOccurred = true });
      httpMock.expectOne(`${BASE}/user-1`).flush(
        {}, { status: 500, statusText: 'Internal Server Error' }
      );
      expect(errorOccurred).toBeTrue();
    });
  });
});
