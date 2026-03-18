import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';

import {
  CreateOrderRequest,
  MockPaymentRequest,
  OrderDto
} from '../models/order';

@Injectable({
  providedIn: 'root'
})
export class OrderApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/orders';

  createOrder(request: CreateOrderRequest): Observable<OrderDto> {
    return this.http.post<OrderDto>(this.baseUrl, request).pipe(
      catchError((error: unknown) => {
        const message =
          error instanceof HttpErrorResponse && error.error?.message
            ? String(error.error.message)
            : error instanceof Error
              ? error.message
              : 'Could not create order. Please try again.';

        return throwError(() => new Error(message));
      })
    );
  }

  mockPayment(orderId: string, request: MockPaymentRequest): Observable<OrderDto> {
    return this.http
      .post<OrderDto>(`${this.baseUrl}/${orderId}/payments/mock`, request)
      .pipe(
        catchError((error: unknown) => {
          const message =
            error instanceof HttpErrorResponse && error.error?.message
              ? String(error.error.message)
              : error instanceof Error
                ? error.message
                : 'Could not process mock payment. Please try again.';

          return throwError(() => new Error(message));
        })
      );
  }
}

