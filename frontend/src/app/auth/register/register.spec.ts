import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Register } from './register';

describe('Register', () => {
  let httpTesting: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Register],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();

    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('requires matching passwords that meet the approved policy', () => {
    const fixture = TestBed.createComponent(Register);
    const form = fixture.componentInstance.registrationForm;

    form.setValue({
      email: 'person@example.com',
      password: 'StrongPass1!',
      confirmPassword: 'DifferentPass1!',
    });
    expect(form.invalid).toBe(true);

    form.controls.confirmPassword.setValue('StrongPass1!');
    expect(form.valid).toBe(true);

    form.controls.password.setValue('weak');
    expect(form.controls.password.hasError('minlength')).toBe(true);
    expect(form.invalid).toBe(true);

    form.controls.password.setValue('lowercase1!');
    expect(form.controls.password.hasError('pattern')).toBe(true);
  });

  it('sends only email and password and displays registration success', () => {
    const fixture = TestBed.createComponent(Register);
    const form = fixture.componentInstance.registrationForm;
    form.setValue({
      email: 'person@example.com',
      password: 'StrongPass1!',
      confirmPassword: 'StrongPass1!',
    });
    fixture.detectChanges();

    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));

    const request = httpTesting.expectOne('/api/auth/register');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      email: 'person@example.com',
      password: 'StrongPass1!',
    });

    request.flush(
      { id: 'user-id', email: 'person@example.com' },
      { status: 201, statusText: 'Created' },
    );
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Your account is ready');
    expect(fixture.nativeElement.textContent).toContain('person@example.com');
  });

  it('shows a clear message when the email is already registered', () => {
    const fixture = TestBed.createComponent(Register);
    const form = fixture.componentInstance.registrationForm;
    form.setValue({
      email: 'person@example.com',
      password: 'StrongPass1!',
      confirmPassword: 'StrongPass1!',
    });
    fixture.detectChanges();
    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
    fixture.detectChanges();

    httpTesting.expectOne('/api/auth/register').flush({}, { status: 409, statusText: 'Conflict' });
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain(
      'An account with this email already exists.',
    );
  });

  it('shows server validation errors', () => {
    const fixture = TestBed.createComponent(Register);
    const form = fixture.componentInstance.registrationForm;
    form.setValue({
      email: 'person@example.com',
      password: 'StrongPass1!',
      confirmPassword: 'StrongPass1!',
    });
    fixture.detectChanges();
    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
    fixture.detectChanges();

    httpTesting.expectOne('/api/auth/register').flush(
      {
        title: 'One or more validation errors occurred.',
        errors: { Email: ['This email address is not allowed.'] },
      },
      { status: 400, statusText: 'Bad Request' },
    );
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('This email address is not allowed.');
  });
});
