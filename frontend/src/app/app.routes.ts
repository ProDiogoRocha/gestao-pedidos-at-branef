
import { Routes } from '@angular/router';
import { OrdersListComponent } from './pages/orders-list.component';
import { OrderFormComponent } from './pages/order-form.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'orders' },
  { path: 'orders', component: OrdersListComponent },
  { path: 'orders/new', component: OrderFormComponent },
  { path: 'orders/:id', component: OrderFormComponent, data: { isEdit: true } }
];
