import { Routes } from '@angular/router';

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
];
