import { Module } from '@nestjs/common';

@Module({})
export class WorkerModule {
  // 占位：定时任务（日结算 / 用量统计 / 告警 / 会话摘要）
  // 依赖 DB 迁移与 billing 落地后实现（@nestjs/schedule + Prisma）。
}
