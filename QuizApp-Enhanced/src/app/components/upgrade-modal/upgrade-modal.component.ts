import { Component, Output, EventEmitter, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserService } from '../../service/user.service';
import { AuthService } from '../../service/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-upgrade-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './upgrade-modal.component.html',
  styleUrl: './upgrade-modal.component.css'
})
export class UpgradeModalComponent {
  @Output() closed = new EventEmitter<void>();
  @Output() upgraded = new EventEmitter<void>();

  private userSvc = inject(UserService);
  private auth = inject(AuthService);
  private router = inject(Router);

  loading = signal(false);
  error = signal('');
  done = signal(false);

  get userId() { return this.auth.currentUser()?.userId ?? ''; }

  upgrade() {
    this.loading.set(true);
    this.error.set('');
    this.userSvc.upgradeToPremium(this.userId).subscribe({
      next: (res) => {
        this.loading.set(false);
        this.done.set(true);
        // Re-store the new token so the JWT has PremiumTaker role
        this.auth.storeToken(res.token);
        this.upgraded.emit();
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.message ?? 'Upgrade failed. Please try again.');
      }
    });
  }

  goToDashboard() {
    this.closed.emit();
    this.router.navigate(['/dashboard']);
  }
}
