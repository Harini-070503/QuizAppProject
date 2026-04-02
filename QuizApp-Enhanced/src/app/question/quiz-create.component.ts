import { Component, signal, computed, OnInit, inject, ElementRef, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule, DatePipe } from '@angular/common';
import { QuizService } from '../service/quiz.service';
import { CategoryService } from '../service/category.service';
import { CategoryDto } from '../models/models';
import * as XLSX from 'xlsx';

export type QuestionType = 'custom' | 'truefalse' | 'yesno';

export interface QuestionForm {
  questionText: string;
  type: QuestionType;
  optionCount: number;
  optionA: string;
  optionB: string;
  optionC: string;
  optionD: string;
  optionE: string;
  optionF: string;
  correctOptions: string[];
  marks: number;
}

function blankQuestion(): QuestionForm {
  return {
    questionText: '', type: 'custom', optionCount: 4,
    optionA: '', optionB: '', optionC: '', optionD: '', optionE: '', optionF: '',
    correctOptions: [], marks: 1
  };
}

@Component({
  selector: 'app-quiz-create',
  standalone: true,
  imports: [FormsModule, RouterLink, CommonModule, DatePipe],
  templateUrl: './quiz-create.component.html',
  styleUrl: './quiz-create.component.css',
})
export class QuizCreateComponent implements OnInit {
  private quizSvc = inject(QuizService);
  private catSvc = inject(CategoryService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  @ViewChild('bulkFileInput') bulkFileInput!: ElementRef<HTMLInputElement>;

  categories = signal<CategoryDto[]>([]);
  saving = signal(false);
  error = signal('');
  success = signal('');
  isEdit = signal(false);
  editQuizId = signal('');

  quizName = '';
  description = '';
  categoryId = '';
  difficulty = 'Easy';
  passMark = 0;
  timeLimit: number | null = null;
  deadline = '';
  questions = signal<QuestionForm[]>([]);
  totalMarks = computed(() => this.questions().reduce((sum, q) => sum + (q.marks || 1), 0));

  showNewCatPanel = signal(false);
  newCatName = '';
  newCatSaving = signal(false);
  newCatError = signal('');

  showBulkPanel = signal(false);
  bulkPreview = signal<QuestionForm[]>([]);
  bulkFileName = signal('');
  bulkError = signal('');

  readonly optionKeys = ['A', 'B', 'C', 'D', 'E', 'F'];

  ngOnInit() {
    this.catSvc.getAll().subscribe({ next: c => this.categories.set(c), error: () => {} });
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit.set(true);
      this.editQuizId.set(id);
      this.quizSvc.get(id).subscribe({
        next: q => {
          this.quizName = q.quizName;
          this.description = q.description || '';
          this.categoryId = q.category?.categoryId || '';
          this.difficulty = q.difficultyLevel || 'Easy';
          this.passMark = q.passMark;
          this.timeLimit = q.timeLimit || null;
          this.deadline = q.deadline ? new Date(q.deadline).toISOString().slice(0, 16) : '';
          this.questions.set((q.questions || []).map(qs => {
            const opts = qs.options;
            const correct = (opts?.correctOption || 'A').split(',').map((s: string) => s.trim());
            const count = opts?.optionD ? 4 : opts?.optionC ? 3 : 2;
            return {
              questionText: qs.questionText,
              type: 'custom' as QuestionType,
              optionCount: count,
              optionA: opts?.optionA || '',
              optionB: opts?.optionB || '',
              optionC: opts?.optionC || '',
              optionD: opts?.optionD || '',
              optionE: '',
              optionF: '',
              correctOptions: correct,
              marks: (qs as any).marks ?? 1
            };
          }));
        },
        error: () => {}
      });
    }
  }

  addQuestion() {
    this.questions.update(qs => [...qs, blankQuestion()]);
  }

  removeQuestion(i: number) {
    this.questions.update(qs => qs.filter((_, idx) => idx !== i));
  }

  setQuestionType(q: QuestionForm, type: QuestionType) {
    q.type = type;
    q.correctOptions = [];
    if (type === 'truefalse') {
      q.optionCount = 2;
      q.optionA = 'True'; q.optionB = 'False';
      q.optionC = ''; q.optionD = ''; q.optionE = ''; q.optionF = '';
    } else if (type === 'yesno') {
      q.optionCount = 2;
      q.optionA = 'Yes'; q.optionB = 'No';
      q.optionC = ''; q.optionD = ''; q.optionE = ''; q.optionF = '';
    }
    this.questions.update(qs => [...qs]);
  }

