import { Component, signal } from '@angular/core';
import { AsyncPipe, CurrencyPipe, NgFor, NgIf } from '@angular/common';
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
  imports: [NgIf, NgFor, AsyncPipe, CurrencyPipe],
  template: `
    <section class="order">
      <header class="order__header">
        <h1>Create order</h1>
        <p class="order__subtitle">Pick products + quantities, then place the order.</p>
      </header>

      <form class="order__form" (submit)="onCreateOrder($event)">
        <div class="order__field">
          <label for="customerName">Customer name</label>
          <input
            id="customerName"
            type="text"
            autocomplete="name"
            [value]="customerName()"
            (input)="onCustomerNameInput($event)"
          />
        </div>

        <div class="order__products">
          <div class="order__state" *ngIf="productsLoading()">
            <p>Loading products...</p>
          </div>

          <div class="order__state order__state--error" *ngIf="productsError()">
            <p>{{ productsError() }}</p>
          </div>

          <ng-container *ngIf="products$ | async as products">
            <div class="order__empty" *ngIf="products.length === 0">
              No products available.
            </div>

            <div class="order__product-grid" *ngIf="products.length > 0">
              <div class="order__product" *ngFor="let product of products">
                <div class="order__product-main">
                  <div class="order__product-info">
                    <h2 class="order__product-name">{{ product.name }}</h2>
                    <p class="order__product-price">{{ product.price | currency }}</p>
                  </div>

                  <div class="order__product-qty">
                    <label>
                      Qty
                      <input
                        type="number"
                        min="0"
                        step="1"
                        [value]="quantities[product.id] ?? 0"
                        [disabled]="product.isActive === false"
                        (input)="onQuantityInput(product.id, $event)"
                      />
                    </label>
                  </div>
                </div>

                <p class="order__product-description" *ngIf="product.description">
                  {{ product.description }}
                </p>
              </div>
            </div>
          </ng-container>
        </div>

        <div class="order__actions">
          <button type="submit" [disabled]="creatingOrder()">
            {{ creatingOrder() ? 'Creating...' : 'Create order' }}
          </button>
        </div>

        <div class="order__state order__state--error" *ngIf="createOrderError()">
          <p>{{ createOrderError() }}</p>
        </div>
      </form>

      <section class="order__summary" *ngIf="createdOrder() as order">
        <header class="order__summary-header">
          <h2>Order {{ order.orderId }}</h2>
          <p class="order__summary-status">
            Status: <strong>{{ order.status }}</strong>
          </p>
          <p class="order__summary-total">Total: {{ order.total | currency }}</p>
        </header>

        <ul class="order__line-items">
          <li class="order__line-item" *ngFor="let line of order.lineItems">
            <span>{{ getProductName(line.productId) }}</span>
            <span>Qty: {{ line.quantity }}</span>
          </li>
        </ul>

        <div class="order__payment" *ngIf="order.status === 'PendingPayment'">
          <h3>Mock payment</h3>

          <div class="order__field">
            <label for="failureReason">Failure reason (used when result is failure)</label>
            <input
              id="failureReason"
              type="text"
              autocomplete="off"
              [value]="mockFailureReason()"
              (input)="onMockFailureReasonInput($event)"
            />
          </div>

          <div class="order__payment-actions">
            <button
              type="button"
              [disabled]="processingPayment()"
              (click)="submitMockPayment('success')"
            >
              {{ processingPayment() ? 'Processing...' : 'Pay success' }}
            </button>
            <button
              type="button"
              class="order__button--secondary"
              [disabled]="processingPayment()"
              (click)="submitMockPayment('failure')"
            >
              Pay failure
            </button>
          </div>

          <div class="order__state order__state--error" *ngIf="paymentError()">
            <p>{{ paymentError() }}</p>
          </div>
        </div>
      </section>
    </section>
  `,
  styleUrl: './create-order.component.css'
})
export class CreateOrderComponent {
  protected readonly customerName = signal('');
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

  protected onCustomerNameInput(event: Event): void {
    this.customerName.set((event.target as HTMLInputElement).value);
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

    const name = this.customerName().trim();
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

    if (!name) {
      this.createOrderError.set('Customer name is required.');
      return;
    }

    if (lineItems.length === 0) {
      this.createOrderError.set('At least one product quantity must be greater than 0.');
      return;
    }

    const request: CreateOrderRequest = {
      customerName: name,
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

