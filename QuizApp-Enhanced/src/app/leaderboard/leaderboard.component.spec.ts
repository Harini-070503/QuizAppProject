import { TestBed, ComponentFixture } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';
import { LeaderboardComponent } from './leaderboard.component';
import { LeaderboardService } from '../service/leaderboard.service';
import { CategoryService } from '../service/category.service';
import { LeaderboardDto, CategoryDto } from '../models/models';

describe('LeaderboardComponent', () => {
  let component: LeaderboardComponent;
  let fixture: ComponentFixture<LeaderboardComponent>;
  let lbSvc: jasmine.SpyObj<LeaderboardService>;
  let catSvc: jasmine.SpyObj<CategoryService>;

  const mockEntries: LeaderboardDto[] = [
    { userId: 'u1', username: 'alice', name: 'Alice', quizId: 'q1', quizName: 'JS Quiz', percentage: 95, createdAt: '2024-01-01' },
    { userId: 'u2', username: 'bob',   name: 'Bob',   quizId: 'q1', quizName: 'JS Quiz', percentage: 80, createdAt: '2024-01-02' },
    { userId: 'u3', username: 'carol', name: 'Carol', quizId: 'q2', quizName: 'TS Quiz', percentage: 70, createdAt: '2024-01-03' }
  ];
  const mockCats: CategoryDto[] = [{ categoryId: 'c1', categoryName: 'Tech' }];

  beforeEach(async () => {
    const lbSpy  = jasmine.createSpyObj('LeaderboardService', ['getTop']);
    const catSpy = jasmine.createSpyObj('CategoryService', ['getAll']);
    lbSpy.getTop.and.returnValue(of(mockEntries));
    catSpy.getAll.and.returnValue(of(mockCats));

    await TestBed.configureTestingModule({
      imports: [LeaderboardComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: LeaderboardService, useValue: lbSpy },
        { provide: CategoryService,    useValue: catSpy }
      ]
    }).compileComponents();

    lbSvc  = TestBed.inject(LeaderboardService) as jasmine.SpyObj<LeaderboardService>;
    catSvc = TestBed.inject(CategoryService)    as jasmine.SpyObj<CategoryService>;
    fixture    = TestBed.createComponent(LeaderboardComponent);
    component  = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => expect(component).toBeTruthy());

  it('should load entries on init', () => {
    expect(lbSvc.getTop).toHaveBeenCalled();
    expect(component.entries().length).toBe(3);
  });

  it('should load categories on init', () => {
    expect(catSvc.getAll).toHaveBeenCalled();
    expect(component.categories().length).toBe(1);
  });

  it('should set loading to false after data loads', () => {
    expect(component.loading()).toBeFalse();
  });

  describe('filterCat()', () => {
    it('should update selectedCat and reload leaderboard', () => {
      lbSvc.getTop.and.returnValue(of([]));
      component.filterCat('c1');
      expect(component.selectedCat()).toBe('c1');
      expect(lbSvc.getTop).toHaveBeenCalledWith('c1', 20);
    });

    it('should clear filter when empty string passed', () => {
      component.filterCat('');
      expect(component.selectedCat()).toBe('');
    });
  });

  it('should handle load error gracefully', () => {
    lbSvc.getTop.and.returnValue(throwError(() => new Error()));
    component.loadLeaderboard();
    expect(component.loading()).toBeFalse();
  });
});
