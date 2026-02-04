
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateOrderRequest, OrderReadDto, ProductDto, UpdateOrderRequest } from '../types/models';

@Injectable({ providedIn: 'root' })
export class OrdersApiService {
  private readonly baseUrl = '/api';

  constructor(private readonly http: HttpClient) {}

  listOrders(): Observable<OrderReadDto[]> {
    return this.http.get<OrderReadDto[]>(`${this.baseUrl}/orders`);
  }

  getOrder(id: string): Observable<OrderReadDto | null> {
    return this.http.get<OrderReadDto | null>(`${this.baseUrl}/orders/${id}`);
  }

  createOrder(req: CreateOrderRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.baseUrl}/orders`, req);
  }

  updateOrder(id: string, req: UpdateOrderRequest): Observable<{ ok: boolean }> {
    return this.http.put<{ ok: boolean }>(`${this.baseUrl}/orders/${id}`, req);
  }

  deleteOrder(id: string): Observable<{ ok: boolean }> {
    return this.http.delete<{ ok: boolean }>(`${this.baseUrl}/orders/${id}`);
  }

  listProducts(): Observable<ProductDto[]> {
    return this.http.get<ProductDto[]>(`${this.baseUrl}/products`);
  }
}
