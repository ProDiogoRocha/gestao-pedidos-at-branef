
export interface ProductDto {
  id: string;
  name: string;
  price: number;
}

export interface OrderItemRequest {
  productId: string;
  quantity: number;
}

export interface CreateOrderRequest {
  items: OrderItemRequest[];
}

export interface UpdateOrderRequest {
  items: OrderItemRequest[];
  status: number;
}

export interface OrderItemReadDto {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface OrderReadDto {
  id: string;
  customerId: string;
  orderDate: string;
  totalAmount: number;
  status: number;
  items: OrderItemReadDto[];
}
