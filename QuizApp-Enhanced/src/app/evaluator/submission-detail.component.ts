import { Component, signal, computed, OnInit, inject, ElementRef, ViewChild } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AttemptService } from '../service/attempt.service';
import { AuthService } from '../service/auth.service';
import { SubmissionDetailDto, SubmissionAnswerDto } from '../models/models';

@Component({
  selector: 'app-submission-detail',
  standalone: true,
  imports: [RouterLink, CommonModule, DatePipe, DecimalPipe, FormsModule],
  templateUrl: './submission-detail.component.html',
  styleUrl: './submission-detail.component.css',
})
export class SubmissionDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private attemptSvc = inject(AttemptService);
  private auth = inject(AuthService);

  @ViewChild('certCanvas') certCanvas!: ElementRef<HTMLCanvasElement>;

  submission = signal<SubmissionDetailDto | null>(null);
  loading = signal(true);
  saving = signal(false);
  saveMsg = signal('');
  showCert = signal(false);

  // editable scores per question — plain object, updated via setScore
  editedScores = signal<Record<string, number>>({});

  editedTotal = computed(() =>
    Object.values(this.editedScores()).reduce((s, v) => s + (Number(v) || 0), 0)
  );

  editedPercentage = computed(() => {
    const sub = this.submission();
    if (!sub || sub.maxMark <= 0) return 0;
    return Math.round(this.editedTotal() / sub.maxMark * 100);
  });

  getScore(questionId: string): number {
    return this.editedScores()[questionId] ?? 0;
  }

  setScore(questionId: string, raw: string) {
    const value = Math.max(0, parseInt(raw, 10) || 0);
    this.editedScores.update(scores => ({ ...scores, [questionId]: value }));
  }

  isAnswerCorrect(ans: SubmissionAnswerDto): boolean {
    if (!ans.chosenOption || !ans.correctOption) return false;
    return ans.chosenOption.trim().toUpperCase() === ans.correctOption.trim().toUpperCase();
  }

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('attemptId')!;
    this.attemptSvc.getSubmissionDetail(id).subscribe({
      next: s => {
        this.submission.set(s);
        const initial: Record<string, number> = {};
        s.answers.forEach(a => { initial[a.questionId] = a.marksAwarded; });
        this.editedScores.set(initial);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  saveScores() {
    const s = this.submission();
    if (!s) return;
    this.saving.set(true);
    this.saveMsg.set('');
    const dto = {
      newTotalMark: this.editedTotal(),
      questionScores: Object.entries(this.editedScores()).map(([questionId, marksAwarded]) => ({
        questionId,
        marksAwarded: Number(marksAwarded) || 0
      }))
    };
    this.attemptSvc.updateScore(s.attemptAnswerId, dto).subscribe({
      next: () => {
        this.saving.set(false);
        this.saveMsg.set('Scores saved successfully!');
        this.submission.update(sub => sub ? {
          ...sub,
          totalMark: this.editedTotal(),
          percentage: this.editedPercentage()
        } : sub);
        setTimeout(() => this.saveMsg.set(''), 3000);
      },
      error: () => { this.saving.set(false); this.saveMsg.set('Failed to save scores.'); }
    });
  }

  generateCertificate() {
    this.showCert.set(true);
    setTimeout(() => this.drawCert(), 50);
  }

  drawCert() {
    const canvas = this.certCanvas?.nativeElement;
    if (!canvas || !this.submission()) return;
    const ctx = canvas.getContext('2d')!;
    const s = this.submission()!;
    const W = 900, H = 640;
    canvas.width = W; canvas.height = H;

    const pct = Math.round(s.maxMark > 0 ? s.totalMark / s.maxMark * 100 : 0);
    const grade = pct >= 80 ? 'Distinction' : pct >= 60 ? 'Merit' : pct >= 40 ? 'Pass' : 'Participation';
    const gradeColor = pct >= 80 ? '#2a7a4f' : pct >= 60 ? '#b87a00' : pct >= 40 ? '#1a5c8a' : '#666';
    const date = new Date(s.submittedAt).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' });

    ctx.fillStyle = '#fdfaf4'; ctx.fillRect(0, 0, W, H);
    ctx.strokeStyle = '#c8a84b'; ctx.lineWidth = 10; ctx.strokeRect(14, 14, W - 28, H - 28);
    ctx.strokeStyle = '#e8c96a'; ctx.lineWidth = 2; ctx.strokeRect(26, 26, W - 52, H - 52);
    [[40,40],[W-40,40],[40,H-40],[W-40,H-40]].forEach(([cx,cy]) => {
      ctx.fillStyle = '#c8a84b'; ctx.beginPath(); ctx.arc(cx, cy, 8, 0, Math.PI*2); ctx.fill();
    });

    const grad = ctx.createLinearGradient(0, 50, W, 120);
    grad.addColorStop(0, '#1a2a6c'); grad.addColorStop(0.5, '#2d4a9e'); grad.addColorStop(1, '#1a2a6c');
    ctx.fillStyle = grad; ctx.fillRect(40, 50, W - 80, 70);
    ctx.fillStyle = '#ffd700'; ctx.font = 'bold 28px Georgia, serif'; ctx.textAlign = 'center';
    ctx.fillText('⚡ QuizZap', W / 2, 95);

    ctx.fillStyle = '#2c1a00'; ctx.font = 'italic 18px Georgia, serif';
    ctx.fillText('Certificate of Completion', W / 2, 155);
    ctx.strokeStyle = '#c8a84b'; ctx.lineWidth = 1.5;
    ctx.beginPath(); ctx.moveTo(180, 168); ctx.lineTo(W - 180, 168); ctx.stroke();

    ctx.fillStyle = '#555'; ctx.font = '15px Georgia, serif';
    ctx.fillText('This is to certify that', W / 2, 200);
    ctx.fillStyle = '#1a2a6c'; ctx.font = 'bold 38px Georgia, serif';
    ctx.fillText(s.username, W / 2, 250);
    const nw = ctx.measureText(s.username).width;
    ctx.strokeStyle = '#c8a84b'; ctx.lineWidth = 1.5;
    ctx.beginPath(); ctx.moveTo(W/2 - nw/2, 258); ctx.lineTo(W/2 + nw/2, 258); ctx.stroke();

    ctx.fillStyle = '#555'; ctx.font = '15px Georgia, serif';
    ctx.fillText('has successfully completed', W / 2, 295);
    ctx.fillStyle = '#2c1a00'; ctx.font = 'bold 22px Georgia, serif';
    ctx.fillText(`"${s.quizName}"`, W / 2, 330);

    ctx.fillStyle = '#f5f0e8'; ctx.strokeStyle = '#c8a84b'; ctx.lineWidth = 1;
    const boxY = 360;
    ctx.beginPath(); (ctx as any).roundRect(W/2 - 220, boxY, 440, 80, 10); ctx.fill(); ctx.stroke();
    ctx.fillStyle = '#1a2a6c'; ctx.font = 'bold 28px Georgia, serif';
    ctx.fillText(`${s.totalMark} / ${s.maxMark}`, W/2 - 90, boxY + 48);
    ctx.fillStyle = '#555'; ctx.font = '13px Georgia, serif';
    ctx.fillText('Score', W/2 - 90, boxY + 66);
    ctx.strokeStyle = '#c8a84b'; ctx.lineWidth = 1;
    ctx.beginPath(); ctx.moveTo(W/2, boxY + 15); ctx.lineTo(W/2, boxY + 65); ctx.stroke();
    ctx.fillStyle = gradeColor; ctx.font = 'bold 24px Georgia, serif';
    ctx.fillText(grade, W/2 + 90, boxY + 48);
    ctx.fillStyle = '#555'; ctx.font = '13px Georgia, serif';
    ctx.fillText('Grade', W/2 + 90, boxY + 66);

    ctx.fillStyle = '#888'; ctx.font = '13px Georgia, serif';
    ctx.fillText(`Issued on: ${date}`, W / 2, 475);
    ctx.strokeStyle = '#c8a84b'; ctx.lineWidth = 1.5;
    ctx.beginPath(); ctx.moveTo(180, 490); ctx.lineTo(W - 180, 490); ctx.stroke();

    ctx.beginPath(); ctx.arc(W / 2, 555, 42, 0, Math.PI * 2);
    ctx.fillStyle = '#1a2a6c'; ctx.fill();
    ctx.strokeStyle = '#ffd700'; ctx.lineWidth = 3; ctx.stroke();
    ctx.fillStyle = '#ffd700'; ctx.font = 'bold 13px Georgia, serif';
    ctx.fillText('CERTIFIED', W / 2, 550);
    ctx.font = '11px Georgia, serif'; ctx.fillText('QuizZap', W / 2, 568);
  }

  downloadCert() {
    const canvas = this.certCanvas?.nativeElement;
    if (!canvas) return;
    const s = this.submission();
    const link = document.createElement('a');
    link.download = `${s?.username}-${s?.quizName}-certificate.png`.replace(/\s+/g, '-').toLowerCase();
    link.href = canvas.toDataURL('image/png');
    link.click();
  }
}
