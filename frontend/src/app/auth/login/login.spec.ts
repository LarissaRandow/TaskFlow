import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { Component } from '@angular/core';
import { Login } from './login';

@Component({ template: '' })
class DashboardStub {}

describe('Login', () => {
  let httpTesting: HttpTestingController;

  beforeEach(async () => {
    sessionStorage.clear();
    await TestBed.configureTestingModule({
      imports: [Login],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([{ path: 'dashboard', component: DashboardStub }]),
      ],
    }).compileComponents();

    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
    sessionStorage.clear();
  });

  it('requires an email and password and validates email format', () => {
    const fixture = TestBed.createComponent(Login);
    const form = fixture.componentInstance.loginForm;

    expect(form.invalid).toBe(true);

    form.controls.email.setValue('not-an-email');
    form.controls.password.setValue('password');
    expect(form.controls.email.hasError('email')).toBe(true);
    expect(form.invalid).toBe(true);

    form.controls.email.setValue('person@example.com');
    expect(form.valid).toBe(true);
  });

  it('sends credentials, stores the session, and navigates to the dashboard', async () => {
    const fixture = TestBed.createComponent(Login);
    const form = fixture.componentInstance.loginForm;
    form.setValue({ email: ' person@example.com ', password: 'StrongPass1!' });
    fixture.detectChanges();

    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));

    const request = httpTesting.expectOne('/api/auth/login');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      email: 'person@example.com',
      password: 'StrongPass1!',
    });

    request.flush({
      accessToken: 'signed-token',
      tokenType: 'Bearer',
      expiresAt: '2026-11-07T23:00:00Z',
      user: { id: 'user-id', email: 'person@example.com' },
    });
    fixture.detectChanges();

    expect(sessionStorage.getItem('taskflow.accessToken')).toBe('signed-token');
    expect(sessionStorage.getItem('taskflow.userEmail')).toBe('person@example.com');
    await fixture.whenStable();
    expect(TestBed.inject(Router).url).toBe('/dashboard');
  });

  it('shows a generic message for invalid credentials', () => {
    const fixture = TestBed.createComponent(Login);
    const form = fixture.componentInstance.loginForm;
    form.setValue({ email: 'person@example.com', password: 'wrong-password' });
    fixture.detectChanges();
    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
    fixture.detectChanges();

    httpTesting
      .expectOne('/api/auth/login')
      .flush(
        { detail: 'The account for this email was not found.' },
        { status: 401, statusText: 'Unauthorized' },
      );
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Email or password is incorrect.');
    expect(fixture.nativeElement.textContent).not.toContain('account for this email was not found');
  });

  it('shows server validation errors', () => {
    const fixture = TestBed.createComponent(Login);
    const form = fixture.componentInstance.loginForm;
    form.setValue({ email: 'person@example.com', password: 'password' });
    fixture.detectChanges();
    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
    fixture.detectChanges();

    httpTesting.expectOne('/api/auth/login').flush(
      {
        title: 'One or more validation errors occurred.',
        errors: { Password: ['Password is required.'] },
      },
      { status: 400, statusText: 'Bad Request' },
    );
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Password is required.');
  });
});
