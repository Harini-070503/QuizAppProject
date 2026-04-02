import { TestBed, ComponentFixture } from '@angular/core/testing';
import { provideRouter, ActivatedRoute } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';
import { AttemptResultComponent } from './attempt-result.component';
import { AttemptService } from '../service/attempt.service';
import { AttemptResultDto } from '../models/models';

describe('AttemptResultComponent', () => {
  let component: AttemptResultComponent;
  let fixture: ComponentFixture<AttemptResultComponent>;
  let attemptSvc: jasmine.SpyObj<AttemptService>;

  const mockResult: AttemptResultDto = {
    attemptAnswerId: 'att-1',
    quizId: 'quiz-1',
    totalMark: 7,
    percentage: 70,
    feedback: [
      { questionId: 'q1', correctOption: 'A', yourOption: 'A', isCorrect: true },
      { questionId: 'q2', correctOption: 'B', yourOption: 'C', isCorrect: false },
      { questionId: 'q3', correctOption: 'A', yourOption: 'A', isCorrect: true }
    ]
  };

  beforeEach(async () => {
    const attemptSpy = jasmine.createSpyObj('AttemptService', ['get']);
    attemptSpy.get.and.returnValue(of(mockResult));

    await TestBed.configureTestingModule({
      imports: [AttemptResultComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: AttemptService, useValue: attemptSpy },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: { get: () => 'att-1' } } }
        }
      ]
    }).compileComponents();

    attemptSvc = TestBed.inject(AttemptService) as jasmine.SpyObj<AttemptService>;
    fixture    = TestBed.createComponent(AttemptResultComponent);
    component  = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => expect(component).toBeTruthy());

  it('should load attempt result on init', () => {
    expect(attemptSvc.get).toHaveBeenCalledWith('att-1');
    expect(component.result()?.attemptAnswerId).toBe('att-1');
  });

  it('should set loading to false after result loads', () => {
    expect(component.loading()).toBeFalse();
  });

  it('should compute correct count from feedback', () => {
    expect(component.correctCount()).toBe(2);
  });

  it('should compute wrong count from feedback', () => {
    expect(component.wrongCount()).toBe(1);
  });

  describe('ringOffset()', () => {
    it('should return 0 offset for 100%', () => {
      component.result.set({ ...mockResult, percentage: 100 });
      expect(component.ringOffset()).toBe(0);
    });

    it('should return full circumference for 0%', () => {
      component.result.set({ ...mockResult, percentage: 0 });
      expect(component.ringOffset()).toBe(326.7);
    });

    it('should compute partial offset for 70%', () => {
      const expected = 326.7 - (70 / 100) * 326.7;
      expect(component.ringOffset()).toBeCloseTo(expected, 1);
    });
  });

  describe('ringColor()', () => {
    it('should return green (#34d399) for >= 80%', () => {
      component.result.set({ ...mockResult, percentage: 85 });
      expect(component.ringColor()).toBe('#34d399');
    });

    it('should return amber (#fbbf24) for 50-79%', () => {
      component.result.set({ ...mockResult, percentage: 65 });
      expect(component.ringColor()).toBe('#fbbf24');
    });

    it('should return red (#f87171) for < 50%', () => {
      component.result.set({ ...mockResult, percentage: 40 });
      expect(component.ringColor()).toBe('#f87171');
    });
  });

  it('should handle error gracefully when attempt not found', () => {
    attemptSvc.get.and.returnValue(throwError(() => new Error('Not found')));
    component.ngOnInit();
    expect(component.loading()).toBeFalse();
    expect(component.result()).toBeNull();
  });
});
