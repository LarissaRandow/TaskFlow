import { HttpErrorResponse } from '@angular/common/http';
import { Component, HostListener, OnInit, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../auth/auth.service';
import {
  CreateTaskRequest,
  Task,
  TaskService,
  TaskStatus,
  UpdateTaskRequest,
} from '../task.service';
import { BrandHeader } from '../../shared/brand-header/brand-header';
import { SiteFooter } from '../../shared/site-footer/site-footer';

type DialogMode = 'create' | 'edit' | 'delete' | null;
type TaskFormControls = {
  title: FormControl<string>;
  description: FormControl<string>;
  dueDate: FormControl<string>;
  status: FormControl<TaskStatus>;
};

interface TaskProblem {
  detail?: string;
  title?: string;
}

@Component({
  imports: [BrandHeader, ReactiveFormsModule, RouterLink, SiteFooter],
  selector: 'app-dashboard',
  styleUrls: ['./dashboard.scss', './dashboard-dialog.scss'],
  templateUrl: './dashboard.html',
})
export class Dashboard implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly taskService = inject(TaskService);

  protected readonly statuses: ReadonlyArray<{ value: TaskStatus; label: string }> = [
    { value: 'Todo', label: 'To Do' },
    { value: 'InProgress', label: 'In Progress' },
    { value: 'Completed', label: 'Completed' },
    { value: 'Discarded', label: 'Discarded' },
  ];
  protected readonly tasks = signal<Task[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly isSaving = signal(false);
  protected readonly dialogMode = signal<DialogMode>(null);
  protected readonly selectedTask = signal<Task | null>(null);
  protected readonly openMenuId = signal<string | null>(null);
  protected readonly accountMenuOpen = signal(false);
  protected readonly boardError = signal<string | null>(null);
  protected readonly notice = signal<string | null>(null);
  protected readonly accountEmail = this.authService.getUserEmail() ?? '';
  protected readonly avatarInitials = this.initialsFor(this.accountEmail);
  protected readonly todayLabel = new Intl.DateTimeFormat('en-US', {
    weekday: 'long',
    month: 'long',
    day: 'numeric',
    year: 'numeric',
  }).format(new Date());

  readonly taskForm = new FormGroup<TaskFormControls>({
    title: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern(/\S/), Validators.maxLength(120)],
    }),
    description: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(2000)],
    }),
    dueDate: new FormControl('', { nonNullable: true }),
    status: new FormControl<TaskStatus>('Todo', { nonNullable: true }),
  });

  ngOnInit(): void {
    this.loadTasks();
  }

  @HostListener('document:keydown.escape')
  protected handleEscape(): void {
    if (this.dialogMode()) {
      this.closeDialog();
    } else {
      this.openMenuId.set(null);
      this.accountMenuOpen.set(false);
    }
  }

  protected tasksFor(status: TaskStatus): Task[] {
    return this.tasks().filter((task) => task.status === status);
  }

  protected openCreateDialog(): void {
    this.resetForm();
    this.dialogMode.set('create');
  }

  protected openEditDialog(task: Task): void {
    this.selectedTask.set(task);
    this.taskForm.reset({
      title: task.title,
      description: task.description ?? '',
      dueDate: this.toDateInput(task.dueDate),
      status: task.status,
    });
    this.boardError.set(null);
    this.notice.set(null);
    this.openMenuId.set(null);
    this.dialogMode.set('edit');
  }

  protected openDeleteDialog(task: Task): void {
    this.selectedTask.set(task);
    this.boardError.set(null);
    this.notice.set(null);
    this.openMenuId.set(null);
    this.dialogMode.set('delete');
  }

  protected closeDialog(): void {
    if (this.isSaving()) {
      return;
    }
    this.dialogMode.set(null);
    this.selectedTask.set(null);
    this.boardError.set(null);
  }

  protected saveTask(): void {
    this.taskForm.markAllAsTouched();
    if (this.taskForm.invalid || this.isSaving()) {
      return;
    }

    const form = this.taskForm.getRawValue();
    const request: CreateTaskRequest = {
      title: form.title.trim(),
      description: form.description.trim(),
      dueDate: this.toIsoDate(form.dueDate),
    };
    this.boardError.set(null);
    this.isSaving.set(true);

    if (this.dialogMode() === 'create') {
      this.taskService
        .create(request)
        .pipe(finalize(() => this.isSaving.set(false)))
        .subscribe({
          next: (task) => {
            this.tasks.update((tasks) => [...tasks, task]);
            this.finishDialog('Task created.');
          },
          error: (error: HttpErrorResponse) => this.showRequestError(error),
        });
      return;
    }

    const task = this.selectedTask();
    if (!task) {
      this.isSaving.set(false);
      this.boardError.set('Select a task to update.');
      return;
    }

    const update: UpdateTaskRequest = { ...request, status: form.status };
    this.taskService
      .update(task.id, update)
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: (updatedTask) => {
          this.tasks.update((tasks) =>
            tasks.map((current) => (current.id === updatedTask.id ? updatedTask : current)),
          );
          this.finishDialog('Task updated.');
        },
        error: (error: HttpErrorResponse) => this.showRequestError(error),
      });
  }

  protected confirmDelete(): void {
    const task = this.selectedTask();
    if (!task || this.isSaving()) {
      return;
    }

    this.boardError.set(null);
    this.isSaving.set(true);
    this.taskService
      .delete(task.id)
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: () => {
          this.tasks.update((tasks) => tasks.filter((current) => current.id !== task.id));
          this.finishDialog('Task deleted.');
        },
        error: (error: HttpErrorResponse) => this.showRequestError(error),
      });
  }

  protected toggleTaskMenu(taskId: string, event: MouseEvent): void {
    event.stopPropagation();
    this.openMenuId.update((current) => (current === taskId ? null : taskId));
    this.accountMenuOpen.set(false);
  }

  protected toggleAccountMenu(): void {
    this.accountMenuOpen.update((open) => !open);
    this.openMenuId.set(null);
  }

  protected closeMenus(): void {
    this.openMenuId.set(null);
    this.accountMenuOpen.set(false);
  }

  protected retryLoad(): void {
    this.loadTasks();
  }

  protected logout(): void {
    this.authService.logout();
    void this.router.navigateByUrl('/login');
  }

  protected formatDueDate(value: string | null): string {
    if (!value) {
      return 'No due date';
    }
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
      return 'Date unavailable';
    }
    return `Due ${new Intl.DateTimeFormat('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    }).format(date)}`;
  }

  private loadTasks(): void {
    this.isLoading.set(true);
    this.boardError.set(null);
    this.taskService
      .list()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (tasks) => this.tasks.set(tasks),
        error: (error: HttpErrorResponse) => this.showRequestError(error),
      });
  }

  private finishDialog(message: string): void {
    this.dialogMode.set(null);
    this.selectedTask.set(null);
    this.resetForm();
    this.notice.set(message);
  }

  private resetForm(): void {
    this.taskForm.reset({
      title: '',
      description: '',
      dueDate: '',
      status: 'Todo',
    });
    this.boardError.set(null);
    this.notice.set(null);
  }

  private showRequestError(error: HttpErrorResponse): void {
    const problem = this.readProblem(error.error);
    this.boardError.set(
      problem?.detail ?? problem?.title ?? 'We could not complete that request. Please try again.',
    );
  }

  private readProblem(value: unknown): TaskProblem | null {
    if (typeof value !== 'object' || value === null) {
      return null;
    }
    const problem: TaskProblem = {};
    if ('detail' in value && typeof value.detail === 'string') {
      problem.detail = value.detail;
    }
    if ('title' in value && typeof value.title === 'string') {
      problem.title = value.title;
    }
    return problem;
  }

  private toDateInput(value: string | null): string {
    if (!value) {
      return '';
    }
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
      return '';
    }
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  private toIsoDate(value: string): string | null {
    return value ? new Date(`${value}T12:00:00`).toISOString() : null;
  }

  private initialsFor(email: string): string {
    const parts =
      email
        .split('@')[0]
        ?.split(/[._-]+/)
        .filter(Boolean) ?? [];
    if (!parts.length) {
      return '—';
    }
    return (
      parts.length > 1 ? `${parts[0][0]}${parts[parts.length - 1][0]}` : parts[0].slice(0, 2)
    ).toUpperCase();
  }
}
