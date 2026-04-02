import { Routes } from '@angular/router';
import { authGuard, creatorGuard, evaluatorGuard, guestGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', loadComponent: () => import('./start-page/start-page.component').then(m => m.StartPageComponent) },
  { path: 'login', canActivate: [guestGuard], loadComponent: () => import('./login/login.component').then(m => m.LoginComponent) },
  { path: 'register', canActivate: [guestGuard], loadComponent: () => import('./register/register.component').then(m => m.RegisterComponent) },
  { path: 'dashboard', canActivate: [authGuard], loadComponent: () => import('./dashboard/dashboard.component').then(m => m.DashboardComponent) },
  { path: 'quiz/:id', canActivate: [authGuard], loadComponent: () => import('./quiz/quiz.component').then(m => m.QuizComponent) },
  { path: 'result/:attemptId', canActivate: [authGuard], loadComponent: () => import('./attempt/attempt-result.component').then(m => m.AttemptResultComponent) },
  { path: 'leaderboard', canActivate: [authGuard], loadComponent: () => import('./leaderboard/leaderboard.component').then(m => m.LeaderboardComponent) },
  { path: 'profile', canActivate: [authGuard], loadComponent: () => import('./profile/profile.component').then(m => m.ProfileComponent) },
  { path: 'user-details', canActivate: [authGuard], loadComponent: () => import('./user-details/user-details.component').then(m => m.UserDetailsComponent) },
  { path: 'admin/quiz/create', canActivate: [authGuard, creatorGuard], loadComponent: () => import('./question/quiz-create.component').then(m => m.QuizCreateComponent) },
  { path: 'admin/quiz/edit/:id', canActivate: [authGuard, creatorGuard], loadComponent: () => import('./question/quiz-create.component').then(m => m.QuizCreateComponent) },
  { path: 'admin/categories', canActivate: [authGuard, creatorGuard], loadComponent: () => import('./category/category-manage.component').then(m => m.CategoryManageComponent) },
  { path: 'admin/quiz/:quizId/questions', canActivate: [authGuard, creatorGuard], loadComponent: () => import('./question-manage/question-manage.component').then(m => m.QuestionManageComponent) },
  // ── Evaluator routes ──
  { path: 'evaluator/dashboard', canActivate: [authGuard, evaluatorGuard], loadComponent: () => import('./evaluator/evaluator-dashboard.component').then(m => m.EvaluatorDashboardComponent) },
  { path: 'evaluator/groups', canActivate: [authGuard, evaluatorGuard], loadComponent: () => import('./evaluator/group-manage.component').then(m => m.GroupManageComponent) },
  { path: 'evaluator/quiz/:quizId/submissions', canActivate: [authGuard, evaluatorGuard], loadComponent: () => import('./evaluator/submissions-list.component').then(m => m.SubmissionsListComponent) },
  { path: 'evaluator/submission/:attemptId', canActivate: [authGuard, evaluatorGuard], loadComponent: () => import('./evaluator/submission-detail.component').then(m => m.SubmissionDetailComponent) },
  { path: '**', redirectTo: '' }
];
