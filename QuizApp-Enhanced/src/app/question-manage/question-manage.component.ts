import { Component, signal, computed, OnInit, inject, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { QuizService } from '../service/quiz.service';
import { QuestionService } from '../service/question.service';
import { CategoryService } from '../service/category.service';
import { ThemeService } from '../service/theme.service';
import { QuestionDto, QuestionCreateDto, OptionCreateDto, CategoryDto } from '../models/models';
import * as XLSX from 'xlsx';

@Component({
  selector: 'app-question-manage',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './question-manage.component.html',
  styleUrl: './question-manage.component.css'
})
export class QuestionManageComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private quizSvc = inject(QuizService);
  private questionSvc = inject(QuestionService);
  private catSvc = inject(CategoryService);
  public themeService = inject(ThemeService);

  quizId = signal('');
  quizName = signal('');
  questions = signal<QuestionDto[]>([]);

  // Quiz settings
  quizTimeLimit = signal<number | null>(null);
  quizPassMark = signal<number>(0);
  quizCategoryId = signal('');
  quizDifficulty = signal('Easy');
  quizDescription = signal('');
  categories = signal<CategoryDto[]>([]);
  showSettingsPanel = signal(false);
  settingsTimeLimit: number | null = null;
  settingsPassMark = 0;
  settingsCategoryId = '';
  settingsDifficulty = 'Easy';
  settingsSaving = signal(false);
  settingsSuccess = signal('');
  settingsError = signal('');

  // Inline new-category in settings
  showNewCatPanel = signal(false);
  newCatName = '';
  newCatSaving = signal(false);
  newCatError = signal('');

  // Pagination
  currentPage = signal(1);
  questionsPerPage = 5;

  // Form state (signals for read, plain props for ngModel two-way binding)
  isAddMode = signal(false);
  isEditMode = signal(false);
  editingQuestionId = signal<string | null>(null);

  // Form data — plain properties so [(ngModel)] works
  questionText = '';
  questionType: 'custom' | 'truefalse' | 'yesno' = 'custom';
  optionCount = 4;
  optionA = '';
  optionB = '';
  optionC = '';
  optionD = '';
  correctOptions: string[] = [];
  marks = 1;

  readonly optionKeys = ['A','B','C','D'];

  @ViewChild('bulkFileInput') bulkFileInput!: ElementRef<HTMLInputElement>;

  // Bulk upload state
  showBulkPanel = signal(false);
  bulkPreview = signal<QuestionCreateDto[]>([]);
  bulkError = signal('');
  bulkUploading = signal(false);
  bulkFileName = signal('');

  // Loading / feedback
  isLoading = signal(true);
  isSaving = signal(false);
  error = signal('');
  successMsg = signal('');

  totalPages = computed(() =>
    Math.ceil(this.questions().length / this.questionsPerPage) || 1
  );

  paginatedQuestions = computed(() => {
    const start = (this.currentPage() - 1) * this.questionsPerPage;
    return this.questions().slice(start, start + this.questionsPerPage);
  });

  pageNumbers = computed(() =>
    Array.from({ length: this.totalPages() }, (_, i) => i + 1)
  );

  availableCorrectOptions = computed(() => {
    return this.optionKeys.slice(0, this.optionCount);
  });

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('quizId');
    if (!id) {
      this.router.navigate(['/dashboard']);
      return;
    }
    this.quizId.set(id);
    this.catSvc.getAll().subscribe({ next: c => this.categories.set(c), error: () => {} });
    this.loadQuestions();
  }

  loadQuestions() {
    this.isLoading.set(true);
    this.quizSvc.get(this.quizId()).subscribe({
      next: (quiz) => {
        this.questions.set(quiz.questions || []);
        this.quizName.set(quiz.quizName || '');
        this.quizTimeLimit.set(quiz.timeLimit ?? null);
        this.quizPassMark.set(quiz.passMark);
        this.quizCategoryId.set(quiz.category?.categoryId || '');
        this.quizDifficulty.set(quiz.difficultyLevel || 'Easy');
        this.quizDescription.set(quiz.description || '');
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading questions:', err);
        this.error.set('Failed to load questions. Is the backend running?');
        this.isLoading.set(false);
      }
    });
  }

  // ——— Quiz Settings ———
  openSettings() {
    this.settingsTimeLimit = this.quizTimeLimit();
    this.settingsPassMark = this.quizPassMark();
    this.settingsCategoryId = this.quizCategoryId();
    this.settingsDifficulty = this.quizDifficulty();
    this.settingsSuccess.set('');
    this.settingsError.set('');
    this.showSettingsPanel.set(true);
  }

  closeSettings() {
    this.showSettingsPanel.set(false);
    this.showNewCatPanel.set(false);
  }

  saveSettings() {
    this.settingsSaving.set(true);
    this.settingsError.set('');
    this.settingsSuccess.set('');
    this.quizSvc.update(this.quizId(), {
      quizName: this.quizName(),
      description: this.quizDescription(),
      categoryId: this.settingsCategoryId,
      passMark: this.settingsPassMark,
      totalQuestion: this.questions().length,
      difficultyLevel: this.settingsDifficulty,
      timeLimit: this.settingsTimeLimit || undefined
    }).subscribe({
      next: () => {
        this.quizTimeLimit.set(this.settingsTimeLimit);
        this.quizPassMark.set(this.settingsPassMark);
        this.quizCategoryId.set(this.settingsCategoryId);
        this.quizDifficulty.set(this.settingsDifficulty);
        this.settingsSaving.set(false);
        this.settingsSuccess.set('Quiz settings updated!');
        setTimeout(() => { this.settingsSuccess.set(''); this.closeSettings(); }, 1500);
      },
      error: (e) => {
        this.settingsSaving.set(false);
        this.settingsError.set(e?.error?.message || 'Failed to save settings.');
      }
    });
  }

  openNewCatPanel() {
    this.newCatName = '';
    this.newCatError.set('');
    this.showNewCatPanel.set(true);
  }

  cancelNewCat() {
    this.showNewCatPanel.set(false);
    this.newCatName = '';
  }

  saveNewCategory() {
    if (!this.newCatName.trim()) { this.newCatError.set('Name required.'); return; }
    this.newCatSaving.set(true);
    this.catSvc.create({ categoryName: this.newCatName.trim() }).subscribe({
      next: (created) => {
        this.categories.update(list => [...list, created]);
        this.settingsCategoryId = created.categoryId;
        this.newCatSaving.set(false);
        this.showNewCatPanel.set(false);
        this.newCatName = '';
      },
      error: (e) => {
        this.newCatSaving.set(false);
        this.newCatError.set(e?.error?.message || 'Failed to create category.');
      }
    });
  }

  // ——— Pagination ———
  goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages()) this.currentPage.set(page);
  }
  previousPage() { if (this.currentPage() > 1) this.currentPage.update(p => p - 1); }
  nextPage() { if (this.currentPage() < this.totalPages()) this.currentPage.update(p => p + 1); }

  // ——— Option count ———
  setOptionCount(count: number) {
    this.optionCount = count;
    if (count < 3) this.optionC = '';
    if (count < 4) this.optionD = '';
    const valid = this.optionKeys.slice(0, count);
    this.correctOptions = this.correctOptions.filter(c => valid.includes(c));
  }

  setQuestionType(type: 'custom' | 'truefalse' | 'yesno') {
    this.questionType = type;
    this.correctOptions = [];
    if (type === 'truefalse') {
      this.optionCount = 2; this.optionA = 'True'; this.optionB = 'False';
      this.optionC = ''; this.optionD = '';
    } else if (type === 'yesno') {
      this.optionCount = 2; this.optionA = 'Yes'; this.optionB = 'No';
      this.optionC = ''; this.optionD = '';
    }
  }

  toggleCorrect(key: string) {
    const idx = this.correctOptions.indexOf(key);
    if (idx === -1) this.correctOptions = [...this.correctOptions, key];
    else this.correctOptions = this.correctOptions.filter(c => c !== key);
  }

  // ——— Form show/hide ———
  showAddForm() {
    this.resetForm();
    this.isAddMode.set(true);
    this.isEditMode.set(false);
    this.successMsg.set('');
    setTimeout(() => document.querySelector('.question-form-container')?.scrollIntoView({ behavior: 'smooth' }), 100);
  }

  hideForm() {
    this.isAddMode.set(false);
    this.isEditMode.set(false);
    this.editingQuestionId.set(null);
    this.resetForm();
  }

  resetForm() {
    this.questionText = '';
    this.questionType = 'custom';
    this.optionA = ''; this.optionB = ''; this.optionC = ''; this.optionD = '';
    this.correctOptions = [];
    this.optionCount = 4;
    this.marks = 1;
    this.error.set('');
  }

  validateForm(): boolean {
    if (!this.questionText.trim()) { this.error.set('Question text is required'); return false; }
    if (!this.optionA.trim()) { this.error.set('Option A is required'); return false; }
    if (!this.optionB.trim()) { this.error.set('Option B is required'); return false; }
    if (this.optionCount >= 3 && !this.optionC.trim()) { this.error.set('Option C is required'); return false; }
    if (this.optionCount >= 4 && !this.optionD.trim()) { this.error.set('Option D is required'); return false; }
    if (this.correctOptions.length === 0) { this.error.set('Select at least one correct answer'); return false; }
    return true;
  }

  private buildDto(): QuestionCreateDto {
    const options: OptionCreateDto = {
      optionA: this.optionA, optionB: this.optionB,
      optionC: this.optionCount >= 3 ? this.optionC : undefined,
      optionD: this.optionCount >= 4 ? this.optionD : undefined,
      correctOption: this.correctOptions.join(',')
    };
    return { questionText: this.questionText, marks: this.marks || 1, options };
  }

  addQuestion() {
    if (!this.validateForm()) return;
    this.isSaving.set(true);
    this.error.set('');

    this.questionSvc.create(this.quizId(), this.buildDto()).subscribe({
      next: (newQ) => {
        this.questions.update(list => [...list, newQ]);
        // Go to last page to see the new question
        const lastPage = Math.ceil((this.questions().length) / this.questionsPerPage);
        this.currentPage.set(lastPage);
        this.resetForm();
        this.isAddMode.set(false);
        this.successMsg.set('Question added successfully!');
        this.isSaving.set(false);
        setTimeout(() => this.successMsg.set(''), 3000);
      },
      error: (err) => {
        console.error(err);
        this.error.set('Failed to add question. Please try again.');
        this.isSaving.set(false);
      }
    });
  }

  editQuestion(question: QuestionDto) {
    this.isEditMode.set(true);
    this.isAddMode.set(true);
    this.editingQuestionId.set(question.questionId);
    this.questionText = question.questionText;
    this.marks = question.marks || 1;
    this.questionType = 'custom';
    if (question.options) {
      this.optionA = question.options.optionA;
      this.optionB = question.options.optionB;
      this.optionC = question.options.optionC || '';
      this.optionD = question.options.optionD || '';
      this.correctOptions = (question.options.correctOption || 'A').split(',').map(s => s.trim());
      this.optionCount = question.options.optionD ? 4 : question.options.optionC ? 3 : 2;
    }
    this.error.set('');
    this.successMsg.set('');
    setTimeout(() => document.querySelector('.question-form-container')?.scrollIntoView({ behavior: 'smooth' }), 100);
  }

  updateQuestion() {
    if (!this.validateForm()) return;
    const id = this.editingQuestionId();
    if (!id) return;
    this.isSaving.set(true);
    this.error.set('');

    this.questionSvc.update(id, this.buildDto()).subscribe({
      next: (updated) => {
        this.questions.update(list => list.map(q => q.questionId === id ? updated : q));
        this.hideForm();
        this.successMsg.set('Question updated successfully!');
        this.isSaving.set(false);
        setTimeout(() => this.successMsg.set(''), 3000);
      },
      error: (err) => {
        console.error(err);
        this.error.set('Failed to update question. Please try again.');
        this.isSaving.set(false);
      }
    });
  }

  deleteQuestion(questionId: string) {
    if (!confirm('Delete this question?')) return;
    this.questionSvc.delete(questionId).subscribe({
      next: () => {
        this.questions.update(list => list.filter(q => q.questionId !== questionId));
        // Adjust page if needed
        if (this.currentPage() > this.totalPages()) this.currentPage.set(this.totalPages());
        this.successMsg.set('Question deleted.');
        setTimeout(() => this.successMsg.set(''), 3000);
      },
      error: (err) => {
        console.error(err);
        this.error.set('Failed to delete question.');
      }
    });
  }

  getQuestionNumber(index: number): number {
    return (this.currentPage() - 1) * this.questionsPerPage + index + 1;
  }

  // ——— Bulk Upload ———
  openBulkPanel() {
    this.showBulkPanel.set(true);
    this.bulkPreview.set([]);
    this.bulkError.set('');
    this.bulkFileName.set('');
  }

  closeBulkPanel() {
    this.showBulkPanel.set(false);
    this.bulkPreview.set([]);
    this.bulkError.set('');
    this.bulkFileName.set('');
    if (this.bulkFileInput?.nativeElement) this.bulkFileInput.nativeElement.value = '';
  }

  downloadTemplate() {
    const headers = ['QuestionText', 'OptionA', 'OptionB', 'OptionC', 'OptionD', 'CorrectOption', 'Marks'];
    const sample = [
      ['What is 2+2?', '3', '4', '5', '6', 'B', '1'],
      ['Is the sky blue?', 'True', 'False', '', '', 'A', '2'],
    ];
    const ws = XLSX.utils.aoa_to_sheet([headers, ...sample]);
    ws['!cols'] = headers.map(() => ({ wch: 20 }));
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Questions');
    XLSX.writeFile(wb, 'questions-template.xlsx');
  }

  onFileSelected(event: Event) {
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

        const qIdx = col('questiontext');
        const aIdx = col('optiona');
        const bIdx = col('optionb');
        const cIdx = col('optionc');
        const dIdx = col('optiond');
        const corrIdx = col('correctoption');
        const marksIdx = col('marks');

        if (qIdx === -1 || aIdx === -1 || bIdx === -1 || corrIdx === -1) {
          this.bulkError.set('Missing required columns: QuestionText, OptionA, OptionB, CorrectOption');
          return;
        }

        const parsed: QuestionCreateDto[] = [];
        const errors: string[] = [];

        rows.slice(1).forEach((row, i) => {
          const rowNum = i + 2;
          const qText = String(row[qIdx] ?? '').trim();
          const optA  = String(row[aIdx] ?? '').trim();
          const optB  = String(row[bIdx] ?? '').trim();
          const optC  = cIdx !== -1 ? String(row[cIdx] ?? '').trim() : '';
          const optD  = dIdx !== -1 ? String(row[dIdx] ?? '').trim() : '';
          const corr  = String(row[corrIdx] ?? '').trim().toUpperCase();
          const marks = marksIdx !== -1 ? parseInt(String(row[marksIdx] ?? '1'), 10) || 1 : 1;

          if (!qText) { errors.push(`Row ${rowNum}: QuestionText is empty`); return; }
          if (!optA || !optB) { errors.push(`Row ${rowNum}: OptionA and OptionB are required`); return; }
          if (!corr) { errors.push(`Row ${rowNum}: CorrectOption is empty`); return; }

          parsed.push({
            questionText: qText,
            marks,
            options: {
              optionA: optA,
              optionB: optB,
              optionC: optC || undefined,
              optionD: optD || undefined,
              correctOption: corr
            }
          });
        });

        if (errors.length > 0) {
          this.bulkError.set(errors.slice(0, 5).join('\n') + (errors.length > 5 ? `\n...and ${errors.length - 5} more` : ''));
          return;
        }

        if (parsed.length === 0) { this.bulkError.set('No valid questions found in the file.'); return; }
        this.bulkPreview.set(parsed);
      } catch (err) {
        this.bulkError.set('Failed to read file. Make sure it is a valid .xlsx or .csv file.');
      }
    };
    reader.readAsArrayBuffer(file);
  }

  uploadBulkQuestions() {
    const questions = this.bulkPreview();
    if (questions.length === 0) return;
    this.bulkUploading.set(true);
    this.bulkError.set('');

    let completed = 0;
    let failed = 0;

    const uploadNext = (index: number) => {
      if (index >= questions.length) {
        this.bulkUploading.set(false);
        this.closeBulkPanel();
        const msg = failed > 0
          ? `${completed} questions uploaded, ${failed} failed.`
          : `${completed} questions uploaded successfully!`;
        this.successMsg.set(msg);
        this.loadQuestions();
        setTimeout(() => this.successMsg.set(''), 4000);
        return;
      }
      this.questionSvc.create(this.quizId(), questions[index]).subscribe({
        next: () => { completed++; uploadNext(index + 1); },
        error: () => { failed++; uploadNext(index + 1); }
      });
    };

    uploadNext(0);
  }

  backToDashboard() {
    this.router.navigate(['/dashboard']);
  }
}
