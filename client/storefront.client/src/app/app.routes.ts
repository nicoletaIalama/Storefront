import { Routes } from '@angular/router';
import { ProductsListComponent } from './features/products/products-list.component';
import { CreateOrderComponent } from './features/orders/create-order.component';
import { RetrieveOrderComponent } from './features/orders/retrieve-order.component';
import { LoginComponent } from './features/auth/login.component';
import { HomeComponent } from './features/home/home.component';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    component: HomeComponent
  },
  {
    path: 'products',
    component: ProductsListComponent
  },
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: 'orders/create',
    component: CreateOrderComponent
  },
  {
    path: 'orders/retrieve',
    component: RetrieveOrderComponent
  }
];
