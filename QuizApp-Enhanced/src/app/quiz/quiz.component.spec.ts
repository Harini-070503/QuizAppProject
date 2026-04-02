import { TestBed, ComponentFixture, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter, ActivatedRoute } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';
import { QuizComponent } from './quiz.component';
import { QuizService } from '../service/quiz.service';
import { AttemptService } from '../service/attempt.service';
import { AuthService } from '../service/auth.service';
import { QuizDto } from '../models/models';

describe('QuizComponent', () => {
  let component: QuizComponent;
  let fixture: ComponentFixture<QuizComponent>;
  let quizSvc: jasmine.SpyObj<QuizService>;
  let attemptSvc: jasmine.SpyObj<AttemptService>;

  const mockQuiz: QuizDto = {
    quizId: 'quiz-1',
    quizName: 'Angular Quiz',
    passMark: 3,
    totalQuestion: 3,
    timeLimit: 5,
    questions: [
      { questionId: 'q1', questionText: 'What is Angular?', options: { optionA: 'Framework', optionB: 'Library', optionC: 'Language', optionD: 'DB', correctOption: 'A' } },
      { questionId: 'q2', questionText: 'What is a signal?', options: { optionA: 'Variable', optionB: 'Reactive primitive', optionC: 'HTTP call', optionD: 'Module', correctOption: 'B' } },
      { questionId: 'q3', questionText: 'What is DI?', options: { optionA: 'Design pattern', optionB: 'Directive Interface', optionC: 'Data Input', optionD: 'None', correctOption: 'A' } }
    ]
  };

  beforeEach(async () => {
    const quizSpy    = jasmine.createSpyObj('QuizService', ['get']);
    const attemptSpy = jasmine.createSpyObj('AttemptService', ['submit']);
    const authSpy    = jasmine.createSpyObj('AuthService', ['currentUser', 'isLoggedIn', 'token', 'isCreator']);

    quizSpy.get.and.returnValue(of(mockQuiz));
    authSpy.currentUser.and.returnValue({ userId: 'u1', username: 'tester', email: 't@t.com', role: 'Taker' });
    authSpy.isLoggedIn.and.returnValue(true);
    authSpy.token.and.returnValue('tok');

    await TestBed.configureTestingModule({
      imports: [QuizComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: QuizService,    useValue: quizSpy },
        { provide: AttemptService, useValue: attemptSpy },
        { provide: AuthService,    useValue: authSpy },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: { get: () => 'quiz-1' } } }
        }
      ]
    }).compileComponents();

    quizSvc    = TestBed.inject(QuizService) as jasmine.SpyObj<QuizService>;
    attemptSvc = TestBed.inject(AttemptService) as jasmine.SpyObj<AttemptService>;
    fixture    = TestBed.createComponent(QuizComponent);
    component  = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => component.ngOnDestroy());

  it('should create', () => expect(component).toBeTruthy());

  it('should load quiz on init', () => {
    expect(quizSvc.get).toHaveBeenCalledWith('quiz-1');
    expect(component.quiz()?.quizId).toBe('quiz-1');
  });

  it('should set loading to false after quiz loads', () => {
    expect(component.loading()).toBeFalse();
  });

  it('should initialise questions from quiz', () => {
    expect(component.questions().length).toBe(3);
  });

  it('should initialise answers array with nulls', () => {
    const answers = component.answers();
    expect(answers.length).toBe(3);
    answers.forEach(a => expect(a).toBeNull());
  });

  it('should start at question index 0', () => {
    expect(component.currentIndex()).toBe(0);
  });

  it('should return first question as currentQuestion', () => {
    expect(component.currentQuestion()?.questionId).toBe('q1');
  });

  describe('selectAnswer()', () => {
    it('should record answer for current question', () => {
      component.selectAnswer('A');
      expect(component.answers()[0]).toBe('A');
    });

    it('should update selectedAnswer computed signal', () => {
      component.selectAnswer('B');
      expect(component.selectedAnswer()).toBe('B');
    });
  });

  describe('nextQuestion()', () => {
    it('should increment currentIndex', fakeAsync(() => {
      component.nextQuestion();
      tick(100);
      expect(component.currentIndex()).toBe(1);
    }));

    it('should not exceed last question', fakeAsync(() => {
      component.currentIndex.set(2);
      component.nextQuestion();
      tick(100);
      expect(component.currentIndex()).toBe(2);
    }));
  });

  describe('prevQuestion()', () => {
    it('should decrement currentIndex', fakeAsync(() => {
      component.currentIndex.set(2);
      component.prevQuestion();
      tick(100);
      expect(component.currentIndex()).toBe(1);
    }));

    it('should not go below 0', () => {
      component.currentIndex.set(0);
      component.prevQuestion();
      expect(component.currentIndex()).toBe(0);
    });
  });

  describe('jumpTo()', () => {
    it('should jump to specified question index', fakeAsync(() => {
      component.jumpTo(2);
      tick(100);
      expect(component.currentIndex()).toBe(2);
    }));
  });

  describe('progressPct()', () => {
    it('should compute correct progress percentage', () => {
      component.currentIndex.set(0);
      expect(component.progressPct()).toBeCloseTo(33.3, 0);
    });

    it('should be 100% at last question', () => {
      component.currentIndex.set(2);
      expect(component.progressPct()).toBe(100);
    });
  });

  describe('submitQuiz()', () => {
    it('should call attemptService.submit with answers', () => {
      attemptSvc.submit.and.returnValue(of({ attemptAnswerId: 'att-1', quizId: 'quiz-1', totalMark: 2, percentage: 66, feedback: [] }));
      component.answers.set(['A', 'B', null]);
      component.submitQuiz();
      expect(attemptSvc.submit).toHaveBeenCalled();
      const callArgs = attemptSvc.submit.calls.mostRecent().args[0];
      expect(callArgs.quizId).toBe('quiz-1');
      expect(callArgs.userId).toBe('u1');
      expect(callArgs.answers.length).toBe(3);
    });

    it('should set error signal on submit failure', () => {
      attemptSvc.submit.and.returnValue(throwError(() => new Error('Failed')));
      component.submitQuiz();
      expect(component.error()).toBe('Failed to submit quiz. Please try again.');
    });
  });

  describe('error handling', () => {
    it('should set error if quiz fails to load', () => {
      quizSvc.get.and.returnValue(throwError(() => new Error('Not found')));
      component.ngOnInit();
      expect(component.error()).toBe('Quiz not found or failed to load.');
      expect(component.loading()).toBeFalse();
    });
  });
});
