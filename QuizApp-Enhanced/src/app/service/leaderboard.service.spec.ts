import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { LeaderboardService } from './leaderboard.service';
import { LeaderboardDto } from '../models/models';

describe('LeaderboardService', () => {
  let service: LeaderboardService;
  let httpMock: HttpTestingController;
  const BASE = 'http://localhost:5137/api/leaderboard';

  const mockEntries: LeaderboardDto[] = [
    { userId: 'u1', username: 'alice', name: 'Alice', quizId: 'q1', quizName: 'JS Quiz', percentage: 95, createdAt: '2024-01-01' },
    { userId: 'u2', username: 'bob',   name: 'Bob',   quizId: 'q1', quizName: 'JS Quiz', percentage: 88, createdAt: '2024-01-02' },
    { userId: 'u3', username: 'carol', name: 'Carol', quizId: 'q2', quizName: 'TS Quiz', percentage: 72, createdAt: '2024-01-03' }
  ];

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [LeaderboardService, provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(LeaderboardService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => expect(service).toBeTruthy());

  describe('getTop()', () => {
    it('should GET leaderboard with default take=20', () => {
      service.getTop().subscribe(entries => {
        expect(entries.length).toBe(3);
        expect(entries[0].username).toBe('alice');
      });
      const req = httpMock.expectOne(`${BASE}?take=20`);
      expect(req.request.method).toBe('GET');
      req.flush(mockEntries);
    });

    it('should GET with custom take value', () => {
      service.getTop(undefined, 5).subscribe();
      const req = httpMock.expectOne(`${BASE}?take=5`);
      expect(req.request.method).toBe('GET');
      req.flush([]);
    });

    it('should GET filtered by categoryId', () => {
      service.getTop('cat-1', 10).subscribe();
      const req = httpMock.expectOne(`${BASE}?take=10&categoryId=cat-1`);
      expect(req.request.method).toBe('GET');
      req.flush([]);
    });

    it('should return empty array when no entries', () => {
      service.getTop().subscribe(entries => expect(entries).toEqual([]));
      httpMock.expectOne(`${BASE}?take=20`).flush([]);
    });

    it('should propagate error on failure', () => {
      let errorOccurred = false;
      service.getTop().subscribe({ error: () => errorOccurred = true });
      httpMock.expectOne(`${BASE}?take=20`).flush(
        {}, { status: 500, statusText: 'Server Error' }
      );
      expect(errorOccurred).toBeTrue();
    });
  });
});
