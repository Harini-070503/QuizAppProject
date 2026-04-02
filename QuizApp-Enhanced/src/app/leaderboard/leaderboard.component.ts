import { Component, signal, OnInit, inject } from '@angular/core';
import { LeaderboardService } from '../service/leaderboard.service';
import { CategoryService } from '../service/category.service';
import { LeaderboardDto, CategoryDto } from '../models/models';
import { DecimalPipe, DatePipe } from '@angular/common';

@Component({
  templateUrl: './leaderboard.component.html',
  styleUrl: './leaderboard.component.css',
  selector: 'app-leaderboard',
  standalone: true,
  imports: [DecimalPipe, DatePipe],

})
export class LeaderboardComponent implements OnInit {
  private lbSvc = inject(LeaderboardService);
  private catSvc = inject(CategoryService);

  entries = signal<LeaderboardDto[]>([]);
  categories = signal<CategoryDto[]>([]);
  loading = signal(true);
  selectedCat = signal('');

  ngOnInit() {
    this.catSvc.getAll().subscribe({ next: c => this.categories.set(c), error: () => {} });
    this.loadLeaderboard();
  }

  filterCat(catId: string) {
    this.selectedCat.set(catId);
    this.loadLeaderboard(catId);
  }

  loadLeaderboard(catId?: string) {
    this.loading.set(true);
    this.lbSvc.getTop(catId || undefined, 20).subscribe({
      next: e => { this.entries.set(e); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }
}
