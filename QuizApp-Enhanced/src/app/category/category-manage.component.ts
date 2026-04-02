import { Component, signal, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CategoryService } from '../service/category.service';
import { CategoryDto } from '../models/models';

@Component({
  templateUrl: './category-manage.component.html',
  styleUrl: './category-manage.component.css',
  selector: 'app-category-manage',
  standalone: true,
  imports: [FormsModule, RouterLink],

})
export class CategoryManageComponent implements OnInit {
  private catSvc = inject(CategoryService);

  categories = signal<CategoryDto[]>([]);
  loading = signal(true);
  saving = signal(false);
  error = signal('');
  success = signal('');
  catName = '';
  editId = signal('');

  ngOnInit() { this.loadCats(); }

  loadCats() {
    this.loading.set(true);
    this.catSvc.getAll().subscribe({ next: c => { this.categories.set(c); this.loading.set(false); }, error: () => this.loading.set(false) });
  }

  onCreate() {
    if (!this.catName.trim()) { this.error.set('Category name is required.'); return; }
    this.saving.set(true); this.error.set(''); this.success.set('');
    this.catSvc.create({ categoryName: this.catName }).subscribe({
      next: () => { this.saving.set(false); this.catName = ''; this.success.set('Category created!'); this.loadCats(); },
      error: (e) => { this.saving.set(false); this.error.set(e?.error?.message || 'Failed to create.'); }
    });
  }

  startEdit(c: CategoryDto) { this.editId.set(c.categoryId); this.catName = c.categoryName; }
  cancelEdit() { this.editId.set(''); this.catName = ''; }

  onUpdate() {
    if (!this.catName.trim()) { this.error.set('Name required.'); return; }
    this.saving.set(true); this.error.set(''); this.success.set('');
    this.catSvc.update(this.editId(), { categoryName: this.catName }).subscribe({
      next: () => { this.saving.set(false); this.success.set('Category updated!'); this.cancelEdit(); this.loadCats(); },
      error: (e) => { this.saving.set(false); this.error.set(e?.error?.message || 'Failed to update.'); }
    });
  }

  onDelete(id: string) {
    if (!confirm('Delete this category?')) return;
    this.catSvc.delete(id).subscribe({
      next: () => { this.success.set('Deleted.'); this.loadCats(); },
      error: () => this.error.set('Failed to delete. It may have quizzes linked.')
    });
  }
}
