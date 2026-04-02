import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../service/auth.service';

@Component({
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, RouterLink],

})
export class LoginComponent {
  username = '';
  password = '';
  loading = signal(false);
  error = signal('');
  success = signal('');
  showPass = signal(false);

  constructor(private auth: AuthService, private router: Router) {}

  onLogin() {
    this.error.set('');
    this.loading.set(true);
    this.auth.login({ username: this.username, password: this.password }).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.error?.message || 'Invalid credentials. Please try again.');
      }
    });
  }
}
