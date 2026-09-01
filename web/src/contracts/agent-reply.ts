/**
 * Shared AgentReply v1.1 consumer for the future web surface.
 *
 * This module is deliberately dependency-free: web presentation may consume a
 * validated proposal, but it must not interpret it as an authoritative world
 * result. Business owners remain responsible for applying action/relationship
 * proposals.
 */

export const AGENT_REPLY_SCHEMA_VERSION = 'agent_reply.v1.1' as const;

export type AgentReply = {
  schema_version: typeof AGENT_REPLY_SCHEMA_VERSION;
  speech: string;
  dialogue_intent: string;
  emotion: string;
  affinity_change: number;
  relation_proposal: {
    kind: string;
    target_ref: string;
    stance: string;
    basis: string;
  };
  negotiation_tone: string;
  refusal_reason: string;
  action_intent: {
    name: string;
    actor_ref: string;
    target_ref: string;
    parameters: Record<string, unknown>;
  };
};

const TOP_LEVEL_KEYS = [
  'schema_version', 'speech', 'dialogue_intent', 'emotion',
  'affinity_change', 'relation_proposal', 'negotiation_tone',
  'refusal_reason', 'action_intent',
] as const;

const FORBIDDEN_RESULT_KEYS = new Set([
  'money', 'price', 'damage', 'reward', 'weather', 'relation_result',
  'relation_value', 'affinity_result', 'final_money', 'final_price',
  'final_damage', 'final_reward', 'final_weather', 'final_relation',
]);

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}

function hasForbiddenResultKey(value: unknown): boolean {
  if (Array.isArray(value)) return value.some(hasForbiddenResultKey);
  if (!isRecord(value)) return false;
  return Object.entries(value).some(([key, child]) =>
    FORBIDDEN_RESULT_KEYS.has(key.toLowerCase()) || hasForbiddenResultKey(child));
}

function hasExactKeys(value: Record<string, unknown>, keys: readonly string[]): boolean {
  const expected = new Set(keys);
  const actual = Object.keys(value);
  return actual.length === expected.size && actual.every((key) => expected.has(key));
}

function isReference(value: unknown): value is string {
  return value === 'none' ||
    (typeof value === 'string' && /^[a-z][a-z0-9_]*:[A-Za-z0-9._:-]+$/.test(value));
}

/** Validate the shared proposal shape before the web layer renders it. */
export function parseAgentReply(value: unknown): AgentReply {
  if (!isRecord(value) || !hasExactKeys(value, TOP_LEVEL_KEYS)) {
    throw new Error('invalid_agent_reply_fields');
  }
  if (value.schema_version !== AGENT_REPLY_SCHEMA_VERSION) {
    throw new Error('invalid_agent_reply_schema_version');
  }
  if (typeof value.speech !== 'string' || value.speech.length < 1 || value.speech.length > 240) {
    throw new Error('invalid_agent_reply_speech');
  }
  if (!Number.isInteger(value.affinity_change) || value.affinity_change < -10 || value.affinity_change > 10) {
    throw new Error('invalid_agent_reply_affinity');
  }
  if (!isRecord(value.relation_proposal) ||
      !hasExactKeys(value.relation_proposal, ['kind', 'target_ref', 'stance', 'basis']) ||
      !isReference(value.relation_proposal.target_ref) ||
      typeof value.relation_proposal.basis !== 'string' ||
      value.relation_proposal.basis.length < 1 || value.relation_proposal.basis.length > 160) {
    throw new Error('invalid_agent_reply_relation');
  }
  if (!isRecord(value.action_intent) ||
      !hasExactKeys(value.action_intent, ['name', 'actor_ref', 'target_ref', 'parameters']) ||
      typeof value.action_intent.name !== 'string' ||
      !isReference(value.action_intent.actor_ref) ||
      !isReference(value.action_intent.target_ref) ||
      !isRecord(value.action_intent.parameters) ||
      hasForbiddenResultKey(value.action_intent.parameters)) {
    throw new Error('invalid_agent_reply_action_intent');
  }
  if (value.relation_proposal.kind === 'none' &&
      (value.relation_proposal.target_ref !== 'none' || value.relation_proposal.stance !== 'none')) {
    throw new Error('invalid_agent_reply_no_relation');
  }
  if (value.action_intent.name === 'none' &&
      (value.action_intent.actor_ref !== 'none' || value.action_intent.target_ref !== 'none' ||
       Object.keys(value.action_intent.parameters).length !== 0)) {
    throw new Error('invalid_agent_reply_noop_action');
  }
  return value as unknown as AgentReply;
}
