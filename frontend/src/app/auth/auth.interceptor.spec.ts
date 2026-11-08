import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { authInterceptor } from './auth.interceptor';

describe('authInterceptor', () => {
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
    sessionStorage.clear();
  });

  it('adds the bearer token to task API requests', () => {
    sessionStorage.setItem('taskflow.accessToken', 'signed-token');
    const http = TestBed.inject(HttpClient);
    http.get('/api/tasks').subscribe();

    const request = httpTesting.expectOne('/api/tasks');
    expect(request.request.headers.get('Authorization')).toBe('Bearer signed-token');
    request.flush([]);
  });

  it('does not send the token to public authentication endpoints', () => {
    sessionStorage.setItem('taskflow.accessToken', 'signed-token');
    const http = TestBed.inject(HttpClient);
    http.post('/api/auth/login', {}).subscribe();

    const request = httpTesting.expectOne('/api/auth/login');
    expect(request.request.headers.has('Authorization')).toBe(false);
    request.flush({});
  });
});
