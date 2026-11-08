import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Dashboard } from './dashboard';
import { Task } from '../task.service';

const task: Task = {
  id: 'task-1',
  title: 'Review customer feedback',
  description: 'Turn interviews into useful insights.',
  status: 'Todo',
  dueDate: '2026-11-10T12:00:00Z',
  userId: 'user-1',
};

describe('Dashboard', () => {
  let httpTesting: HttpTestingController;

  beforeEach(async () => {
    sessionStorage.setItem('taskflow.accessToken', 'signed-token');
    sessionStorage.setItem('taskflow.userEmail', 'alex.morgan@example.com');
    await TestBed.configureTestingModule({
      imports: [Dashboard],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
    sessionStorage.clear();
  });

  function createDashboard(tasks: Task[] = [task]) {
    const fixture = TestBed.createComponent(Dashboard);
    fixture.detectChanges();
    httpTesting.expectOne('/api/tasks').flush(tasks);
    fixture.detectChanges();
    return fixture;
  }

  it('loads the four status columns and tasks for the signed-in user', () => {
    const fixture = createDashboard();
    const element = fixture.nativeElement as HTMLElement;

    expect(
      Array.from(element.querySelectorAll('.column-heading h2')).map((heading) =>
        heading.textContent?.trim(),
      ),
    ).toEqual(['To Do', 'In Progress', 'Completed', 'Discarded']);
    expect(element.querySelector('.task-card h3')?.textContent?.trim()).toBe(task.title);
    expect(element.querySelector('.account-email')?.textContent?.trim()).toBe(
      'alex.morgan@example.com',
    );
    expect(element.querySelector('.avatar')?.textContent?.trim()).toBe('AM');
  });

  it('rejects whitespace-only titles', () => {
    const fixture = createDashboard([]);
    fixture.componentInstance.taskForm.controls.title.setValue('   ');

    expect(fixture.componentInstance.taskForm.controls.title.invalid).toBe(true);
  });

  it('creates tasks in Todo without sending a status field', () => {
    const fixture = createDashboard([]);
    const element = fixture.nativeElement as HTMLElement;
    element.querySelector<HTMLButtonElement>('.create-button')!.click();
    fixture.detectChanges();

    const title = element.querySelector<HTMLInputElement>('#task-title')!;
    title.value = 'Prepare demo';
    title.dispatchEvent(new Event('input', { bubbles: true }));
    fixture.detectChanges();
    element.querySelector<HTMLButtonElement>('.task-form button[type="submit"]')!.click();

    const request = httpTesting.expectOne('/api/tasks');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      title: 'Prepare demo',
      description: '',
      dueDate: null,
    });
    expect(request.request.body).not.toHaveProperty('status');
    request.flush({ ...task, title: 'Prepare demo', description: '', status: 'Todo' });
    fixture.detectChanges();

    expect(element.querySelector('.column-Todo .task-card h3')?.textContent?.trim()).toBe(
      'Prepare demo',
    );
    expect(element.querySelector('.notice')?.textContent).toContain('Task created.');
  });

  it('updates status using one of the approved API values', () => {
    const fixture = createDashboard();
    const element = fixture.nativeElement as HTMLElement;
    element.querySelector<HTMLButtonElement>('.more-button')!.click();
    fixture.detectChanges();
    element.querySelector<HTMLButtonElement>('.task-menu button')!.click();
    fixture.detectChanges();

    const status = element.querySelector<HTMLSelectElement>('#task-status')!;
    status.value = 'InProgress';
    status.dispatchEvent(new Event('change', { bubbles: true }));
    element.querySelector<HTMLButtonElement>('.task-form button[type="submit"]')!.click();

    const request = httpTesting.expectOne('/api/tasks/task-1');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body.status).toBe('InProgress');
    request.flush({ ...task, status: 'InProgress' });
    fixture.detectChanges();

    expect(element.querySelector('.column-InProgress .task-card h3')?.textContent?.trim()).toBe(
      task.title,
    );
  });

  it('deletes a task only after confirmation', () => {
    const fixture = createDashboard();
    const element = fixture.nativeElement as HTMLElement;
    element.querySelector<HTMLButtonElement>('.more-button')!.click();
    fixture.detectChanges();
    element.querySelectorAll<HTMLButtonElement>('.task-menu button')[1].click();
    fixture.detectChanges();
    element.querySelector<HTMLButtonElement>('.danger-button')!.click();

    const request = httpTesting.expectOne('/api/tasks/task-1');
    expect(request.request.method).toBe('DELETE');
    request.flush(null, { status: 204, statusText: 'No Content' });
    fixture.detectChanges();

    expect(element.querySelector('.task-card')).toBeNull();
    expect(element.querySelector('.notice')?.textContent).toContain('Task deleted.');
  });
});
