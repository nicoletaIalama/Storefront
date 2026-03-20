import { Component, signal } from '@angular/core';
import { AsyncPipe, CurrencyPipe, NgFor, NgIf } from '@angular/common';
import { RouterLink } from '@angular/router';
import { catchError, finalize, of, tap } from 'rxjs';

import { ProductApiService } from '../../core/services/product-api.service';
import { OrderApiService } from '../../core/services/order-api.service';
import { Product } from '../../core/models/product';
import {
  CreateOrderLineItemRequest,
  CreateOrderRequest,
  MockPaymentResult,
  OrderDto
} from '../../core/models/order';

@Component({
  selector: 'app-create-order',
  standalone: true,
  imports: [NgIf, NgFor, AsyncPipe, CurrencyPipe, RouterLink],
  templateUrl: './create-order.component.html',
  styleUrl: './create-order.component.css'
})
export class CreateOrderComponent {
  protected readonly quantities: Partial<Record<string, number>> = {};

  protected readonly productsLoading = signal(true);
  protected readonly productsError = signal<string | null>(null);
  protected productsById: Record<string, string> = {};

  protected readonly createdOrder = signal<OrderDto | null>(null);
  protected readonly creatingOrder = signal(false);
  protected readonly createOrderError = signal<string | null>(null);

  protected readonly mockFailureReason = signal('');
  protected readonly processingPayment = signal(false);
  protected readonly paymentError = signal<string | null>(null);

  protected readonly products$;

  constructor(
    private readonly productApiService: ProductApiService,
    private readonly orderApiService: OrderApiService
  ) {
    this.products$ = this.productApiService.getProducts().pipe(
      tap((products) => {
        this.productsError.set(null);
        this.productsById = Object.fromEntries(
          products.map((p) => [p.id, p.name])
        );
      }),
      catchError((error: unknown) => {
        const message =
          error instanceof Error
            ? error.message
            : 'Could not load products. Please try again.';

        this.productsError.set(message);
        return of<Product[]>([]);
      }),
      finalize(() => {
        this.productsLoading.set(false);
      })
    );
  }

  protected getProductName(productId: string): string {
    return this.productsById[productId] ?? productId;
  }

  protected onMockFailureReasonInput(event: Event): void {
    this.mockFailureReason.set((event.target as HTMLInputElement).value);
  }

  protected onQuantityInput(productId: string, event: Event): void {
    const raw = (event.target as HTMLInputElement).value;
    const asNumber = raw === '' ? 0 : Number(raw);
    const normalized = Number.isFinite(asNumber) ? Math.max(0, Math.floor(asNumber)) : 0;
    this.quantities[productId] = normalized;
  }

  protected onCreateOrder(event: Event): void {
    event.preventDefault();

    this.createOrderError.set(null);
    this.createdOrder.set(null);

    const lineItems = Object.entries(this.quantities)
      .filter(
        (entry): entry is [string, number] => {
          const qty = entry[1];
          return typeof qty === 'number' && qty > 0;
        }
      )
      .map(
        ([productId, quantity]): CreateOrderLineItemRequest => ({
          productId,
          quantity
        })
      );

    if (lineItems.length === 0) {
      this.createOrderError.set('At least one product quantity must be greater than 0.');
      return;
    }

    const request: CreateOrderRequest = {
      lineItems
    };

    this.creatingOrder.set(true);
    this.orderApiService.createOrder(request).subscribe({
      next: (order) => {
        this.createdOrder.set(order);
      },
      error: (error: unknown) => {
        const message =
          error instanceof Error ? error.message : 'Could not create order. Please try again.';
        this.createOrderError.set(message);
      },
      complete: () => {
        this.creatingOrder.set(false);
      }
    });
  }

  protected submitMockPayment(result: MockPaymentResult): void {
    const order = this.createdOrder();
    if (!order) {
      return;
    }

    this.paymentError.set(null);
    this.processingPayment.set(true);

    const request = {
      result,
      failureReason: result === 'failure' ? this.mockFailureReason() || 'Payment failed.' : null
    };

    this.orderApiService.mockPayment(order.orderId, request).subscribe({
      next: (updated) => {
        this.createdOrder.set(updated);
      },
      error: (error: unknown) => {
        const message =
          error instanceof Error
            ? error.message
            : 'Could not process mock payment. Please try again.';
        this.paymentError.set(message);
      },
      complete: () => {
        this.processingPayment.set(false);
      }
    });
  }
}

