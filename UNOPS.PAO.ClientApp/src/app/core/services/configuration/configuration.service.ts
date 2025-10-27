import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError } from 'rxjs/operators';
import { of, firstValueFrom, map } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ConfigurationService {
  private config: any;

  constructor(private http: HttpClient) {}

  loadConfig() {
    return firstValueFrom(
      this.http.get('/api/configuration').pipe(
        map((data) => {
          this.config = data;
        }),
        catchError((error) => {
          console.error('Failed to load config:', error);
          return of({});
        }),
      ),
    );
  }

  getConfig() {
    return this.config;
  }
}
