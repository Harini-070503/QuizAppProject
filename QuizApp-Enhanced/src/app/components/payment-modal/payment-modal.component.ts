import { Component, Input, Output, EventEmitter, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PaymentService } from '../../service/payment.service';
import { AuthService } from '../../service/auth.service';
import { PaymentResponseDto } from '../../models/models';

type Step = 'choose' | 'confirm' | 'done';
type Plan = 'monthly' | 'retry';

@Component({
  selector: 'app-payment-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './payment-modal.component.html',
  styleUrl: './payment-modal.component.css'
})
export class PaymentModalComponent {
  @Input() quizId!: string;
  @Input() quizName!: string;
  @Output() paymentConfirmed = new EventEmitter<void>();
  @Output() closed = new EventEmitter<void>();

  private paymentSvc = inject(PaymentService);
  private auth = inject(AuthService);

  step = signal<Step>('choose');
  selectedPlan = signal<Plan>('monthly');
  loading = signal(false);
  error = signal('');
  payment = signal<PaymentResponseDto | null>(null);
  transactionRef = signal('');

  get userId() { return this.auth.currentUser()?.userId ?? ''; }

  selectPlan(plan: Plan) {
    this.selectedPlan.set(plan);
    this.error.set('');
  }

  proceed() {
    this.loading.set(true);
    this.error.set('');

    const obs = this.selectedPlan() === 'monthly'
      ? this.paymentSvc.initiateMonthly({ userId: this.userId })
      : this.paymentSvc.initiate({ userId: this.userId, quizId: this.quizId });

    obs.subscribe({
      next: (p) => { this.payment.set(p); this.loading.set(false); this.step.set('confirm'); },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.message ?? 'Failed to initiate payment.');
      }
    });
  }

  confirm() {
    const p = this.payment();
    if (!p || !this.transactionRef().trim()) {
      this.error.set('Please enter the transaction reference.');
      return;
    }
    this.loading.set(true);
    this.error.set('');
    this.paymentSvc.confirm({ paymentId: p.paymentId, transactionRef: this.transactionRef() }).subscribe({
      next: () => { this.loading.set(false); this.step.set('done'); },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.message ?? 'Payment confirmation failed.');
      }
    });
  }

  onRetryNow() { this.paymentConfirmed.emit(); }
  close() { this.closed.emit(); }
}
