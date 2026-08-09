import { Injectable } from '@nestjs/common';

type MarketAction = 'buy' | 'sell';

interface Commodity {
  id: string;
  displayName: string;
  basePrice: number;
  cargoVolume: number;
}

interface Port {
  id: string;
  tariffRate: number;
  specialties: Record<string, number>;
}

export interface TradeQuote {
  port_id: string;
  commodity_id: string;
  buy_unit_price: number;
  sell_unit_price: number;
  supply_coefficient: number;
  tariff_coefficient: number;
  event_coefficient: number;
}

export interface TradeResult {
  succeeded: boolean;
  code: string;
  message: string;
  quantity: number;
  total_price: number;
  money_after: number;
}

@Injectable()
export class TradeService {
  private readonly targetStock = 50;
  private readonly commodities: Record<string, Commodity> = {
    salt: { id: 'salt', displayName: '盐', basePrice: 4, cargoVolume: 1 },
    sugar: { id: 'sugar', displayName: '糖', basePrice: 6, cargoVolume: 1 },
    tobacco: { id: 'tobacco', displayName: '烟草', basePrice: 12, cargoVolume: 1 },
    rum: { id: 'rum', displayName: '朗姆酒', basePrice: 18, cargoVolume: 1 },
    coffee: { id: 'coffee', displayName: '咖啡', basePrice: 14, cargoVolume: 1 },
    cocoa: { id: 'cocoa', displayName: '可可', basePrice: 16, cargoVolume: 1 },
    spices: { id: 'spices', displayName: '香料', basePrice: 22, cargoVolume: 1 },
    dyes: { id: 'dyes', displayName: '染料', basePrice: 20, cargoVolume: 1 },
    timber: { id: 'timber', displayName: '木材', basePrice: 8, cargoVolume: 2 },
    medicine: { id: 'medicine', displayName: '药材', basePrice: 28, cargoVolume: 1 },
  };

  private readonly ports: Record<string, Port> = {
    santa_margarita: { id: 'santa_margarita', tariffRate: 0.08, specialties: { tobacco: 0.86, rum: 0.9 } },
    green_cay: { id: 'green_cay', tariffRate: 0.04, specialties: { sugar: 0.82, cocoa: 0.86 } },
    driftwood_isle: { id: 'driftwood_isle', tariffRate: 0, specialties: { timber: 0.78, dyes: 0.88, spices: 0.9 } },
  };

  quote(portId: string, commodityId: string, stock = 50, eventCoefficient = 1): TradeQuote {
    const port = this.ports[portId];
    const commodity = this.commodities[commodityId];
    if (!port || !commodity) throw new Error('invalid_reference');
    const safeStock = Math.max(0, Math.trunc(stock));
    const supply = this.clamp(1 + (this.targetStock - safeStock) / 100, 0.5, 1.8);
    const specialty = port.specialties[commodityId] ?? 1;
    const event = this.clamp(eventCoefficient, 0.5, 1.5);
    const buyTariff = 1 + port.tariffRate;
    return {
      port_id: portId,
      commodity_id: commodityId,
      buy_unit_price: this.price(commodity.basePrice * supply * specialty * buyTariff * event),
      sell_unit_price: this.price(commodity.basePrice * supply * specialty * (1 - port.tariffRate * 0.5) * event * 0.75),
      supply_coefficient: supply,
      tariff_coefficient: buyTariff,
      event_coefficient: event,
    };
  }

  order(input: {
    portId: string;
    commodityId: string;
    action: MarketAction;
    quantity: number;
    money: number;
    cargoCount: number;
    cargoCapacity: number;
    marketStock: number;
    eventCoefficient: number;
  }): TradeResult {
    const quantity = Math.trunc(input.quantity);
    if (quantity <= 0) return this.fail('invalid_quantity', '交易数量必须大于零。', input.money);
    const commodity = this.commodities[input.commodityId];
    if (!commodity) return this.fail('invalid_reference', '货物不存在。', input.money);
    const quote = this.quote(input.portId, input.commodityId, input.marketStock, input.eventCoefficient);
    const money = Math.max(0, Math.trunc(input.money));
    const total = (input.action === 'buy' ? quote.buy_unit_price : quote.sell_unit_price) * quantity;
    if (input.action === 'buy') {
      if (Math.trunc(input.marketStock) < quantity) return this.fail('market_empty', '港口库存不足。', money);
      if (total > money) return this.fail('not_enough_money', '金币不足。', money);
      if (Math.trunc(input.cargoCount) + quantity * commodity.cargoVolume > Math.trunc(input.cargoCapacity)) {
        return this.fail('cargo_full', '货舱容量不足。', money);
      }
      return { succeeded: true, code: 'ok', message: '购买完成。', quantity, total_price: total, money_after: money - total };
    }
    if (input.action === 'sell') {
      if (Math.trunc(input.cargoCount) < quantity) return this.fail('not_enough_cargo', '货舱中没有足够的货物。', money);
      return { succeeded: true, code: 'ok', message: '出售完成。', quantity, total_price: total, money_after: money + total };
    }
    return this.fail('invalid_action', '交易方向无效。', money);
  }

  settle(input: Record<string, unknown>) {
    return {
      succeeded: input.succeeded === true,
      origin_id: typeof input.origin_id === 'string' ? input.origin_id : '',
      destination_id: typeof input.destination_id === 'string' ? input.destination_id : '',
      days_elapsed: this.nonNegativeInt(input.days_elapsed),
      events: Array.isArray(input.events) ? input.events.filter((event): event is string => typeof event === 'string') : [],
      water_remaining: this.nonNegativeInt(input.water_remaining),
      food_remaining: this.nonNegativeInt(input.food_remaining),
      hull_remaining: this.nonNegativeInt(input.hull_remaining),
      message: typeof input.message === 'string' ? input.message : '航程结算镜像已接受。',
    };
  }

  private fail(code: string, message: string, money: number): TradeResult {
    return { succeeded: false, code, message, quantity: 0, total_price: 0, money_after: Math.max(0, Math.trunc(money)) };
  }

  private price(value: number): number {
    return Math.max(1, Math.round(value));
  }

  private clamp(value: number, min: number, max: number): number {
    return Number.isFinite(value) ? Math.max(min, Math.min(max, value)) : 1;
  }

  private nonNegativeInt(value: unknown): number {
    return typeof value === 'number' && Number.isFinite(value) ? Math.max(0, Math.trunc(value)) : 0;
  }
}
