import { HttpErrorResponse } from '@angular/common/http';
import { Component, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../auth.service';

interface LoginProblem {
  detail?: string;
  errors?: Record<string, string[]>;
  title?: string;
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null;
}

type LoginControls = {
  email: FormControl<string>;
  password: FormControl<string>;
};

@Component({
  imports: [ReactiveFormsModule, RouterLink],
  selector: 'app-login',
  styleUrl: './login.scss',
  templateUrl: './login.html',
})
export class Login {
  protected readonly isSubmitting = signal(false);
  protected readonly formError = signal<string | null>(null);
  protected readonly passwordVisible = signal(false);
  protected readonly serverErrors = signal<Partial<Record<'email' | 'password', string>>>({});

  readonly loginForm = new FormGroup<LoginControls>({
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email, Validators.maxLength(256)],
    }),
    password: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
  });

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router,
  ) {}

  protected submit(): void {
    this.loginForm.markAllAsTouched();
    this.formError.set(null);
    this.loginForm.controls.email.setValue(this.loginForm.controls.email.value.trim());

    if (this.loginForm.invalid || this.isSubmitting()) {
      return;
    }

    this.serverErrors.set({});
    this.isSubmitting.set(true);

    const { email, password } = this.loginForm.getRawValue();
    this.authService
      .login({ email: email.trim(), password })
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: () => void this.router.navigateByUrl('/dashboard'),
        error: (error: HttpErrorResponse) => this.handleLoginError(error),
      });
  }

  protected clearServerError(field: 'email' | 'password'): void {
    this.serverErrors.update((errors) => ({ ...errors, [field]: undefined }));
    this.formError.set(null);
  }

  private handleLoginError(error: HttpErrorResponse): void {
    if (error.status === 401) {
      this.formError.set('Email or password is incorrect.');
      return;
    }

    const problem = this.readProblem(error.error);
    const emailError = this.firstError(problem?.errors, 'email', 'Email');
    const passwordError = this.firstError(problem?.errors, 'password', 'Password');
    this.serverErrors.set({
      ...(emailError ? { email: emailError } : {}),
      ...(passwordError ? { password: passwordError } : {}),
    });

    this.formError.set(
      problem?.detail ??
        problem?.title ??
        'We could not sign you in. Please check your information and try again.',
    );
  }

  private readProblem(value: unknown): LoginProblem | null {
    if (!isRecord(value)) {
      return null;
    }

    const problem: LoginProblem = {};
    if (typeof value['detail'] === 'string') {
      problem.detail = value['detail'];
    }
    if (typeof value['title'] === 'string') {
      problem.title = value['title'];
    }

    const errors = value['errors'];
    if (isRecord(errors)) {
      const fieldErrors: Record<string, string[]> = {};
      for (const [field, messages] of Object.entries(errors)) {
        if (Array.isArray(messages) && messages.every((message) => typeof message === 'string')) {
          fieldErrors[field] = messages;
        }
      }
      problem.errors = fieldErrors;
    }

    return problem;
  }

  private firstError(
    errors: Record<string, string[]> | undefined,
    ...keys: string[]
  ): string | undefined {
    for (const key of keys) {
      const message = errors?.[key]?.[0];
      if (message) {
        return message;
      }
    }

    return undefined;
  }
}
