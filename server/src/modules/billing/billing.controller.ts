import { Controller, Get } from '@nestjs/common';

@Controller('wallet')
export class BillingController {
  // GET /api/v1/wallet —— 玩家点数余额与用量（用户中心/游戏内展示）
  @Get()
  wallet() {
    return {
      points_balance: 0,
      monthly_usage_tokens: 0,
      monthly_limit_tokens: 0,
      hint: '待 DB 迁移与 token 商业模型定稿',
    };
  }
}
