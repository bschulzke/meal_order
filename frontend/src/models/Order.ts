export interface OrderItem {
  menuItemId: number;
  menuItemName: string;
  quantity: number;
  unitPrice: number;
}

// DiscountType should match backend enum: e.g., "Percent", "Fixed"
export interface OrderDiscount {
  discountId: number;
  name: string;
  type: string; // "Percent" or "Fixed" (case-sensitive, matches backend DiscountType)
  amount: number; // For "Percent", this is the percent (e.g., 5 for 5%). For "Fixed", this is the dollar amount.
}

export interface OrderTax {
  taxId: number;
  name: string;
  percentage: number;
}

export class Order {
  constructor(
    public id: number,
    public userId: number,
    public userFirstName: string,
    public userLastName: string,
    public createdAt: string,
    public items: OrderItem[],
    public discounts: OrderDiscount[],
    public taxes: OrderTax[]
  ) {}

  subtotal(): number {
    return this.items.reduce((sum, item) => sum + (item.quantity * item.unitPrice), 0);
  }

  totalDiscounts(): number {
    return this.discounts.reduce((sum, d) => {
      if (d.type === "percent") return sum + this.subtotal() * (d.amount / 100);
      if (d.type === "fixed") return sum + d.amount;
      return sum;
    }, 0);
  }

  totalTaxes(): number {
    const discounted = this.preTaxTotal();
    return this.taxes.reduce((sum, t) => sum + (discounted * (t.percentage / 100)), 0);
  }

  preTaxTotal(): number {
    return Math.max(0, this.subtotal() - this.totalDiscounts());
  }

  total(): number {
    return Math.max(0, this.preTaxTotal() + this.totalTaxes());
  }
}