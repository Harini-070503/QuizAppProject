import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../service/auth.service';

export const authGuard: CanActivateFn = () => {
  const auth   = inject(AuthService);
  const router = inject(Router);
  if (auth.isLoggedIn()) return true;
  return router.createUrlTree(['/login']);
};

export const creatorGuard: CanActivateFn = () => {
  const auth   = inject(AuthService);
  const router = inject(Router);
  // Both Creator and Evaluator can access quiz/category management
  if (auth.isLoggedIn() && (auth.isCreator() || auth.isEvaluator())) return true;
  return router.createUrlTree(['/dashboard']);
};

export const guestGuard: CanActivateFn = () => {
  const auth   = inject(AuthService);
  const router = inject(Router);
  if (!auth.isLoggedIn()) return true;
  return router.createUrlTree(['/dashboard']);
};

export const evaluatorGuard: CanActivateFn = () => {
  const auth   = inject(AuthService);
  const router = inject(Router);
  if (auth.isLoggedIn() && auth.isEvaluator()) return true;
  return router.createUrlTree(['/dashboard']);
};

export const premiumTakerGuard: CanActivateFn = () => {
  const auth   = inject(AuthService);
  const router = inject(Router);
  if (auth.isLoggedIn() && auth.isPremiumTaker()) return true;
  return router.createUrlTree(['/dashboard']);
};
