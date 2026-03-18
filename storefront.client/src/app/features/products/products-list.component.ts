import { Component, signal } from '@angular/core';
import { AsyncPipe, CurrencyPipe, NgIf, NgFor } from '@angular/common';
import { RouterLink } from '@angular/router';

import { ProductApiService } from '../../core/services/product-api.service';
import { Product } from '../../core/models/product';
import { catchError, finalize, of, tap } from 'rxjs';

@Component({
  selector: 'app-products-list',
  standalone: true,
  imports: [NgIf, NgFor, AsyncPipe, CurrencyPipe, RouterLink],
  template: `
    <section class="products">
      <header class="products__header">
        <h1>Products</h1>
        <p class="products__subtitle">Browse the available items in the storefront.</p>

        <div class="products__actions">
          <a class="products__create-order" routerLink="/orders/create">
            Create order
          </a>
        </div>
      </header>

      <div class="products__state" *ngIf="loading()">
        <p>Loading products...</p>
      </div>

      <div class="products__state products__state--error" *ngIf="error()">
        <p>{{ error() }}</p>
      </div>

      <ng-container>
        <ng-container *ngIf="products$ | async as products">
          <div class="products__state" *ngIf="products.length === 0">
            <p>No products available.</p>
          </div>

          <ul class="products__list" *ngIf="products.length > 0">
            <li class="products__item" *ngFor="let product of products">
              <div class="products__item-main">
                <h2 class="products__item-name">{{ product.name }}</h2>
                <p class="products__item-price">{{ product.price | currency }}</p>
              </div>
              <p class="products__item-description" *ngIf="product.description">
                {{ product.description }}
              </p>
            </li>
          </ul>
        </ng-container>
      </ng-container>
    </section>
  `,
  styleUrl: './products-list.component.css'
})
export class ProductsListComponent {
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  protected readonly products$;

  constructor(private readonly productApiService: ProductApiService) {
    this.products$ = this.productApiService.getProducts().pipe(
      tap(() => {
        this.error.set(null);
      }),
      catchError((error: unknown) => {
        const message =
          error instanceof Error
            ? error.message
            : 'Could not load products. Please try again.';
        this.error.set(message);
        return of<Product[]>([]);
      }),
      finalize(() => {
        this.loading.set(false);
      })
    );
  }
}

