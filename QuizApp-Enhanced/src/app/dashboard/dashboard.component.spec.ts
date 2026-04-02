import { TestBed, ComponentFixture } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';
import { DashboardComponent } from './dashboard.component';
import { AuthService } from '../service/auth.service';
import { QuizService } from '../service/quiz.service';
import { CategoryService } from '../service/category.service';
import { LeaderboardService } from '../service/leaderboard.service';
import { QuizDto, CategoryDto, LeaderboardDto } from '../models/models';

describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let fixture: ComponentFixture<DashboardComponent>;
  let quizSvc: jasmine.SpyObj<QuizService>;
  let catSvc: jasmine.SpyObj<CategoryService>;
  let lbSvc: jasmine.SpyObj<LeaderboardService>;
  let authSvc: jasmine.SpyObj<AuthService>;

  const mockUser = { userId: 'u1', username: 'testuser', email: 't@t.com', role: 'Taker' };

  const mockQuizzes: QuizDto[] = [
    { quizId: 'q1', quizName: 'Angular Quiz', passMark: 5, totalQuestion: 10, category: { categoryId: 'cat-1', categoryName: 'Tech' } },
    { quizId: 'q2', quizName: 'Science Quiz', passMark: 3, totalQuestion: 5,  category: { categoryId: 'cat-2', categoryName: 'Science' } }
  ];

  const mockCategories: CategoryDto[] = [
    { categoryId: 'cat-1', categoryName: 'Tech' },
    { categoryId: 'cat-2', categoryName: 'Science' }
  ];

  const mockLeaderboard: LeaderboardDto[] = [
    { userId: 'u1', username: 'alice', name: 'Alice', quizId: 'q1', quizName: 'Angular Quiz', percentage: 90, createdAt: '2024-01-01' }
  ];

  beforeEach(async () => {
    const quizSpy = jasmine.createSpyObj('QuizService', ['getAll']);
    const catSpy  = jasmine.createSpyObj('CategoryService', ['getAll']);
    const lbSpy   = jasmine.createSpyObj('LeaderboardService', ['getTop']);
    const authSpy = jasmine.createSpyObj('AuthService', ['isLoggedIn', 'currentUser', 'isCreator', 'token']);

    quizSpy.getAll.and.returnValue(of(mockQuizzes));
    catSpy.getAll.and.returnValue(of(mockCategories));
    lbSpy.getTop.and.returnValue(of(mockLeaderboard));
    authSpy.isLoggedIn.and.returnValue(true);
    authSpy.currentUser.and.returnValue(mockUser);
    authSpy.isCreator.and.returnValue(false);
    authSpy.token.and.returnValue('tok');

    await TestBed.configureTestingModule({
      imports: [DashboardComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: QuizService,      useValue: quizSpy },
        { provide: CategoryService,  useValue: catSpy },
        { provide: LeaderboardService, useValue: lbSpy },
        { provide: AuthService,      useValue: authSpy }
      ]
    }).compileComponents();

    quizSvc = TestBed.inject(QuizService) as jasmine.SpyObj<QuizService>;
    catSvc  = TestBed.inject(CategoryService) as jasmine.SpyObj<CategoryService>;
    lbSvc   = TestBed.inject(LeaderboardService) as jasmine.SpyObj<LeaderboardService>;
    authSvc = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => expect(component).toBeTruthy());

  it('should load quizzes on init', () => {
    expect(quizSvc.getAll).toHaveBeenCalled();
    expect(component.quizzes().length).toBe(2);
  });

  it('should load categories on init', () => {
    expect(catSvc.getAll).toHaveBeenCalled();
    expect(component.categories().length).toBe(2);
  });

  it('should load leaderboard on init', () => {
    expect(lbSvc.getTop).toHaveBeenCalled();
    expect(component.leaderboard().length).toBe(1);
  });

  it('should set loading to false after data loads', () => {
    expect(component.loading()).toBeFalse();
  });

  describe('filteredQuizzes()', () => {
    it('should return all quizzes when no category selected', () => {
      component.selectedCat.set('');
      expect(component.filteredQuizzes().length).toBe(2);
    });

    it('should filter quizzes by selected category', () => {
      component.selectedCat.set('cat-1');
      const filtered = component.filteredQuizzes();
      expect(filtered.length).toBe(1);
      expect(filtered[0].quizId).toBe('q1');
    });

    it('should return empty array when category has no quizzes', () => {
      component.selectedCat.set('cat-99');
      expect(component.filteredQuizzes().length).toBe(0);
    });
  });

  it('should handle quiz load error gracefully', () => {
    quizSvc.getAll.and.returnValue(throwError(() => new Error('Network error')));
    component.ngOnInit();
    expect(component.loading()).toBeFalse();
  });
});
