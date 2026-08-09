import { BadRequestException, Body, Controller, HttpCode, Post } from '@nestjs/common';
import { TradeService } from './trade.service';

@Controller()
export class TradeController {
  constructor(private readonly trade: TradeService) {}

  @Post('trade/quote')
  @HttpCode(200)
  quote(@Body() body: Record<string, unknown>) {
    const portId = this.string(body.port_id);
    const commodityId = this.string(body.commodity_id);
    if (!portId || !commodityId) throw new BadRequestException('port_id and commodity_id are required');
    try {
      return this.trade.quote(portId, commodityId, this.number(body.market_stock, 50), this.number(body.event_coefficient, 1));
    } catch {
      throw new BadRequestException('invalid trade reference');
    }
  }

  @Post('trade/order')
  @HttpCode(200)
  order(@Body() body: Record<string, unknown>) {
    const portId = this.string(body.port_id);
    const commodityId = this.string(body.commodity_id);
    const action = body.action === 'buy' || body.action === 'sell' ? body.action : null;
    if (!portId || !commodityId || !action) throw new BadRequestException('invalid trade order');
    try {
      return this.trade.order({
        portId,
        commodityId,
        action,
        quantity: this.number(body.quantity, 0),
        money: this.number(body.money, 0),
        cargoCount: this.number(body.cargo_count, 0),
        cargoCapacity: this.number(body.cargo_capacity, 0),
        marketStock: this.number(body.market_stock, 50),
        eventCoefficient: this.number(body.event_coefficient, 1),
      });
    } catch {
      throw new BadRequestException('invalid trade order');
    }
  }

  @Post('voyage/settle')
  @HttpCode(200)
  settle(@Body() body: Record<string, unknown>) {
    if (!body || typeof body.origin_id !== 'string' || typeof body.destination_id !== 'string') {
      throw new BadRequestException('origin_id and destination_id are required');
    }
    return this.trade.settle(body);
  }

  private string(value: unknown): string {
    return typeof value === 'string' ? value.trim() : '';
  }

  private number(value: unknown, fallback: number): number {
    return typeof value === 'number' && Number.isFinite(value) ? value : fallback;
  }
}
