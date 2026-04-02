import { Component, signal, computed, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { DecimalPipe } from '@angular/common';
import { UserService } from '../service/user.service';
import { AuthService } from '../service/auth.service';
import { AttemptService } from '../service/attempt.service';
import { QuizService } from '../service/quiz.service';
import { UpdateUserDto, AttemptResultDto, QuizDto } from '../models/models';

@Component({
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css',
  selector: 'app-profile',
  standalone: true,
  imports: [FormsModule, RouterLink, DecimalPipe],
})
export class ProfileComponent implements OnInit {
  auth = inject(AuthService);
  private userSvc = inject(UserService);
  private attemptSvc = inject(AttemptService);
  private quizSvc = inject(QuizService);

  form: UpdateUserDto = {};
  saving = signal(false);
  success = signal('');
  error = signal('');

  attempts = signal<AttemptResultDto[]>([]);
  quizMap = signal<Record<string, QuizDto>>({});
  attemptsLoading = signal(true);

  getQuizName(quizId: string): string {
    return this.quizMap()[quizId]?.quizName ?? 'Unknown Quiz';
  }

  ngOnInit() {
    const id = this.auth.currentUser()?.userId;
    if (id) {
      this.userSvc.getById(id).subscribe({
        next: (u) => {
          this.form = {
            name: u.name, phoneNumber: u.phoneNumber,
            addressLine1: u.addressLine1, addressLine2: u.addressLine2,
            state: u.state, city: u.city, pincode: u.pincode
          };
        }, error: () => {}
      });

      this.attemptSvc.getMyAttempts(id).subscribe({
        next: (list) => {
          this.attempts.set(list);
          // Fetch quiz names for all unique quizIds
          const uniqueIds = [...new Set(list.map(a => a.quizId))];
          let loaded = 0;
          if (uniqueIds.length === 0) { this.attemptsLoading.set(false); return; }
          uniqueIds.forEach(qid => {
            this.quizSvc.get(qid).subscribe({
              next: (q) => {
                this.quizMap.update(m => ({ ...m, [qid]: q }));
                if (++loaded === uniqueIds.length) this.attemptsLoading.set(false);
              },
              error: () => { if (++loaded === uniqueIds.length) this.attemptsLoading.set(false); }
            });
          });
        },
        error: () => this.attemptsLoading.set(false)
      });
    }
  }

  onSave() {
    this.success.set(''); this.error.set('');
    const id = this.auth.currentUser()?.userId;
    if (!id) return;
    this.saving.set(true);
    this.userSvc.update(id, this.form).subscribe({
      next: () => { this.saving.set(false); this.success.set('Profile updated successfully!'); },
      error: () => { this.saving.set(false); this.error.set('Failed to update profile.'); }
    });
  }
}
