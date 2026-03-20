import { Component, signal } from '@angular/core';
import { CurrencyPipe, NgIf, NgFor } from '@angular/common';
import { RouterLink } from '@angular/router';

import { AuthService } from '../../core/services/auth.service';
import { ProductApiService } from '../../core/services/product-api.service';
import { CreateProductRequest, Product } from '../../core/models/product';

@Component({
  selector: 'app-products-list',
  standalone: true,
  imports: [NgIf, NgFor, CurrencyPipe, RouterLink],
  templateUrl: './products-list.component.html',
  styleUrl: './products-list.component.css'
})
export class ProductsListComponent {
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly products = signal<Product[]>([]);
  protected readonly savingProduct = signal(false);
  protected readonly deletingProductId = signal<string | null>(null);

  protected readonly newProductName = signal('');
  protected readonly newProductPrice = signal('');
  protected readonly newProductDescription = signal('');

  constructor(
    private readonly productApiService: ProductApiService,
    private readonly authService: AuthService
  ) {
    this.loadProducts();
  }

  protected isAuthenticated(): boolean {
    return this.authService.isAuthenticated();
  }

  protected isAdmin(): boolean {
    return this.authService.isAdmin();
  }

  protected roleLabel(): string {
    return this.authService.getRole() ?? 'User';
  }

  protected logout(): void {
    this.authService.logout();
  }

  protected onNewProductNameInput(event: Event): void {
    this.newProductName.set((event.target as HTMLInputElement).value);
  }

  protected onNewProductPriceInput(event: Event): void {
    this.newProductPrice.set((event.target as HTMLInputElement).value);
  }

  protected onNewProductDescriptionInput(event: Event): void {
    this.newProductDescription.set((event.target as HTMLInputElement).value);
  }

  protected createProduct(event: Event): void {
    event.preventDefault();

    const request: CreateProductRequest = {
      name: this.newProductName().trim(),
      price: Number(this.newProductPrice()),
      description: this.newProductDescription().trim() || null,
      isActive: true
    };

    if (!request.name) {
      this.error.set('Product name is required.');
      return;
    }

    if (!Number.isFinite(request.price) || request.price < 0) {
      this.error.set('Price must be a non-negative number.');
      return;
    }

    this.error.set(null);
    this.savingProduct.set(true);
    this.productApiService.createProduct(request).subscribe({
      next: () => {
        this.newProductName.set('');
        this.newProductPrice.set('');
        this.newProductDescription.set('');
        this.loadProducts();
      },
      error: (error: unknown) => {
        const message =
          error instanceof Error ? error.message : 'Could not create product. Please try again.';
        this.error.set(message);
      },
      complete: () => {
        this.savingProduct.set(false);
      }
    });
  }

  protected deleteProduct(productId: string): void {
    this.error.set(null);
    this.deletingProductId.set(productId);
    this.productApiService.deleteProduct(productId).subscribe({
      next: () => {
        this.products.update((items) => items.filter((p) => p.id !== productId));
      },
      error: (error: unknown) => {
        const message =
          error instanceof Error ? error.message : 'Could not delete product. Please try again.';
        this.error.set(message);
      },
      complete: () => {
        this.deletingProductId.set(null);
      }
    });
  }

  private loadProducts(): void {
    this.loading.set(true);
    this.productApiService.getProducts().subscribe({
      next: (products) => {
        this.products.set(products);
        this.error.set(null);
      },
      error: (error: unknown) => {
        const message =
          error instanceof Error ? error.message : 'Could not load products. Please try again.';
        this.error.set(message);
        this.products.set([]);
      },
      complete: () => {
        this.loading.set(false);
      }
    });
  }
}

