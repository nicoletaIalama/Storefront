export interface Product {
  // Backend returns `Guid` serialized as a string, e.g. "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  id: string;
  name: string;
  price: number;
  description?: string | null;
  isActive?: boolean;
}

