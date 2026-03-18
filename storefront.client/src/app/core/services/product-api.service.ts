import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, map, throwError } from 'rxjs';

import { Product } from '../models/product';

@Injectable({
  providedIn: 'root'
})
export class ProductApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/products';

  getProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(this.baseUrl).pipe(
      map((products) => products ?? []),
      catchError((error) => {
        console.error('Failed to load products', error);
        return throwError(
          () => new Error('Could not load products. Please try again.')
        );
      })
    );
  }
}

