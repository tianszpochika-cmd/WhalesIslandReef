import { Injectable } from '@nestjs/common';

export interface ChatRequest {
  crewId: string;
  playerInput: string;
  personaSnapshot?: string;
}

export type AgentEmotion = 'neutral' | 'suspicious' | 'happy' | 'angry' | 'sad' | 'afraid';

export interface AgentReply {
  speech: string;
  emotion: AgentEmotion;
  affinity_change: number;
  action: 'none';
  action_args: string[];
}

@Injectable()
export class GatewayService {
  // D2 开发态网关：先提供可重复的合法回复，确保服务端接线和 Unity Mock
  // 不被真实账号、配额或上游模型阻塞。生产路由在本模块边界内替换。
  async chat(req: ChatRequest): Promise<AgentReply> {
    const input = req.playerInput.trim();
    const reply: AgentReply = input.includes('天气')
      ? {
          speech: '海风正好，船长。',
          emotion: 'happy',
          affinity_change: 1,
          action: 'none',
          action_args: [],
        }
      : {
          speech: '先把手上的活干完，船长。',
          emotion: 'neutral',
          affinity_change: 0,
          action: 'none',
          action_args: [],
        };

    this.assertReply(reply);
    return reply;
  }

  private assertReply(reply: AgentReply): void {
    if (!reply.speech || reply.speech.length < 1) throw new Error('invalid_agent_reply_speech');
    if (reply.affinity_change < -10 || reply.affinity_change > 10
      || !Number.isInteger(reply.affinity_change)) {
      throw new Error('invalid_agent_reply_affinity');
    }
    if (reply.action !== 'none') throw new Error('invalid_agent_reply_action');
  }
}
