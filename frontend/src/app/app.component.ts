
import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterLink, RouterOutlet],
  template: `
    <div class="container">
      <h1>Gestão de Pedidos</h1>
      <div class="row" style="margin-bottom: 12px;">
        <a class="btn" routerLink="/orders">Pedidos</a>
        <a class="btn primary" routerLink="/orders/new">Novo Pedido</a>
      </div>
      <router-outlet />
      <p class="small">API: /api (proxy no nginx)</p>
    </div>
  `
})
export class AppComponent {}
