import { TestBed, ComponentFixture } from '@angular/core/testing';
import { provideRouter, ActivatedRoute, Router } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';
import { QuizCreateComponent } from './quiz-create.component';
import { QuizService } from '../service/quiz.service';
import { CategoryService } from '../service/category.service';
import { AuthService } from '../service/auth.service';
import { QuizDto, CategoryDto } from '../models/models';

describe('QuizCreateComponent', () => {
  let component: QuizCreateComponent;
  let fixture: ComponentFixture<QuizCreateComponent>;
  let quizSvc: jasmine.SpyObj<QuizService>;
  let catSvc: jasmine.SpyObj<CategoryService>;
  let authSvc: jasmine.SpyObj<AuthService>;
  let router: Router;

  const mockUser = { userId: 'u1', username: 'creator', email: 'c@c.com', role: 'Creator' };

  const mockCategories: CategoryDto[] = [
    { categoryId: 'cat-1', categoryName: 'Tech' },
    { categoryId: 'cat-2', categoryName: 'Science' }
  ];

  const mockCreatedQuiz: QuizDto = {
    quizId: 'q-new',
    quizName: 'Angular Test',
    passMark: 5,
    totalQuestion: 2,
    category: { categoryId: 'cat-1', categoryName: 'Tech' }
  };

  beforeEach(async () => {
    const quizSpy = jasmine.createSpyObj('QuizService', ['create', 'update', 'get']);
    const catSpy  = jasmine.createSpyObj('CategoryService', ['getAll']);
    const authSpy = jasmine.createSpyObj('AuthService', ['currentUser', 'isLoggedIn', 'token', 'isCreator']);

    catSpy.getAll.and.returnValue(of(mockCategories));
    quizSpy.create.and.returnValue(of(mockCreatedQuiz));
    authSpy.currentUser.and.returnValue(mockUser);
    authSpy.isLoggedIn.and.returnValue(true);
    authSpy.isCreator.and.returnValue(true);
    authSpy.token.and.returnValue('tok');

    await TestBed.configureTestingModule({
      imports: [QuizCreateComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: QuizService,     useValue: quizSpy },
        { provide: CategoryService, useValue: catSpy },
        { provide: AuthService,     useValue: authSpy },
        { provide: ActivatedRoute,  useValue: { snapshot: { paramMap: { get: () => null } } } }
      ]
    }).compileComponents();

    quizSvc = TestBed.inject(QuizService) as jasmine.SpyObj<QuizService>;
    catSvc  = TestBed.inject(CategoryService) as jasmine.SpyObj<CategoryService>;
    authSvc = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    router  = TestBed.inject(Router);
    fixture = TestBed.createComponent(QuizCreateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => expect(component).toBeTruthy());

  it('should load categories on init', () => {
    expect(catSvc.getAll).toHaveBeenCalled();
    expect(component.categories().length).toBe(2);
  });

  it('should initialise isEdit as false when no id in route', () => {
    expect(component.isEdit()).toBeFalse();
  });

  it('should initialise with empty questions list', () => {
    expect(component.questions().length).toBe(0);
  });

  it('should initialise form fields as defaults', () => {
    expect(component.quizName).toBe('');
    expect(component.difficulty).toBe('Easy');
    expect(component.passMark).toBe(0);
  });

  describe('addQuestion()', () => {
    it('should add a new blank question to the list', () => {
      component.addQuestion();
      expect(component.questions().length).toBe(1);
    });

    it('should default correctOption to A for new question', () => {
      component.addQuestion();
      expect(component.questions()[0].options.correctOption).toBe('A');
    });

    it('should add multiple questions', () => {
      component.addQuestion();
      component.addQuestion();
      component.addQuestion();
      expect(component.questions().length).toBe(3);
    });
  });

  describe('removeQuestion()', () => {
    it('should remove question at specified index', () => {
      component.addQuestion();
      component.addQuestion();
      component.removeQuestion(0);
      expect(component.questions().length).toBe(1);
    });

    it('should remove the correct question by index', () => {
      component.addQuestion();
      component.addQuestion();
      component.questions()[0].questionText = 'First';
      component.questions()[1].questionText = 'Second';
      component.removeQuestion(0);
      expect(component.questions()[0].questionText).toBe('Second');
    });
  });

  describe('onSubmit()', () => {
    it('should set error if quizName is empty', () => {
      component.quizName = '';
      component.categoryId = 'cat-1';
      component.onSubmit();
      expect(component.error()).toBe('Quiz name is required.');
      expect(quizSvc.create).not.toHaveBeenCalled();
    });

    it('should set error if categoryId is not selected', () => {
      component.quizName = 'My Quiz';
      component.categoryId = '';
      component.onSubmit();
      expect(component.error()).toBe('Please select a category.');
      expect(quizSvc.create).not.toHaveBeenCalled();
    });

    it('should call quizService.create on valid submission', () => {
      component.quizName = 'Angular Test';
      component.categoryId = 'cat-1';
      component.passMark = 5;
      component.onSubmit();
      expect(quizSvc.create).toHaveBeenCalled();
    });

    it('should pass userId from auth service to create DTO', () => {
      component.quizName = 'Test';
      component.categoryId = 'cat-1';
      component.onSubmit();
      const callArgs = quizSvc.create.calls.mostRecent().args[0];
      expect(callArgs.userId).toBe('u1');
    });

    it('should pass questions list in DTO', () => {
      component.quizName = 'Test';
      component.categoryId = 'cat-1';
      component.addQuestion();
      component.questions()[0].questionText = 'Q1?';
      component.onSubmit();
      const callArgs = quizSvc.create.calls.mostRecent().args[0];
      expect(callArgs.questions?.length).toBe(1);
    });

    it('should set success message after successful create', () => {
      component.quizName = 'Test';
      component.categoryId = 'cat-1';
      component.onSubmit();
      expect(component.success()).toBe('Quiz saved!');
    });

    it('should set error message on create failure', () => {
      quizSvc.create.and.returnValue(throwError(() => ({ error: { message: 'Duplicate quiz name.' } })));
      component.quizName = 'Test';
      component.categoryId = 'cat-1';
      component.onSubmit();
      expect(component.error()).toBe('Duplicate quiz name.');
    });
  });

  describe('edit mode', () => {
    it('should set isEdit to true when route has an id', async () => {
      const editQuiz: QuizDto = { ...mockCreatedQuiz, questions: [] };
      quizSvc.get = jasmine.createSpy().and.returnValue(of(editQuiz));

      await TestBed.overrideProvider(ActivatedRoute, {
        useValue: { snapshot: { paramMap: { get: () => 'q-existing' } } }
      });

      const newFixture = TestBed.createComponent(QuizCreateComponent);
      const newComponent = newFixture.componentInstance;
      newFixture.detectChanges();

      expect(newComponent.isEdit()).toBeTrue();
    });
  });
});
