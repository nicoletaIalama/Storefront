import { Routes } from '@angular/router';
import { ProductsListComponent } from './features/products/products-list.component';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'products'
  },
  {
    path: 'products',
    component: ProductsListComponent
  }
];
