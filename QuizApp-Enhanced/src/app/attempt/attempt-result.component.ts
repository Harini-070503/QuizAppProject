import { Component, signal, computed, OnInit, inject, ElementRef, ViewChild } from '@angular/core';
import { ActivatedRoute, RouterLink, Router } from '@angular/router';
import { AttemptService } from '../service/attempt.service';
import { QuizService } from '../service/quiz.service';
import { AuthService } from '../service/auth.service';
import { CoinService } from '../service/coin.service';
import { AttemptResultDto, QuizDto } from '../models/models';
import { DecimalPipe } from '@angular/common';

@Component({
  templateUrl: './attempt-result.component.html',
  styleUrl: './attempt-result.component.css',
  selector: 'app-attempt-result',
  standalone: true,
  imports: [RouterLink, DecimalPipe],
})
export class AttemptResultComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private attemptSvc = inject(AttemptService);
  private quizSvc = inject(QuizService);
  private auth = inject(AuthService);
  coinSvc = inject(CoinService);
  get isEvaluator() { return this.auth.isEvaluator(); }

  @ViewChild('certCanvas') certCanvas!: ElementRef<HTMLCanvasElement>;

  result = signal<AttemptResultDto | null>(null);
  quiz   = signal<QuizDto | null>(null);
  loading = signal(true);
  showCertificate = signal(false);
  coinsEarned = signal(0);

  correctCount = computed(() => {
    const fb = this.result()?.feedback;
    if (fb && fb.length > 0) return fb.filter(f => f.isCorrect).length;
    // Fallback: derive from totalMark when feedback is unavailable
    return this.result()?.totalMark ?? 0;
  });

  wrongCount = computed(() => {
    const fb = this.result()?.feedback;
    if (fb && fb.length > 0) return fb.filter(f => !f.isCorrect).length;
    // Fallback: derive from quiz total questions minus correct
    const total = this.quiz()?.totalQuestion ?? 0;
    const correct = this.result()?.totalMark ?? 0;
    return Math.max(0, total - correct);
  });

  questionTextMap = computed<Record<string, string>>(() => {
    const questions = this.quiz()?.questions ?? [];
    return Object.fromEntries(questions.map(q => [q.questionId, q.questionText]));
  });

  ringOffset() {
    const pct = this.result()?.percentage ?? 0;
    return 326.7 - (pct / 100) * 326.7;
  }

  ringColor() {
    const pct = this.result()?.percentage ?? 0;
    if (pct >= 80) return '#3daa6e';
    if (pct >= 50) return '#d4900a';
    return '#e05252';
  }

  getOptionsForQuestion(questionId: string): { key: string; value: string }[] {
    const q = this.quiz()?.questions?.find(x => x.questionId === questionId);
    if (!q?.options) return [];
    const opts = q.options;
    const result: { key: string; value: string }[] = [
      { key: 'A', value: opts.optionA },
      { key: 'B', value: opts.optionB },
    ];
    if (opts.optionC) result.push({ key: 'C', value: opts.optionC });
    if (opts.optionD) result.push({ key: 'D', value: opts.optionD });
    return result;
  }

  getCorrectOption(questionId: string): string {
    const q = this.quiz()?.questions?.find(x => x.questionId === questionId);
    return q?.options?.correctOption ?? '';
  }

  awardCoins() {
    const r = this.result();
    if (!r) return;
    const earned = this.coinSvc.calcCoins(this.correctCount(), r.percentage);
    this.coinsEarned.set(earned);
    this.coinSvc.addCoins(earned);
  }

  viewCertificate() {
    this.showCertificate.set(true);
    setTimeout(() => this.drawCertificate(), 50);
  }

  hideCertificate() {
    this.showCertificate.set(false);
  }

  drawCertificate() {
    const canvas = this.certCanvas?.nativeElement;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    const W = 900, H = 640;
    canvas.width = W; canvas.height = H;

    const pct   = Math.round(this.result()?.percentage ?? 0);
    const name  = this.auth.currentUser()?.username ?? 'Participant';
    const quiz  = this.quiz()?.quizName ?? 'Quiz';
    const cat   = this.quiz()?.category?.categoryName ?? '';
    const date  = new Date().toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' });
    const grade = pct >= 80 ? 'Distinction' : pct >= 60 ? 'First class' : pct >= 40 ? 'Pass' : 'Participation';
    const gradeColor = pct >= 80 ? '#2a7a4f' : pct >= 60 ? '#b87a00' : pct >= 40 ? '#1a5c8a' : '#666';

    // Background
    ctx.fillStyle = '#fdfaf4';
    ctx.fillRect(0, 0, W, H);

    // Outer border
    ctx.strokeStyle = '#c8a84b'; ctx.lineWidth = 10;
    ctx.strokeRect(14, 14, W - 28, H - 28);

    // Inner border
    ctx.strokeStyle = '#e8c96a'; ctx.lineWidth = 2;
    ctx.strokeRect(26, 26, W - 52, H - 52);

    // Corner dots
    [[40,40],[W-40,40],[40,H-40],[W-40,H-40]].forEach(([cx,cy]) => {
      ctx.fillStyle = '#c8a84b';
      ctx.beginPath(); ctx.arc(cx, cy, 8, 0, Math.PI * 2); ctx.fill();
    });

    // Header gradient band
    const grad = ctx.createLinearGradient(0, 50, W, 120);
    grad.addColorStop(0, '#1a2a6c'); grad.addColorStop(0.5, '#2d4a9e'); grad.addColorStop(1, '#1a2a6c');
    ctx.fillStyle = grad; ctx.fillRect(40, 50, W - 80, 70);

    ctx.fillStyle = '#ffd700'; ctx.font = 'bold 28px Georgia, serif';
    ctx.textAlign = 'center'; ctx.fillText('⚡ QuizZap', W / 2, 95);

    ctx.fillStyle = '#2c1a00'; ctx.font = 'italic 18px Georgia, serif';
    ctx.fillText('Certificate of Completion', W / 2, 155);

    ctx.strokeStyle = '#c8a84b'; ctx.lineWidth = 1.5;
    ctx.beginPath(); ctx.moveTo(180, 168); ctx.lineTo(W - 180, 168); ctx.stroke();

    ctx.fillStyle = '#555'; ctx.font = '15px Georgia, serif';
    ctx.fillText('This is to certify that', W / 2, 200);

    ctx.fillStyle = '#1a2a6c'; ctx.font = 'bold 38px Georgia, serif';
    ctx.fillText(name, W / 2, 250);

    const nameW = ctx.measureText(name).width;
    ctx.strokeStyle = '#c8a84b'; ctx.lineWidth = 1.5;
    ctx.beginPath(); ctx.moveTo(W/2 - nameW/2, 258); ctx.lineTo(W/2 + nameW/2, 258); ctx.stroke();

    ctx.fillStyle = '#555'; ctx.font = '15px Georgia, serif';
    ctx.fillText('has successfully completed', W / 2, 295);

    ctx.fillStyle = '#2c1a00'; ctx.font = 'bold 22px Georgia, serif';
    ctx.fillText(`"${quiz}"`, W / 2, 330);

    if (cat) {
      ctx.fillStyle = '#777'; ctx.font = '14px Georgia, serif';
      ctx.fillText(`Category: ${cat}`, W / 2, 358);
    }

    // Score box
    ctx.fillStyle = '#f5f0e8'; ctx.strokeStyle = '#c8a84b'; ctx.lineWidth = 1;
    const boxY = 378;
    ctx.beginPath(); (ctx as any).roundRect(W/2 - 200, boxY, 400, 80, 10); ctx.fill(); ctx.stroke();

    ctx.fillStyle = '#1a2a6c'; ctx.font = 'bold 32px Georgia, serif';
    ctx.fillText(`${pct}%`, W/2 - 80, boxY + 50);
    ctx.fillStyle = '#555'; ctx.font = '13px Georgia, serif';
    ctx.fillText('Score', W/2 - 80, boxY + 68);

    ctx.strokeStyle = '#c8a84b'; ctx.lineWidth = 1;
    ctx.beginPath(); ctx.moveTo(W/2, boxY + 15); ctx.lineTo(W/2, boxY + 65); ctx.stroke();

    ctx.fillStyle = gradeColor; ctx.font = 'bold 26px Georgia, serif';
    ctx.fillText(grade, W/2 + 80, boxY + 50);
    ctx.fillStyle = '#555'; ctx.font = '13px Georgia, serif';
    ctx.fillText('Grade', W/2 + 80, boxY + 68);

    ctx.fillStyle = '#888'; ctx.font = '13px Georgia, serif';
    ctx.fillText(`Issued on: ${date}`, W / 2, 490);

    ctx.strokeStyle = '#c8a84b'; ctx.lineWidth = 1.5;
    ctx.beginPath(); ctx.moveTo(180, 505); ctx.lineTo(W - 180, 505); ctx.stroke();

    // Seal
    ctx.beginPath(); ctx.arc(W / 2, 565, 42, 0, Math.PI * 2);
    ctx.fillStyle = '#1a2a6c'; ctx.fill();
    ctx.strokeStyle = '#ffd700'; ctx.lineWidth = 3; ctx.stroke();
    ctx.fillStyle = '#ffd700'; ctx.font = 'bold 13px Georgia, serif';
    ctx.fillText('CERTIFIED', W / 2, 560);
    ctx.font = '11px Georgia, serif'; ctx.fillText('QuizZap', W / 2, 578);
  }

  downloadCertificate() {
    const canvas = this.certCanvas?.nativeElement;
    if (!canvas) return;
    const link = document.createElement('a');
    const name = (this.auth.currentUser()?.username ?? 'certificate').replace(/\s+/g, '-');
    const quiz = (this.quiz()?.quizName ?? 'quiz').replace(/\s+/g, '-');
    link.download = `${name}-${quiz}-certificate.png`.toLowerCase();
    link.href = canvas.toDataURL('image/png');
    link.click();
  }

  ngOnInit() {
    const nav = this.router.getCurrentNavigation();
    const state = nav?.extras?.state ?? history.state;

    if (state?.result && state?.quiz) {
      this.result.set(state.result as AttemptResultDto);
      this.quiz.set(state.quiz as QuizDto);
      this.loading.set(false);
      this.awardCoins();
      return;
    }

    // Fallback: fetch from API (feedback may be empty depending on backend)
    const id = this.route.snapshot.paramMap.get('attemptId')!;
    this.attemptSvc.get(id).subscribe({
      next: r => {
        this.result.set(r);
        this.quizSvc.get(r.quizId).subscribe({
          next: q => { this.quiz.set(q); this.loading.set(false); this.awardCoins(); },
          error: () => this.loading.set(false)
        });
      },
      error: () => this.loading.set(false)
    });
  }
}
