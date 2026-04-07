import { Component, signal, computed, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { PaymentService } from '../service/payment.service';
import { AuthService } from '../service/auth.service';
import { PaymentResponseDto } from '../models/models';

@Component({
  selector: 'app-payment-history',
  standalone: true,
  imports: [RouterLink, DatePipe],
  templateUrl: './payment-history.component.html',
  styleUrl: './payment-history.component.css'
})
export class PaymentHistoryComponent implements OnInit {
  private paymentSvc = inject(PaymentService);
  private auth = inject(AuthService);

  payments = signal<PaymentResponseDto[]>([]);
  loading = signal(true);
  error = signal('');

  completedCount = computed(() =>
    this.payments().filter(p => p.status === 'Completed').length
  );

  totalSpent = computed(() =>
    this.payments()
      .filter(p => p.status === 'Completed')
      .reduce((sum, p) => sum + p.amount, 0)
      .toFixed(2)
  );

  get userId() { return this.auth.currentUser()?.userId ?? ''; }

  ngOnInit() {
    this.paymentSvc.getMyPayments(this.userId).subscribe({
      next: (p) => { this.payments.set(p); this.loading.set(false); },
      error: () => { this.error.set('Failed to load payment history.'); this.loading.set(false); }
    });
  }

  statusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'completed': return 'status-completed';
      case 'pending':   return 'status-pending';
      default:          return 'status-failed';
    }
  }

  statusIcon(status: string): string {
    switch (status.toLowerCase()) {
      case 'completed': return '✅';
      case 'pending':   return '⏳';
      default:          return '❌';
    }
  }
}
