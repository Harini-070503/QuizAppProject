import { Component, OnInit, signal, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { UserService } from '../service/user.service';
import { AuthService } from '../service/auth.service';
import { UpdateUserDto } from '../models/models';

@Component({
  templateUrl: './user-details.component.html',
  styleUrl: './user-details.component.css',
  selector: 'app-user-details',
  standalone: true,
  imports: [FormsModule, RouterLink],

})
export class UserDetailsComponent implements OnInit {
  auth = inject(AuthService);
  private userSvc = inject(UserService);

  form: UpdateUserDto = {};
  saving = signal(false);
  success = signal('');
  error = signal('');

  ngOnInit() {
    const id = this.auth.currentUser()?.userId;
    if (id) {
      this.userSvc.getById(id).subscribe({
        next: u => {
          this.form = {
            name: u.name,
            phoneNumber: u.phoneNumber,
            addressLine1: u.addressLine1,
            addressLine2: u.addressLine2,
            state: u.state,
            city: u.city,
            pincode: u.pincode
          };
        },
        error: () => {}
      });
    }
  }

  onSave() {
    this.success.set('');
    this.error.set('');
    const id = this.auth.currentUser()?.userId;
    if (!id) return;
    this.saving.set(true);
    this.userSvc.update(id, this.form).subscribe({
      next: () => {
        this.saving.set(false);
        this.success.set('Details updated successfully!');
      },
      error: () => {
        this.saving.set(false);
        this.error.set('Failed to update details. Please try again.');
      }
    });
  }
}
