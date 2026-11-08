import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export type TaskStatus = 'Todo' | 'InProgress' | 'Completed' | 'Discarded';

export interface Task {
  id: string;
  title: string;
  description: string | null;
  status: TaskStatus;
  dueDate: string | null;
  userId: string;
}

export interface CreateTaskRequest {
  title: string;
  description: string;
  dueDate: string | null;
}

export interface UpdateTaskRequest extends CreateTaskRequest {
  status: TaskStatus;
}

@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly http = inject(HttpClient);

  list(): Observable<Task[]> {
    return this.http.get<Task[]>('/api/tasks');
  }

  create(request: CreateTaskRequest): Observable<Task> {
    return this.http.post<Task>('/api/tasks', request);
  }

  update(id: string, request: UpdateTaskRequest): Observable<Task> {
    return this.http.put<Task>(`/api/tasks/${encodeURIComponent(id)}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`/api/tasks/${encodeURIComponent(id)}`);
  }
}
