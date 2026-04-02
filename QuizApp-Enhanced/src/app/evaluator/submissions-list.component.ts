import { Component, signal, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { AttemptService } from '../service/attempt.service';
import { QuizService } from '../service/quiz.service';
import { SubmissionDetailDto } from '../models/models';

@Component({
  selector: 'app-submissions-list',
  standalone: true,
  imports: [RouterLink, CommonModule, DatePipe, DecimalPipe],
  templateUrl: './submissions-list.component.html',
  styleUrl: './submissions-list.component.css',
})
export class SubmissionsListComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private attemptSvc = inject(AttemptService);
  private quizSvc = inject(QuizService);

  quizId = signal('');
  quizName = signal('');
  submissions = signal<SubmissionDetailDto[]>([]);
  loading = signal(true);
  error = signal('');

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('quizId')!;
    this.quizId.set(id);
    this.quizSvc.get(id).subscribe({ next: q => this.quizName.set(q.quizName), error: () => {} });
    this.attemptSvc.getSubmissionsByQuiz(id).subscribe({
      next: s => { this.submissions.set(s); this.loading.set(false); },
      error: () => { this.error.set('Failed to load submissions.'); this.loading.set(false); }
    });
  }

  viewDetail(attemptId: string) {
    this.router.navigate(['/evaluator/submission', attemptId]);
  }
}
