import { Routes } from '@angular/router';
import { ProductsListComponent } from './features/products/products-list.component';
import { CreateOrderComponent } from './features/orders/create-order.component';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'products'
  },
  {
    path: 'products',
    component: ProductsListComponent
  },
  {
    path: 'orders/create',
    component: CreateOrderComponent
  }
];
