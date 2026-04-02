import { Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, Router } from '@angular/router';
import { CommonModule, DatePipe } from '@angular/common';
import { AuthService } from '../service/auth.service';
import { ThemeService } from '../service/theme.service';
import { CoinService } from '../service/coin.service';
import { NotificationService } from '../service/notification.service';

@Component({
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css',
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, CommonModule, DatePipe],
})
export class NavbarComponent {
  auth = inject(AuthService);
  themeService = inject(ThemeService);
  coinSvc = inject(CoinService);
  notifSvc = inject(NotificationService);
  private router = inject(Router);

  showNotifPanel = signal(false);
  get isEvaluator() { return this.auth.isEvaluator(); }

  ngOnInit() {
    if (this.auth.isLoggedIn()) this.notifSvc.load();
  }

  toggleNotifPanel() {
    this.showNotifPanel.update(v => !v);
    if (this.showNotifPanel()) this.notifSvc.load();
  }

  markAllRead() {
    this.notifSvc.markAllRead().subscribe({
      next: () => this.notifSvc.load()
    });
  }

  openNotif(n: any) {
    this.notifSvc.markRead(n.notificationId).subscribe({ next: () => this.notifSvc.load() });
    this.showNotifPanel.set(false);
    if (n.linkUrl) this.router.navigateByUrl(n.linkUrl);
  }
}
