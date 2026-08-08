import { BadRequestException, Body, Controller, HttpCode, Post } from '@nestjs/common';
import { ChatRequest, GatewayService } from './gateway.service';

@Controller('agent')
export class GatewayController {
  constructor(private readonly gateway: GatewayService) {}

  // POST /api/v1/agent/chat —— D2 开发态对话入口。
  // 真实 JWT/配额/上游路由在后续服务端阶段接入；本阶段返回严格合法的 Mock AgentReply。
  @Post('chat')
  @HttpCode(200)
  chat(@Body() body: Record<string, unknown>) {
    const crewId = typeof body?.crew_id === 'string' ? body.crew_id : '';
    const playerInput = typeof body?.player_input === 'string' ? body.player_input : '';
    const personaSnapshot = typeof body?.persona_snapshot === 'string'
      ? body.persona_snapshot
      : undefined;

    if (!crewId.trim() || !playerInput.trim()) {
      throw new BadRequestException({
        code: 'invalid_chat_request',
        message: 'crew_id and player_input are required',
      });
    }

    const request: ChatRequest = {
      crewId: crewId.trim(),
      playerInput: playerInput.trim(),
      personaSnapshot,
    };
    return this.gateway.chat(request);
  }
}
