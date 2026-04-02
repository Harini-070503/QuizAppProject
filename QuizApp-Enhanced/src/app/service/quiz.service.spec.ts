import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { QuizService } from './quiz.service';
import { QuizDto, QuizCreateDto, QuizUpdateDto } from '../models/models';

describe('QuizService', () => {
  let service: QuizService;
  let httpMock: HttpTestingController;

  const BASE = 'http://localhost:5137/api/quizzes';

  const mockQuiz: QuizDto = {
    quizId: 'quiz-1',
    quizName: 'Angular Basics',
    description: 'Test your Angular knowledge',
    difficultyLevel: 'Easy',
    timeLimit: 10,
    passMark: 5,
    totalQuestion: 10,
    category: { categoryId: 'cat-1', categoryName: 'Programming' },
    creatorId: 'user-1',
    creatorName: 'Admin',
    questions: []
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [QuizService, provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(QuizService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getAll()', () => {
    it('should GET all quizzes', () => {
      service.getAll().subscribe(quizzes => {
        expect(quizzes.length).toBe(1);
        expect(quizzes[0].quizId).toBe('quiz-1');
      });
      const req = httpMock.expectOne(BASE);
      expect(req.request.method).toBe('GET');
      req.flush([mockQuiz]);
    });

    it('should GET quizzes filtered by categoryId', () => {
      service.getAll('cat-1').subscribe();
      const req = httpMock.expectOne(`${BASE}?categoryId=cat-1`);
      expect(req.request.method).toBe('GET');
      req.flush([mockQuiz]);
    });

    it('should return empty array when no quizzes', () => {
      service.getAll().subscribe(quizzes => expect(quizzes).toEqual([]));
      httpMock.expectOne(BASE).flush([]);
    });
  });

  describe('get()', () => {
    it('should GET a single quiz by id', () => {
      service.get('quiz-1').subscribe(q => {
        expect(q.quizId).toBe('quiz-1');
        expect(q.quizName).toBe('Angular Basics');
      });
      const req = httpMock.expectOne(`${BASE}/quiz-1`);
      expect(req.request.method).toBe('GET');
      req.flush(mockQuiz);
    });

    it('should propagate 404 error', () => {
      let errorOccurred = false;
      service.get('nonexistent').subscribe({ error: () => errorOccurred = true });
      httpMock.expectOne(`${BASE}/nonexistent`).flush(
        { message: 'Not found' }, { status: 404, statusText: 'Not Found' }
      );
      expect(errorOccurred).toBeTrue();
    });
  });

  describe('create()', () => {
    it('should POST a new quiz', () => {
      const dto: QuizCreateDto = {
        userId: 'user-1',
        quizName: 'New Quiz',
        categoryId: 'cat-1',
        passMark: 3,
        totalQuestion: 5
      };
      service.create(dto).subscribe(q => expect(q.quizId).toBe('quiz-1'));
      const req = httpMock.expectOne(BASE);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(dto);
      req.flush(mockQuiz);
    });
  });

  describe('update()', () => {
    it('should PUT to update a quiz', () => {
      const dto: QuizUpdateDto = {
        quizName: 'Updated Quiz',
        categoryId: 'cat-1',
        passMark: 5,
        totalQuestion: 10
      };
      service.update('quiz-1', dto).subscribe(q => expect(q.quizName).toBe('Angular Basics'));
      const req = httpMock.expectOne(`${BASE}/quiz-1`);
      expect(req.request.method).toBe('PUT');
      req.flush(mockQuiz);
    });
  });

  describe('delete()', () => {
    it('should DELETE a quiz by id', () => {
      service.delete('quiz-1').subscribe();
      const req = httpMock.expectOne(`${BASE}/quiz-1`);
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });
  });
});
