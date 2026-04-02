import { Component, signal, computed, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule, DatePipe } from '@angular/common';
import { QuizService } from '../service/quiz.service';
import { AuthService } from '../service/auth.service';
import { QuizDto } from '../models/models';

@Component({
  selector: 'app-evaluator-dashboard',
  standalone: true,
  imports: [RouterLink, CommonModule, DatePipe],
  templateUrl: './evaluator-dashboard.component.html',
  styleUrl: './evaluator-dashboard.component.css',
})
export class EvaluatorDashboardComponent implements OnInit {
  auth = inject(AuthService);
  private quizSvc = inject(QuizService);

  quizzes = signal<QuizDto[]>([]);
  loading = signal(true);

  myQuizzes = computed(() => {
    const uid = this.auth.currentUser()?.userId;
    return this.quizzes().filter(q => q.creatorId === uid);
  });

  activeQuizzes = computed(() =>
    this.myQuizzes().filter(q => !q.isExpired && (!q.deadline || new Date(q.deadline) > new Date()))
  );

  expiredQuizzes = computed(() =>
    this.myQuizzes().filter(q => q.deadline && new Date(q.deadline) <= new Date())
  );

  ngOnInit() {
    this.quizSvc.getAll().subscribe({
      next: q => { this.quizzes.set(q); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  isExpired(quiz: QuizDto): boolean {
    return !!quiz.deadline && new Date(quiz.deadline) <= new Date();
  }
}
