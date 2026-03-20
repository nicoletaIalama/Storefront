import { Component, signal } from '@angular/core';
import { CurrencyPipe, NgFor, NgIf } from '@angular/common';

import { OrderApiService } from '../../core/services/order-api.service';
import { OrderDto } from '../../core/models/order';

@Component({
  selector: 'app-retrieve-order',
  standalone: true,
  imports: [NgIf, NgFor, CurrencyPipe],
  templateUrl: './retrieve-order.component.html',
  styleUrl: './retrieve-order.component.css'
})
export class RetrieveOrderComponent {
  protected readonly orderId = signal('');
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly retrievedOrder = signal<OrderDto | null>(null);

  constructor(private readonly orderApiService: OrderApiService) {}

  protected onOrderIdInput(event: Event): void {
    this.orderId.set((event.target as HTMLInputElement).value);
  }

  protected onRetrieveOrder(event: Event): void {
    event.preventDefault();
    this.error.set(null);
    this.retrievedOrder.set(null);

    const id = this.orderId().trim();
    if (!id) {
      this.error.set('Order ID is required.');
      return;
    }

    this.loading.set(true);
    this.orderApiService.getOrderById(id).subscribe({
      next: (order) => {
        this.retrievedOrder.set(order);
      },
      error: (error: unknown) => {
        const message =
          error instanceof Error ? error.message : 'Could not retrieve order. Please try again.';
        this.error.set(message);
      },
      complete: () => {
        this.loading.set(false);
      }
    });
  }
}
