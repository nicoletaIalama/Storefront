export type OrderStatus = 'PendingPayment' | 'Paid' | 'PaymentFailed';

export interface OrderLineItemDto {
  productId: string;
  quantity: number;
}

export interface MockPaymentDto {
  succeeded: boolean;
  provider: string;
  failureReason?: string | null;
}

export interface OrderDto {
  orderId: string;
  lineItems: OrderLineItemDto[];
  total: number;
  status: OrderStatus;
  payment?: MockPaymentDto | null;
}

export interface CreateOrderRequest {
  lineItems: CreateOrderLineItemRequest[];
}

export interface CreateOrderLineItemRequest {
  productId: string;
  quantity: number;
}

export type MockPaymentResult = 'success' | 'failure';

export interface MockPaymentRequest {
  // Backend accepts case-insensitive "success" or "failure".
  result: MockPaymentResult | string;
  failureReason?: string | null;
}