  setOptionCount(q: QuestionForm, count: number) {
    q.optionCount = count;
    if (count < 3) q.optionC = '';
    if (count < 4) q.optionD = '';
    if (count < 5) q.optionE = '';
    if (count < 6) q.optionF = '';
    const valid = this.optionKeys.slice(0, count);
    q.correctOptions = q.correctOptions.filter(c => valid.includes(c));
    this.questions.update(qs => [...qs]);
  }

  toggleCorrect(q: QuestionForm, key: string) {
    const idx = q.correctOptions.indexOf(key);
    if (idx === -1) {
      q.correctOptions = [...q.correctOptions, key];
    } else {
      q.correctOptions = q.correctOptions.filter(c => c !== key);
    }
    this.questions.update(qs => [...qs]);
  }

  getOptionValue(q: QuestionForm, key: string): string {
    switch (key) {
      case 'A': return q.optionA;
      case 'B': return q.optionB;
      case 'C': return q.optionC;
      case 'D': return q.optionD;
      case 'E': return q.optionE;
      case 'F': return q.optionF;
      default: return '';
    }
  }

  setOptionValue(q: QuestionForm, key: string, val: string) {
    switch (key) {
      case 'A': q.optionA = val; break;
      case 'B': q.optionB = val; break;
      case 'C': q.optionC = val; break;
      case 'D': q.optionD = val; break;
      case 'E': q.optionE = val; break;
      case 'F': q.optionF = val; break;
    }
    this.questions.update(qs => [...qs]);
  }

  openNewCatPanel() {
    this.newCatName = '';
    this.newCatError.set('');
    this.showNewCatPanel.set(true);
  }

  cancelNewCat() {
    this.showNewCatPanel.set(false);
    this.newCatName = '';
    this.newCatError.set('');
  }

  saveNewCategory() {
    if (!this.newCatName.trim()) { this.newCatError.set('Category name is required.'); return; }
    this.newCatSaving.set(true);
    this.newCatError.set('');
    this.catSvc.create({ categoryName: this.newCatName.trim() }).subscribe({
      next: (created) => {
        this.categories.update(list => [...list, created]);
        this.categoryId = created.categoryId;
        this.newCatSaving.set(false);
        this.showNewCatPanel.set(false);
        this.newCatName = '';
      },
      error: (e: any) => {
        this.newCatSaving.set(false);
        this.newCatError.set(e?.error?.message || 'Failed to create category.');
      }
    });
  }

  openBulkPanel() {
    this.showBulkPanel.set(true);
    this.bulkError.set('');
    this.bulkFileName.set('');
    this.bulkPreview.set([]);
  }

  closeBulkPanel() {
    this.showBulkPanel.set(false);
    this.bulkError.set('');
    this.bulkFileName.set('');
    this.bulkPreview.set([]);
  }

  downloadTemplate() {
    const headers = ['QuestionText', 'OptionA', 'OptionB', 'OptionC', 'OptionD', 'CorrectOption', 'Marks'];
    const sample = [
      ['What is 2+2?', '3', '4', '5', '6', 'B', '1'],
      ['Is the sky blue?', 'True', 'False', '', '', 'A', '2'],
    ];
    const ws = XLSX.utils.aoa_to_sheet([headers, ...sample]);
    ws['!cols'] = headers.map(() => ({ wch: 22 }));
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Questions');
    XLSX.writeFile(wb, 'questions-template.xlsx');
  }

