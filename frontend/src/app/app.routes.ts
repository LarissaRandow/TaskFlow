import { Routes } from '@angular/router';
import { authGuard } from './auth/auth.guard';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'login',
  },
  {
    path: 'login',
    loadComponent: () => import('./auth/login/login').then((module) => module.Login),
  },
  {
    path: 'register',
    loadComponent: () => import('./auth/register/register').then((module) => module.Register),
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./tasks/dashboard/dashboard').then((module) => module.Dashboard),
  },
  {
    path: '**',
    redirectTo: 'dashboard',
  },
];
