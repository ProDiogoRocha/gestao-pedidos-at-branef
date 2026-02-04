
import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { OrdersApiService } from '../services/orders-api.service';
import { ProductDto, OrderReadDto } from '../types/models';

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, CurrencyPipe, DatePipe],
  template: `
    <div class="card">
      <h2 *ngIf="!id">Novo Pedido</h2>
      <h2 *ngIf="id">Editar Pedido</h2>
      <p class="small" *ngIf="id && order">Criado em: {{order.orderDate | date:'short'}} | Total: {{order.totalAmount | currency:'BRL'}}</p>
    </div>

    <div class="card" *ngIf="loading">Carregando...</div>

    <div class="card" *ngIf="!loading">
      <h3>Itens (3 produtos fixos)</h3>

      <div *ngFor="let p of products" class="row" style="align-items:center; justify-content: space-between;">
        <div style="min-width: 220px;">
          <strong>{{p.name}}</strong>
          <div class="small">{{p.price | currency:'BRL'}}</div>
        </div>
        <div>
          <input class="input" type="number" min="0" [value]="qty(p.id)" (input)="setQty(p.id, $any($event.target).value)" />
        </div>
      </div>

      <div style="margin-top: 12px;">
        <label class="small">Status</label><br/>
        <select class="input" [formControl]="form.controls['status']">
          <option [value]="0">Created</option>
          <option [value]="1">Paid</option>
          <option [value]="2">Shipped</option>
          <option [value]="3">Delivered</option>
          <option [value]="4">Cancelled</option>
        </select>
      </div>

      <div class="row" style="margin-top: 12px;">
        <button class="btn primary" (click)="save()">Salvar</button>
        <a class="btn" routerLink="/orders">Voltar</a>
      </div>

      <p class="small" style="margin-top: 12px;">
        Dica: o read-model (lista) vem do Mongo. Pode levar ~1s para refletir após salvar.
      </p>
    </div>
  `
})
export class OrderFormComponent implements OnInit {
  id: string | null = null;
  products: ProductDto[] = [];
  order: OrderReadDto | null = null;
  loading = false;

  quantities = new Map<string, number>();

  form: FormGroup = this.fb.group({
    status: 0
  });

  constructor(
    private readonly fb: FormBuilder,
    private readonly api: OrdersApiService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.id = this.route.snapshot.paramMap.get('id');
    this.loadProducts();

    if (this.id) {
      this.loading = true;
      console.log(`[OrderForm] Carregando pedido: ${this.id}`);
      this.api.getOrder(this.id).subscribe({
        next: (o) => {
          console.log('[OrderForm] Pedido recebido:', o);
          this.order = o;
          if (o) {
            this.form.patchValue({ status: o.status });
            this.quantities.clear();
            for (const it of o.items ?? []) {
              this.quantities.set(it.productId, it.quantity);
            }
          }
          this.loading = false;
          this.cdr.markForCheck();
        },
        error: (err) => {
          console.error('[OrderForm] Erro ao carregar pedido:', err);
          this.loading = false;
        }
      });
    }
  }

  private loadProducts(): void {
    this.api.listProducts().subscribe({
      next: (p) => {
        console.log('[OrderForm] Produtos recebidos:', p);
        this.products = p ?? [];
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('[OrderForm] Erro ao carregar produtos:', err);
        this.products = [];
      }
    });
  }

  qty(productId: string): number {
    return this.quantities.get(productId) ?? 0;
  }

  setQty(productId: string, value: string): void {
    const n = Number(value ?? 0);
    if (!Number.isFinite(n) || n < 0) return;
    this.quantities.set(productId, Math.floor(n));
  }

  save(): void {
    const items = Array.from(this.quantities.entries())
      .filter(([_, q]) => q > 0)
      .map(([productId, quantity]) => ({ productId, quantity }));

    if (items.length === 0) {
      alert('Informe pelo menos 1 item com quantidade > 0.');
      return;
    }

    const status = Number(this.form.value.status ?? 0);

    if (!this.id) {
      this.api.createOrder({ items }).subscribe({
        next: () => this.router.navigateByUrl('/orders'),
        error: () => alert('Erro ao criar pedido.')
      });
      return;
    }

    this.api.updateOrder(this.id, { items, status }).subscribe({
      next: () => this.router.navigateByUrl('/orders'),
      error: () => alert('Erro ao atualizar pedido.')
    });
  }
}
