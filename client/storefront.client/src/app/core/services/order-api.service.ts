import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';

import {
  CreateOrderRequest,
  MockPaymentRequest,
  OrderDto
} from '../models/order';
import { getApiErrorMessage } from '../utils/api-error';

@Injectable({
  providedIn: 'root'
})
export class OrderApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/orders';

  createOrder(request: CreateOrderRequest): Observable<OrderDto> {
    return this.http.post<OrderDto>(this.baseUrl, request).pipe(
      catchError((error: unknown) => {
        const message = getApiErrorMessage(error, 'Could not create order. Please try again.');
        return throwError(() => new Error(message));
      })
    );
  }

  mockPayment(orderId: string, request: MockPaymentRequest): Observable<OrderDto> {
    return this.http
      .post<OrderDto>(`${this.baseUrl}/${orderId}/payments/mock`, request)
      .pipe(
        catchError((error: unknown) => {
          const message = getApiErrorMessage(error, 'Could not process mock payment. Please try again.');
          return throwError(() => new Error(message));
        })
      );
  }

  getOrderById(orderId: string): Observable<OrderDto> {
    return this.http.get<OrderDto>(`${this.baseUrl}/${orderId}`).pipe(
      catchError((error: unknown) => {
        if (error instanceof HttpErrorResponse && error.status === 404) {
          const fromBody = getApiErrorMessage(error, '');
          return throwError(() => new Error(fromBody || 'Order not found.'));
        }
        const message = getApiErrorMessage(error, 'Could not retrieve order. Please try again.');
        return throwError(() => new Error(message));
      })
    );
  }
}
