import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../service/auth.service';

@Component({
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule, RouterLink],

})
export class RegisterComponent {
  name = '';
  username = '';
  email = '';
  password = '';
  role = 'Taker';
  loading = signal(false);
  error = signal('');
  showPass = signal(false);

  constructor(private auth: AuthService, private router: Router) {}

  onRegister() {
    this.error.set('');
    this.loading.set(true);
    this.auth.register({ username: this.username, email: this.email, password: this.password, role: this.role, name: this.name || undefined }).subscribe({
      next: () => { this.loading.set(false); this.router.navigate(['/dashboard']); },
      error: (err) => { this.loading.set(false); this.error.set(err?.error?.message || 'Registration failed. Please try again.'); }
    });
  }
}
