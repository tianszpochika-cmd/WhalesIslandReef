import { Injectable } from '@nestjs/common';

export interface ChatRequest {
  crewId: string;
  playerInput: string;
}

@Injectable()
export class GatewayService {
  // ★ LLM 网关核心（骨架占位）
  //
  // 设计链路（对应 contracts/openapi/v1.yaml 的 /agent/chat）：
  //   鉴权(JWT) → 配额校验(billing) → 上游路由(本地 Ollama 优先 → 云端 DeepSeek 兜底)
  //   → metering 计量(入+出 token) → 扣减余额 → 流式返回(WS/SSE)
  //
  // 待玩法关键词/需求文档定稿后实现：
  //   - 路由策略与上游注册表（按成本/质量/可用性）
  //   - Persona + 记忆快照的传递方式（客户端上送 vs 服务端拉取）
  //   - 重试、熔断、超时与降级（回落模板对话）
  async chat(req: ChatRequest): Promise<unknown> {
    return {
      status: 'not_implemented',
      hint: '待网关路由与上游接入',
      crew_id: req.crewId,
    };
  }
}
