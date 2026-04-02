import { Component, signal, computed, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { QuizService } from '../service/quiz.service';
import { AttemptService } from '../service/attempt.service';
import { AuthService } from '../service/auth.service';
import { ThemeService } from '../service/theme.service';
import { GroupService } from '../service/group.service';
import { QuizDto, QuestionDto, AttemptAnswerItemDto } from '../models/models';

export type Difficulty = 'All' | 'Easy' | 'Medium' | 'Hard';

interface QuestionState {
  question: QuestionDto;
  questionNumber: number;
  selectedAnswer: string | null;
  status: 'pending' | 'attended' | 'current' | 'skipped';
}

@Component({
  templateUrl: './quiz.component.html',
  styleUrl: './quiz.component.css',
  selector: 'app-quiz',
  standalone: true,
  imports: [CommonModule, FormsModule, DatePipe],
})
export class QuizComponent implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private quizSvc = inject(QuizService);
  private attemptSvc = inject(AttemptService);
  private auth = inject(AuthService);
  public themeService = inject(ThemeService);
  private groupSvc = inject(GroupService);

  hasAccess = signal(true); // assume open until checked

  quiz = signal<QuizDto | null>(null);
  allQuestionStates = signal<QuestionState[]>([]);  // full unfiltered list
  questionStates = signal<QuestionState[]>([]);      // filtered/active list
  currentIndex = signal(0);

  selectedDifficulty = signal<Difficulty>('All');
  readonly difficulties: Difficulty[] = ['All', 'Easy', 'Medium', 'Hard'];

  totalTimeLimit = signal(0);
  timeRemaining = signal(0);

  loading = signal(true);
  error = signal('');
  submitting = signal(false);
  hasStarted = signal(false);
  scratchpad = '';

  private timerInterval: any;
  private startedAt = new Date().toISOString();

  currentQuestion = computed(() => this.questionStates()[this.currentIndex()] ?? null);

  attendedCount = computed(() =>
    this.questionStates().filter(q => q.selectedAnswer !== null).length
  );

  pendingCount = computed(() =>
    this.questionStates().filter(q => q.selectedAnswer === null && q.status !== 'skipped').length
  );

  skippedCount = computed(() =>
    this.questionStates().filter(q => q.status === 'skipped').length
  );

  formattedTime = computed(() => {
    const s = this.timeRemaining();
    return `${Math.floor(s / 60).toString().padStart(2, '0')}:${(s % 60).toString().padStart(2, '0')}`;
  });

  timerClass = computed(() => {
    const r = this.timeRemaining();
    if (r <= 60) return 'danger';
    if (r <= 300) return 'warning';
    return '';
  });

  availableOptions = computed(() => {
    const current = this.currentQuestion();
    if (!current) return [];
    const opts: string[] = ['A', 'B'];
    if (current.question.options?.optionC) opts.push('C');
    if (current.question.options?.optionD) opts.push('D');
    return opts;
  });

  // Difficulty badge config
  difficultyConfig: Record<Difficulty, { label: string; color: string }> = {
    All:    { label: 'All',    color: '' },
    Easy:   { label: 'Easy',   color: 'easy' },
    Medium: { label: 'Medium', color: 'medium' },
    Hard:   { label: 'Hard',   color: 'hard' },
  };

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.loadQuiz(id);
  }

  loadQuiz(id: string) {
    this.loading.set(true);
    this.quizSvc.get(id).subscribe({
      next: (q) => {
        this.quiz.set(q);
        // Check group access if quiz is restricted
        if (q.groupId) {
          this.groupSvc.checkAccess(id).subscribe({
            next: (ok) => this.hasAccess.set(ok),
            error: () => this.hasAccess.set(false)
          });
        } else {
          this.hasAccess.set(true);
        }
        const questions = q.questions ?? [];
        const states: QuestionState[] = questions.map((question, index) => ({
          question,
          questionNumber: index + 1,
          selectedAnswer: null,
          status: index === 0 ? 'current' : 'pending'
        }));
        this.allQuestionStates.set(states);
        this.questionStates.set(states);

        // Pre-select difficulty to match quiz's own difficulty level
        const qDiff = q.difficultyLevel as Difficulty;
        if (qDiff && this.difficulties.includes(qDiff)) {
          this.selectedDifficulty.set(qDiff);
        }

        this.loading.set(false);
        if (q.timeLimit) this.totalTimeLimit.set(q.timeLimit * 60);
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Quiz not found or failed to load.');
      }
    });
  }

  applyDifficultyFilter(diff: Difficulty) {
    this.selectedDifficulty.set(diff);
    const all = this.allQuestionStates();
    const quizDiff = this.quiz()?.difficultyLevel;

    // If quiz difficulty matches filter (or All), show all questions
    // Otherwise show empty (no questions match a different difficulty)
    let filtered: QuestionState[];
    if (diff === 'All' || diff === quizDiff) {
      filtered = all;
    } else {
      filtered = [];
    }

    // Re-number and reset statuses
    const renumbered = filtered.map((s, i) => ({
      ...s,
      questionNumber: i + 1,
      status: (i === 0 ? 'current' : 'pending') as QuestionState['status']
    }));
    this.questionStates.set(renumbered);
    this.currentIndex.set(0);
  }

  startQuiz() {
    this.hasStarted.set(true);
    this.startedAt = new Date().toISOString();
    if (this.totalTimeLimit() > 0) {
      this.timeRemaining.set(this.totalTimeLimit());
      this.startTimer();
    }
  }

  startTimer() {
    clearInterval(this.timerInterval);
    this.timerInterval = setInterval(() => {
      const remaining = this.timeRemaining() - 1;
      if (remaining <= 0) {
        this.stopTimer();
        alert('Time is up! Submitting your quiz...');
        this.submitQuiz();
      } else {
        this.timeRemaining.set(remaining);
      }
    }, 1000);
  }

  stopTimer() {
    if (this.timerInterval) clearInterval(this.timerInterval);
  }

  selectAnswer(option: string) {
    const states = [...this.questionStates()];
    const idx = this.currentIndex();
    if (idx >= 0 && idx < states.length) {
      states[idx] = { ...states[idx], selectedAnswer: option, status: 'attended' };
      this.questionStates.set(states);
    }
  }

  getOptionText(option: string): string {
    const current = this.currentQuestion();
    if (!current?.question.options) return '';
    const opts = current.question.options;
    switch (option) {
      case 'A': return opts.optionA;
      case 'B': return opts.optionB;
      case 'C': return opts.optionC || '';
      case 'D': return opts.optionD || '';
      default: return '';
    }
  }

  goToQuestion(index: number) {
    const states = [...this.questionStates()];
    if (index < 0 || index >= states.length) return;

    const cur = this.currentIndex();
    if (cur >= 0 && cur < states.length && states[cur].status === 'current') {
      states[cur] = {
        ...states[cur],
        status: states[cur].selectedAnswer ? 'attended' : 'pending'
      };
    }
    states[index] = { ...states[index], status: 'current' };
    this.questionStates.set(states);
    this.currentIndex.set(index);
    this.scratchpad = '';   // clear working space for each new question
  }

  nextQuestion() {
    const idx = this.currentIndex();
    if (idx < this.questionStates().length - 1) this.goToQuestion(idx + 1);
  }

  skipQuestion() {
    const states = [...this.questionStates()];
    const idx = this.currentIndex();
    if (idx < 0 || idx >= states.length) return;
    // Mark current as skipped (clears any selected answer), then find next
    states[idx] = { ...states[idx], selectedAnswer: null, status: 'skipped' };
    // Find next unanswered/unskipped question
    let next = states.findIndex((s, i) => i > idx && s.status !== 'skipped' && s.status !== 'attended');
    if (next === -1) next = states.findIndex((s, i) => i !== idx && s.status !== 'skipped' && s.status !== 'attended');
    if (next !== -1 && next !== idx) {
      states[next] = { ...states[next], status: 'current' };
      this.questionStates.set(states);
      this.currentIndex.set(next);
    } else if (idx < states.length - 1) {
      states[idx + 1] = { ...states[idx + 1], status: 'current' };
      this.questionStates.set(states);
      this.currentIndex.set(idx + 1);
    } else {
      this.questionStates.set(states);
    }
    this.scratchpad = '';
  }

  previousQuestion() {
    const idx = this.currentIndex();
    if (idx > 0) this.goToQuestion(idx - 1);
  }

  canSubmit(): boolean {
    return this.questionStates().every(q => q.selectedAnswer !== null);
  }

  submitQuiz() {
    if (this.questionStates().length === 0) {
      this.error.set('No questions available for the selected difficulty.');
      return;
    }
    if (!this.canSubmit()) {
      const unanswered = this.pendingCount();
      if (!confirm(`You have ${unanswered} unanswered question(s). Submit anyway?`)) return;
    }

    this.stopTimer();
    this.submitting.set(true);

    const userId = this.auth.currentUser()?.userId;
    const quizId = this.quiz()?.quizId;
    if (!userId || !quizId) {
      this.error.set('User not logged in or quiz data is invalid.');
      this.submitting.set(false);
      return;
    }

    const answers: AttemptAnswerItemDto[] = this.questionStates().map(s => ({
      questionId: s.question.questionId,
      chosenOption: s.selectedAnswer || 'A'
    }));

    this.attemptSvc.submit({
      quizId, userId, answers,
      startedAtUtc: this.startedAt,
      endedAtUtc: new Date().toISOString()
    }).subscribe({
      next: (res) => {
        this.submitting.set(false);
        // Pass full result + quiz via router state so result page
        // doesn't need to re-fetch (GET endpoint may not return feedback)
        this.router.navigate(['/result', res.attemptAnswerId], {
          state: { result: res, quiz: this.quiz() }
        });
      },
      error: (err) => {
        this.submitting.set(false);
        let msg = 'Failed to submit quiz. Please try again.';
        if (err.status === 0) msg = 'Cannot connect to server.';
        else if (err.status === 401) msg = 'Session expired. Please login again.';
        else if (err.error?.message) msg = err.error.message;
        this.error.set(msg);
      }
    });
  }

  clearScratchpad() {
    this.scratchpad = '';
  }

  isQuizExpired(): boolean {
    const deadline = this.quiz()?.deadline;
    return !!deadline && new Date(deadline) <= new Date();
  }

  exitQuiz() {
    if (this.hasStarted() && !confirm('Exit quiz? Your progress will be lost.')) return;
    this.stopTimer();
    this.router.navigate(['/dashboard']);
  }

  ngOnDestroy() {
    this.stopTimer();
  }
}
