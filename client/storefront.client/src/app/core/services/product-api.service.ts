import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, catchError, map, throwError } from 'rxjs';

import { CreateProductRequest, Product } from '../models/product';
import { getApiErrorMessage } from '../utils/api-error';

@Injectable({
  providedIn: 'root'
})
export class ProductApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/products';

  getProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(this.baseUrl).pipe(
      map((products) => products ?? []),
      catchError((error: unknown) => {
        const message = getApiErrorMessage(error, 'Could not load products. Please try again.');
        return throwError(() => new Error(message));
      })
    );
  }

  createProduct(request: CreateProductRequest): Observable<Product> {
    return this.http.post<Product>(this.baseUrl, request).pipe(
      catchError((error: unknown) => {
        if (error instanceof HttpErrorResponse && error.status === 403) {
          return throwError(() => new Error('Admin role is required.'));
        }
        const message = getApiErrorMessage(error, 'Could not create product. Please try again.');
        return throwError(() => new Error(message));
      })
    );
  }

  deleteProduct(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`).pipe(
      catchError((error: unknown) => {
        if (error instanceof HttpErrorResponse && error.status === 403) {
          return throwError(() => new Error('Admin role is required.'));
        }
        if (error instanceof HttpErrorResponse && error.status === 404) {
          const fromBody = getApiErrorMessage(error, '');
          return throwError(() => new Error(fromBody || 'Product not found.'));
        }
        const message = getApiErrorMessage(error, 'Could not delete product. Please try again.');
        return throwError(() => new Error(message));
      })
    );
  }
}