  onBulkFileSelected(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    this.bulkFileName.set(file.name);
    this.bulkError.set('');
    this.bulkPreview.set([]);

    const reader = new FileReader();
    reader.onload = (e) => {
      try {
        const data = new Uint8Array(e.target!.result as ArrayBuffer);
        const wb = XLSX.read(data, { type: 'array' });
        const ws = wb.Sheets[wb.SheetNames[0]];
        const rows: any[][] = XLSX.utils.sheet_to_json(ws, { header: 1, defval: '' });

        if (rows.length < 2) { this.bulkError.set('File is empty or has no data rows.'); return; }

        const header = (rows[0] as string[]).map(h => String(h).trim().toLowerCase());
        const col = (name: string) => header.indexOf(name.toLowerCase());

        const qIdx = col('questiontext'), aIdx = col('optiona'), bIdx = col('optionb');
        const cIdx = col('optionc'), dIdx = col('optiond');
        const corrIdx = col('correctoption'), marksIdx = col('marks');

        if (qIdx === -1 || aIdx === -1 || bIdx === -1 || corrIdx === -1) {
          this.bulkError.set('Missing columns: QuestionText, OptionA, OptionB, CorrectOption are required.');
          return;
        }

        const parsed: QuestionForm[] = [];
        const errors: string[] = [];

        rows.slice(1).forEach((row, i) => {
          const qText = String(row[qIdx] ?? '').trim();
          const optA  = String(row[aIdx] ?? '').trim();
          const optB  = String(row[bIdx] ?? '').trim();
          const optC  = cIdx !== -1 ? String(row[cIdx] ?? '').trim() : '';
          const optD  = dIdx !== -1 ? String(row[dIdx] ?? '').trim() : '';
          const corr  = String(row[corrIdx] ?? '').trim().toUpperCase();
          const marks = marksIdx !== -1 ? parseInt(String(row[marksIdx] ?? '1'), 10) || 1 : 1;

          if (!qText || !optA || !optB || !corr) {
            errors.push('Row ' + (i + 2) + ': missing required fields'); return;
          }

          const count = optD ? 4 : optC ? 3 : 2;
          parsed.push({
            questionText: qText, type: 'custom', optionCount: count,
            optionA: optA, optionB: optB, optionC: optC, optionD: optD,
            optionE: '', optionF: '',
            correctOptions: corr.split(',').map((s: string) => s.trim()),
            marks
          });
        });

        if (errors.length) { this.bulkError.set(errors.slice(0, 3).join('\n')); return; }
        if (!parsed.length) { this.bulkError.set('No valid questions found.'); return; }

        this.questions.update(qs => [...qs, ...parsed]);
        this.bulkPreview.set(parsed);
      } catch {
        this.bulkError.set('Failed to read file. Ensure it is a valid .xlsx or .csv.');
      }
    };
    reader.readAsArrayBuffer(file);
  }

  onSubmit() {
    this.error.set('');
    this.success.set('');
    if (!this.quizName.trim()) { this.error.set('Quiz name is required.'); return; }
    if (!this.categoryId) { this.error.set('Please select a category.'); return; }

    for (let i = 0; i < this.questions().length; i++) {
      const q = this.questions()[i];
      if (!q.questionText.trim()) { this.error.set('Q' + (i + 1) + ': Question text is required.'); return; }
      if (!q.optionA.trim() || !q.optionB.trim()) { this.error.set('Q' + (i + 1) + ': Options A and B are required.'); return; }
      if (q.correctOptions.length === 0) { this.error.set('Q' + (i + 1) + ': Select at least one correct answer.'); return; }
    }

    this.saving.set(true);

    const mappedQuestions = this.questions().map(q => ({
      questionText: q.questionText,
      marks: q.marks ?? 1,
      options: {
        optionA: q.optionA,
        optionB: q.optionB,
        optionC: q.optionCount >= 3 ? q.optionC : undefined,
        optionD: q.optionCount >= 4 ? q.optionD : undefined,
        correctOption: q.correctOptions.join(',')
      }
    }));

    const dto = {
      quizName: this.quizName,
      description: this.description,
      categoryId: this.categoryId,
      passMark: this.passMark,
      totalQuestion: this.questions().length,
      difficultyLevel: this.difficulty,
      timeLimit: this.timeLimit || undefined,
      deadline: this.deadline || undefined,
      questions: mappedQuestions
    };

    const obs = this.isEdit()
      ? this.quizSvc.update(this.editQuizId(), dto)
      : this.quizSvc.create(dto);

    obs.subscribe({
      next: () => {
        this.saving.set(false);
        this.success.set('Quiz saved!');
        setTimeout(() => this.router.navigate(['/dashboard']), 1200);
      },
      error: (e: any) => {
        this.saving.set(false);
        this.error.set(e?.error?.message || 'Failed to save quiz.');
      }
    });
  }
}
