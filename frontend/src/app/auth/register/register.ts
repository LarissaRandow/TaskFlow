import { HttpErrorResponse } from '@angular/common/http';
import { Component, signal } from '@angular/core';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../auth.service';

interface RegistrationProblem {
  detail?: string;
  errors?: Record<string, string[]>;
  title?: string;
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null;
}

type RegistrationControls = {
  email: FormControl<string>;
  password: FormControl<string>;
  confirmPassword: FormControl<string>;
};

function passwordConfirmationValidator(control: AbstractControl): ValidationErrors | null {
  const password = control.get('password')?.value;
  const confirmation = control.get('confirmPassword')?.value;

  return password && confirmation && password !== confirmation ? { passwordMismatch: true } : null;
}

const passwordPolicy = /(?=.*\p{Lu})(?=.*\p{Ll})(?=.*\p{Nd})(?=.*[^\p{L}\p{N}])/u;

@Component({
  imports: [ReactiveFormsModule, RouterLink],
  selector: 'app-register',
  styleUrl: './register.scss',
  templateUrl: './register.html',
})
export class Register {
  protected readonly isSubmitting = signal(false);
  protected readonly formError = signal<string | null>(null);
  protected readonly registeredEmail = signal<string | null>(null);
  protected readonly passwordVisible = signal(false);
  protected readonly confirmPasswordVisible = signal(false);
  protected readonly serverErrors = signal<Partial<Record<'email' | 'password', string>>>({});

  readonly registrationForm = new FormGroup<RegistrationControls>(
    {
      email: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required, Validators.email, Validators.maxLength(256)],
      }),
      password: new FormControl('', {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.minLength(8),
          Validators.pattern(passwordPolicy),
        ],
      }),
      confirmPassword: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required],
      }),
    },
    { validators: passwordConfirmationValidator },
  );

  constructor(private readonly authService: AuthService) {}

  protected submit(): void {
    this.registrationForm.markAllAsTouched();
    this.formError.set(null);

    if (this.registrationForm.invalid || this.isSubmitting()) {
      return;
    }

    this.serverErrors.set({});
    this.isSubmitting.set(true);

    const { email, password } = this.registrationForm.getRawValue();
    this.authService
      .register({ email: email.trim(), password })
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (response) => this.registeredEmail.set(response.email),
        error: (error: HttpErrorResponse) => this.handleRegistrationError(error),
      });
  }

  protected clearServerError(field: 'email' | 'password'): void {
    this.serverErrors.update((errors) => ({ ...errors, [field]: undefined }));
    this.formError.set(null);
  }

  private handleRegistrationError(error: HttpErrorResponse): void {
    if (error.status === 409) {
      this.formError.set('An account with this email already exists. Try signing in instead.');
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
        'We could not create your account. Please check your information and try again.',
    );
  }

  private readProblem(value: unknown): RegistrationProblem | null {
    if (!isRecord(value)) {
      return null;
    }

    const problem: RegistrationProblem = {};
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
