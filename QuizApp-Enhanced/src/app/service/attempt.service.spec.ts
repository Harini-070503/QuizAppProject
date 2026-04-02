import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AttemptService } from './attempt.service';
import { AttemptDto, AttemptResultDto } from '../models/models';

describe('AttemptService', () => {
  let service: AttemptService;
  let httpMock: HttpTestingController;
  const BASE = 'http://localhost:5137/api/attempts';

  const mockResult: AttemptResultDto = {
    attemptAnswerId: 'att-1',
    quizId: 'quiz-1',
    totalMark: 7,
    percentage: 70,
    feedback: [
      { questionId: 'q-1', correctOption: 'A', yourOption: 'A', isCorrect: true },
      { questionId: 'q-2', correctOption: 'B', yourOption: 'C', isCorrect: false }
    ]
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [AttemptService, provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(AttemptService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => expect(service).toBeTruthy());

  describe('submit()', () => {
    it('should POST attempt to /api/attempts', () => {
      const dto: AttemptDto = {
        quizId: 'quiz-1',
        userId: 'user-1',
        answers: [{ questionId: 'q-1', chosenOption: 'A' }]
      };
      service.submit(dto).subscribe(result => {
        expect(result.attemptAnswerId).toBe('att-1');
        expect(result.percentage).toBe(70);
      });
      const req = httpMock.expectOne(BASE);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(dto);
      req.flush(mockResult);
    });

    it('should include startedAtUtc and endedAtUtc when provided', () => {
      const dto: AttemptDto = {
        quizId: 'quiz-1', userId: 'user-1', answers: [],
        startedAtUtc: '2024-01-01T10:00:00Z',
        endedAtUtc: '2024-01-01T10:15:00Z'
      };
      service.submit(dto).subscribe();
      const req = httpMock.expectOne(BASE);
      expect(req.request.body.startedAtUtc).toBe('2024-01-01T10:00:00Z');
      req.flush(mockResult);
    });

    it('should propagate error on submit failure', () => {
      let errorOccurred = false;
      service.submit({ quizId: 'q', userId: 'u', answers: [] })
        .subscribe({ error: () => errorOccurred = true });
      httpMock.expectOne(BASE).flush(
        { message: 'Time limit exceeded' }, { status: 400, statusText: 'Bad Request' }
      );
      expect(errorOccurred).toBeTrue();
    });
  });

  describe('getMyAttempts()', () => {
    it('should GET attempts by userId', () => {
      service.getMyAttempts('user-1').subscribe(attempts => {
        expect(attempts.length).toBe(1);
        expect(attempts[0].quizId).toBe('quiz-1');
      });
      const req = httpMock.expectOne(`${BASE}/mine?userId=user-1`);
      expect(req.request.method).toBe('GET');
      req.flush([mockResult]);
    });
  });

  describe('get()', () => {
    it('should GET a single attempt by id', () => {
      service.get('att-1').subscribe(a => {
        expect(a.attemptAnswerId).toBe('att-1');
        expect(a.feedback.length).toBe(2);
      });
      const req = httpMock.expectOne(`${BASE}/att-1`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResult);
    });

    it('should propagate 404 when attempt not found', () => {
      let errorOccurred = false;
      service.get('bad-id').subscribe({ error: () => errorOccurred = true });
      httpMock.expectOne(`${BASE}/bad-id`).flush(
        {}, { status: 404, statusText: 'Not Found' }
      );
      expect(errorOccurred).toBeTrue();
    });
  });
});
