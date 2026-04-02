import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { QuestionService } from './question.service';
import { QuestionDto, QuestionCreateDto } from '../models/models';

describe('QuestionService', () => {
  let service: QuestionService;
  let httpMock: HttpTestingController;
  const BASE = 'http://localhost:5137/api/questions';

  const mockQuestion: QuestionDto = {
    questionId: 'q-1',
    questionText: 'What is Angular?',
    options: {
      optionId: 'opt-1',
      optionA: 'A framework',
      optionB: 'A library',
      optionC: 'A language',
      optionD: 'A database',
      correctOption: 'A'
    }
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [QuestionService, provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(QuestionService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => expect(service).toBeTruthy());

  describe('getByQuiz()', () => {
    it('should GET questions for a quiz', () => {
      service.getByQuiz('quiz-1').subscribe(qs => {
        expect(qs.length).toBe(1);
        expect(qs[0].questionText).toBe('What is Angular?');
      });
      const req = httpMock.expectOne(`${BASE}/by-quiz/quiz-1`);
      expect(req.request.method).toBe('GET');
      req.flush([mockQuestion]);
    });

    it('should return empty array when quiz has no questions', () => {
      service.getByQuiz('quiz-empty').subscribe(qs => expect(qs).toEqual([]));
      httpMock.expectOne(`${BASE}/by-quiz/quiz-empty`).flush([]);
    });
  });

  describe('get()', () => {
    it('should GET a single question by id', () => {
      service.get('q-1').subscribe(q => {
        expect(q.questionId).toBe('q-1');
        expect(q.options?.correctOption).toBe('A');
      });
      const req = httpMock.expectOne(`${BASE}/q-1`);
      expect(req.request.method).toBe('GET');
      req.flush(mockQuestion);
    });
  });

  describe('create()', () => {
    it('should POST a question to a quiz', () => {
      const dto: QuestionCreateDto = {
        questionText: 'New question?',
        options: { optionA: 'A', optionB: 'B', optionC: 'C', optionD: 'D', correctOption: 'B' }
      };
      service.create('quiz-1', dto).subscribe(q => expect(q.questionId).toBe('q-1'));
      const req = httpMock.expectOne(`${BASE}/quiz-1`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(dto);
      req.flush(mockQuestion);
    });
  });

  describe('update()', () => {
    it('should PUT to update a question', () => {
      const dto: QuestionCreateDto = {
        questionText: 'Updated?',
        options: { optionA: 'X', optionB: 'Y', optionC: 'Z', optionD: 'W', correctOption: 'C' }
      };
      service.update('q-1', dto).subscribe(q => expect(q).toBeTruthy());
      const req = httpMock.expectOne(`${BASE}/q-1`);
      expect(req.request.method).toBe('PUT');
      req.flush(mockQuestion);
    });
  });

  describe('delete()', () => {
    it('should DELETE a question by id', () => {
      service.delete('q-1').subscribe();
      const req = httpMock.expectOne(`${BASE}/q-1`);
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });
  });
});
