
import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule, DatePipe, CurrencyPipe } from '@angular/common';
import { OrdersApiService } from '../services/orders-api.service';
import { OrderReadDto } from '../types/models';

@Component({
  standalone: true,
  imports: [CommonModule, RouterLink, DatePipe, CurrencyPipe],
  template: `
    <div class="card">
      <h2>Pedidos</h2>
      <button class="btn" (click)="load()">Atualizar</button>
      <p class="small" *ngIf="loading">Carregando...</p>
      <p class="small" *ngIf="!loading">Total: {{orders.length}}</p>
    </div>

    <div class="card" *ngFor="let o of orders">
      <div class="row" style="justify-content: space-between; align-items: center;">
        <div>
          <div><strong>#{{o.id}}</strong></div>
          <div class="small">{{o.orderDate | date:'short'}}</div>
        </div>
        <div>
          <span class="badge">Status: {{statusLabel(o.status)}}</span>
        </div>
        <div>
          <strong>{{o.totalAmount | currency:'BRL'}}</strong>
        </div>
      </div>

      <div style="margin-top: 10px;">
        <a class="btn" [routerLink]="['/orders', o.id]">Detalhar / Editar</a>
        <button class="btn" (click)="remove(o.id)">Remover</button>
      </div>
    </div>
  `
})
export class OrdersListComponent implements OnInit {
  orders: OrderReadDto[] = [];
  loading = false;

  constructor(
    private readonly api: OrdersApiService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    console.log('[OrdersList] Carregando pedidos...');
    this.api.listOrders().subscribe({
      next: (data) => {
        console.log('[OrdersList] Pedidos recebidos:', data);
        this.orders = data ?? [];
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('[OrdersList] Erro ao carregar pedidos:', err);
        this.orders = [];
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  remove(id: string): void {
    if (!confirm('Remover este pedido?')) return;
    this.api.deleteOrder(id).subscribe(() => this.load());
  }

  statusLabel(status: number): string {
    switch (status) {
      case 0: return 'Created';
      case 1: return 'Paid';
      case 2: return 'Shipped';
      case 3: return 'Delivered';
      case 4: return 'Cancelled';
      default: return String(status);
    }
  }
}
