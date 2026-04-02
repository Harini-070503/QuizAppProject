import { Injectable, signal, computed } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class CoinService {
  private readonly KEY = 'quiz_coins';
  private _coins = signal<number>(parseInt(localStorage.getItem(this.KEY) ?? '0', 10));

  readonly coins = computed(() => this._coins());

  /** Calculate coins earned from a quiz result */
  calcCoins(correctCount: number, percentage: number): number {
    let earned = correctCount * 10;
    if (percentage >= 80) earned += 50;       // distinction bonus
    else if (percentage >= 60) earned += 25;  // merit bonus
    else if (percentage >= 40) earned += 10;  // pass bonus
    return earned;
  }

  addCoins(amount: number): void {
    const next = this._coins() + amount;
    this._coins.set(next);
    localStorage.setItem(this.KEY, String(next));
  }

  reset(): void {
    this._coins.set(0);
    localStorage.removeItem(this.KEY);
  }
}
