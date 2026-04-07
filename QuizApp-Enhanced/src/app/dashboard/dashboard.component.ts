import { Component, signal, computed, OnInit, inject } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { DecimalPipe, DatePipe } from '@angular/common';
import { QuizService } from '../service/quiz.service';
import { CategoryService } from '../service/category.service';
import { LeaderboardService } from '../service/leaderboard.service';
import { AuthService } from '../service/auth.service';
import { QuizDto, CategoryDto, LeaderboardDto } from '../models/models';
import { SlicePipe } from '@angular/common';

@Component({
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, DecimalPipe, SlicePipe, DatePipe],
})
export class DashboardComponent implements OnInit {
  auth = inject(AuthService);
  private quizSvc = inject(QuizService);
  private catSvc = inject(CategoryService);
  private lbSvc = inject(LeaderboardService);
  private router = inject(Router);

  quizzes = signal<QuizDto[]>([]);
  categories = signal<CategoryDto[]>([]);
  leaderboard = signal<LeaderboardDto[]>([]);
  loading = signal(true);
  selectedCat = signal('');
  selectedDiff = signal('');
  deleteConfirmId = signal<string | null>(null);
  deleteError = signal('');

  myQuizzes = computed(() => {
    const userId = this.auth.currentUser()?.userId;
    if (!userId) return this.quizzes();
    return this.quizzes().filter(q => (q as any).creatorId === userId || (q as any).userId === userId);
  });

  filteredQuizzes = computed(() => {
    const cat = this.selectedCat();
    const diff = this.selectedDiff();
    let list = this.quizzes().filter(q => q.creatorRole !== 'Evaluator');
    if (cat)  list = list.filter(q => q.category?.categoryId === cat);
    if (diff) list = list.filter(q => q.difficultyLevel?.toLowerCase() === diff.toLowerCase());
    return list;
  });

  // Evaluator quizzes allocated to this student
  evaluatorQuizzes = computed(() =>
    this.quizzes().filter(q => q.creatorRole === 'Evaluator')
  );

  // Pagination
  readonly pageSize = 6;
  currentPage = signal(1);

  totalPages = computed(() =>
    Math.ceil(this.filteredQuizzes().length / this.pageSize) || 1
  );

  pageNumbers = computed(() =>
    Array.from({ length: this.totalPages() }, (_, i) => i + 1)
  );

  paginatedQuizzes = computed(() => {
    const start = (this.currentPage() - 1) * this.pageSize;
    return this.filteredQuizzes().slice(start, start + this.pageSize);
  });

  goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages()) this.currentPage.set(page);
  }

  onCategoryChange(catId: string) {
    this.selectedCat.set(catId);
    this.currentPage.set(1);
  }

  onDifficultyChange(diff: string) {
    this.selectedDiff.set(diff);
    this.currentPage.set(1);
  }

  ngOnInit() {
    // Redirect evaluator to their dedicated dashboard
    if (this.auth.isEvaluator()) {
      this.router.navigate(['/evaluator/dashboard']);
      return;
    }
    this.catSvc.getAll().subscribe({ next: c => this.categories.set(c), error: () => {} });
    this.lbSvc.getTop(undefined, 10).subscribe({ next: l => this.leaderboard.set(l), error: () => {} });
    const userId = this.auth.currentUser()?.userId;
    this.quizSvc.getAll(undefined, userId).subscribe({
      next: q => { this.quizzes.set(q); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  confirmDelete(quizId: string) {
    this.deleteConfirmId.set(quizId);
    this.deleteError.set('');
  }

  cancelDelete() {
    this.deleteConfirmId.set(null);
    this.deleteError.set('');
  }

  deleteQuiz(quizId: string) {
    this.quizSvc.delete(quizId).subscribe({
      next: () => {
        this.quizzes.update(list => list.filter(q => q.quizId !== quizId));
        this.deleteConfirmId.set(null);
      },
      error: (err) => {
        this.deleteError.set('Failed to delete quiz. Please try again.');
        console.error(err);
      }
    });
  }

  manageQuestions(quizId: string) {
    this.router.navigate(['/admin/quiz', quizId, 'questions']);
  }

  editQuiz(quizId: string) {
    this.router.navigate(['/admin/quiz/edit', quizId]);
  }
}
