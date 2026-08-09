import { Module } from '@nestjs/common';
import { HealthModule } from './modules/health/health.module';
import { AuthModule } from './modules/auth/auth.module';
import { BillingModule } from './modules/billing/billing.module';
import { GatewayModule } from './modules/gateway/gateway.module';
import { WorkerModule } from './modules/worker/worker.module';
import { TradeModule } from './modules/trade/trade.module';
import { D4Module } from './modules/d4/d4.module';

@Module({
  imports: [HealthModule, AuthModule, BillingModule, GatewayModule, WorkerModule, TradeModule, D4Module],
})
export class AppModule {}
