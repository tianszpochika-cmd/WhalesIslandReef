import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { Injectable } from '@nestjs/common';

export interface ChatRequest {
  crewId: string;
  playerInput: string;
  personaSnapshot?: string;
}

export type AgentEmotion =
  | 'neutral' | 'suspicious' | 'happy' | 'angry' | 'sad' | 'afraid'
  | 'calm' | 'concerned' | 'determined' | 'relieved' | 'disgusted' | 'hopeful';
export type DialogueIntent = 'greet' | 'inform' | 'ask' | 'answer' | 'warn' | 'suggest' | 'negotiate' | 'refuse' | 'apologize' | 'report' | 'confirm' | 'farewell';
export type RelationKind = 'none' | 'trust' | 'respect' | 'affinity' | 'loyalty' | 'fear' | 'rivalry' | 'romance' | 'professional';
export type RelationStance = 'none' | 'build' | 'maintain' | 'repair' | 'challenge' | 'avoid' | 'request_mediation' | 'offer_cooperation' | 'accept' | 'decline';
export type NegotiationTone = 'neutral' | 'cooperative' | 'firm' | 'conciliatory' | 'guarded' | 'competitive' | 'formal' | 'urgent';
export type RefusalReason = 'none' | 'unknown_request' | 'insufficient_authority' | 'unsafe_target' | 'missing_prerequisite' | 'conflicting_order' | 'personal_boundary' | 'legal_restriction' | 'resource_unavailable' | 'out_of_scope' | 'not_in_character' | 'ambiguous_intent';

export interface RelationProposal {
  kind: RelationKind;
  target_ref: string;
  stance: RelationStance;
  basis: string;
}

export interface ActionIntent {
  name: string;
  actor_ref: string;
  target_ref: string;
  parameters: Record<string, unknown>;
}

export interface AgentReply {
  schema_version: 'agent_reply.v1.1';
  speech: string;
  dialogue_intent: DialogueIntent;
  emotion: AgentEmotion;
  affinity_change: number;
  relation_proposal: RelationProposal;
  negotiation_tone: NegotiationTone;
  refusal_reason: RefusalReason;
  action_intent: ActionIntent;
}

export interface ReplyValidationContext {
  authorizedActorRefs?: ReadonlySet<string>;
  authorizedTargetRefs?: ReadonlySet<string>;
  permissions?: ReadonlySet<string>;
  satisfiedPrerequisites?: ReadonlySet<string>;
  enforceAuthorization?: boolean;
}

interface ParameterDefinition {
  type: 'string' | 'integer' | 'boolean';
  enum?: string[];
  pattern?: string;
  minimum?: number;
  maximum?: number;
}

interface ActionDefinition {
  name: string;
  category: string;
  parameter_schema: {
    required?: string[];
    properties?: Record<string, ParameterDefinition>;
  };
  permissions: string[];
  prerequisites: string[];
  failure_codes: string[];
  executor: string;
  allowed_actor_kinds: string[];
  allowed_target_kinds: string[];
}

interface ActionRegistryDocument {
  registry_version: 'p2a.v1';
  actions: ActionDefinition[];
}

const registryPath = resolve(__dirname, '../../../../contracts/p2a-action-registry.json');
const registry = JSON.parse(readFileSync(registryPath, 'utf8')) as ActionRegistryDocument;
export const P2A_ACTIONS: ReadonlyMap<string, ActionDefinition> = new Map(registry.actions.map((action) => [action.name, action]));

const TOP_LEVEL_KEYS = new Set([
  'schema_version', 'speech', 'dialogue_intent', 'emotion', 'affinity_change',
  'relation_proposal', 'negotiation_tone', 'refusal_reason', 'action_intent',
]);
const DIALOGUE_INTENTS = new Set<DialogueIntent>(['greet', 'inform', 'ask', 'answer', 'warn', 'suggest', 'negotiate', 'refuse', 'apologize', 'report', 'confirm', 'farewell']);
const EMOTIONS = new Set<AgentEmotion>(['neutral', 'suspicious', 'happy', 'angry', 'sad', 'afraid', 'calm', 'concerned', 'determined', 'relieved', 'disgusted', 'hopeful']);
const RELATION_KINDS = new Set<RelationKind>(['none', 'trust', 'respect', 'affinity', 'loyalty', 'fear', 'rivalry', 'romance', 'professional']);
const RELATION_STANCES = new Set<RelationStance>(['none', 'build', 'maintain', 'repair', 'challenge', 'avoid', 'request_mediation', 'offer_cooperation', 'accept', 'decline']);
const NEGOTIATION_TONES = new Set<NegotiationTone>(['neutral', 'cooperative', 'firm', 'conciliatory', 'guarded', 'competitive', 'formal', 'urgent']);
const REFUSAL_REASONS = new Set<RefusalReason>(['none', 'unknown_request', 'insufficient_authority', 'unsafe_target', 'missing_prerequisite', 'conflicting_order', 'personal_boundary', 'legal_restriction', 'resource_unavailable', 'out_of_scope', 'not_in_character', 'ambiguous_intent']);
const FORBIDDEN_RESULT_FIELDS = new Set(['money', 'price', 'damage', 'reward', 'weather', 'relation_result', 'relation_value', 'affinity_result', 'final_money', 'final_price', 'final_damage', 'final_reward', 'final_weather', 'final_relation']);

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}

function assertCode(condition: boolean, code: string): asserts condition {
  if (!condition) throw new Error(code);
}

function assertExactKeys(value: Record<string, unknown>, keys: readonly string[], code: string): void {
  const expected = new Set(keys);
  assertCode(Object.keys(value).length === expected.size && Object.keys(value).every((key) => expected.has(key)), code);
}

function assertEnum<T extends string>(value: unknown, values: ReadonlySet<T>, code: string): asserts value is T {
  assertCode(typeof value === 'string' && values.has(value as T), code);
}

function assertReference(value: unknown, code: string): asserts value is string {
  assertCode(value === 'none' || (typeof value === 'string' && /^[a-z][a-z0-9_]*:[A-Za-z0-9._:-]+$/.test(value)), code);
}

function referenceKind(reference: string): string {
  return reference === 'none' ? 'none' : reference.split(':', 1)[0];
}

function containsForbiddenField(value: unknown): boolean {
  if (Array.isArray(value)) return value.some(containsForbiddenField);
  if (!isRecord(value)) return false;
  return Object.entries(value).some(([key, child]) => FORBIDDEN_RESULT_FIELDS.has(key.toLowerCase()) || containsForbiddenField(child));
}

export function validateAgentReply(value: unknown, context: ReplyValidationContext = {}): AgentReply {
  assertCode(isRecord(value), 'invalid_agent_reply_object');
  assertExactKeys(value, [...TOP_LEVEL_KEYS], 'invalid_agent_reply_fields');
  assertCode(value.schema_version === 'agent_reply.v1.1', 'invalid_agent_reply_schema_version');
  assertCode(typeof value.speech === 'string' && value.speech.trim().length > 0, 'invalid_agent_reply_speech');
  assertCode(value.speech.length <= 240, 'invalid_agent_reply_speech_length');
  assertEnum(value.dialogue_intent, DIALOGUE_INTENTS, 'invalid_agent_reply_dialogue_intent');
  assertEnum(value.emotion, EMOTIONS, 'invalid_agent_reply_emotion');
  assertCode(typeof value.affinity_change === 'number' && Number.isInteger(value.affinity_change) && value.affinity_change >= -10 && value.affinity_change <= 10, 'invalid_agent_reply_affinity');

  assertCode(isRecord(value.relation_proposal), 'invalid_agent_reply_relation');
  assertExactKeys(value.relation_proposal, ['kind', 'target_ref', 'stance', 'basis'], 'invalid_agent_reply_relation');
  assertEnum(value.relation_proposal.kind, RELATION_KINDS, 'invalid_agent_reply_relation');
  assertReference(value.relation_proposal.target_ref, 'invalid_agent_reply_relation');
  assertEnum(value.relation_proposal.stance, RELATION_STANCES, 'invalid_agent_reply_relation');
  assertCode(typeof value.relation_proposal.basis === 'string' && value.relation_proposal.basis.length >= 1 && value.relation_proposal.basis.length <= 160, 'invalid_agent_reply_relation');
  if (value.relation_proposal.kind === 'none') assertCode(value.relation_proposal.target_ref === 'none' && value.relation_proposal.stance === 'none', 'invalid_agent_reply_relation');

  assertEnum(value.negotiation_tone, NEGOTIATION_TONES, 'invalid_agent_reply_negotiation_tone');
  assertEnum(value.refusal_reason, REFUSAL_REASONS, 'invalid_agent_reply_refusal_reason');
  assertCode(isRecord(value.action_intent), 'invalid_agent_reply_action_intent');
  assertExactKeys(value.action_intent, ['name', 'actor_ref', 'target_ref', 'parameters'], 'invalid_agent_reply_action_intent');
  assertCode(typeof value.action_intent.name === 'string', 'invalid_agent_reply_action_name');
  assertReference(value.action_intent.actor_ref, 'invalid_agent_reply_action_reference');
  assertReference(value.action_intent.target_ref, 'invalid_agent_reply_action_reference');
  assertCode(isRecord(value.action_intent.parameters), 'invalid_agent_reply_action_parameters');
  assertCode(!containsForbiddenField(value.action_intent.parameters), 'forbidden_result_field');
  const enforce = context.enforceAuthorization !== false;
  if (enforce && value.relation_proposal.target_ref !== 'none') {
    assertCode(context.authorizedTargetRefs?.has(value.relation_proposal.target_ref) === true, 'unauthorized_target');
  }

  if (value.action_intent.name === 'none') {
    assertCode(value.action_intent.actor_ref === 'none' && value.action_intent.target_ref === 'none' && Object.keys(value.action_intent.parameters).length === 0, 'invalid_agent_reply_noop_action');
  } else {
    const action = P2A_ACTIONS.get(value.action_intent.name);
    assertCode(action !== undefined, 'unknown_action');
    assertCode(action.allowed_actor_kinds.includes(referenceKind(value.action_intent.actor_ref)), 'invalid_target_kind');
    assertCode(action.allowed_target_kinds.includes(referenceKind(value.action_intent.target_ref)), 'invalid_target_kind');
    const schema = action.parameter_schema;
    const properties = schema.properties ?? {};
    for (const key of Object.keys(value.action_intent.parameters)) {
      const definition = properties[key];
      assertCode(definition !== undefined, 'invalid_action_parameters');
      const parameterValue = value.action_intent.parameters[key];
      if (definition.type === 'string') {
        assertCode(typeof parameterValue === 'string', 'invalid_action_parameters');
        if (definition.enum) assertCode(definition.enum.includes(parameterValue), 'invalid_action_parameters');
        if (definition.pattern) assertCode(new RegExp(definition.pattern).test(parameterValue), 'invalid_action_parameters');
      } else if (definition.type === 'integer') {
        assertCode(typeof parameterValue === 'number' && Number.isInteger(parameterValue), 'invalid_action_parameters');
        if (definition.minimum !== undefined) assertCode(parameterValue >= definition.minimum, 'invalid_action_parameters');
        if (definition.maximum !== undefined) assertCode(parameterValue <= definition.maximum, 'invalid_action_parameters');
      } else {
        assertCode(typeof parameterValue === 'boolean', 'invalid_action_parameters');
      }
    }
    for (const required of schema.required ?? []) assertCode(Object.prototype.hasOwnProperty.call(value.action_intent.parameters, required), 'missing_action_parameter');
    if (enforce) {
      assertCode(context.authorizedActorRefs?.has(value.action_intent.actor_ref) === true, 'unauthorized_actor');
      if (value.action_intent.target_ref !== 'none') assertCode(context.authorizedTargetRefs?.has(value.action_intent.target_ref) === true, 'unauthorized_target');
      for (const permission of action.permissions) assertCode(context.permissions?.has(permission) === true, 'permission_denied');
      for (const prerequisite of action.prerequisites) assertCode(context.satisfiedPrerequisites?.has(prerequisite) === true, 'missing_prerequisite');
    }
  }
  if (value.refusal_reason !== 'none') assertCode(value.action_intent.name === 'none', 'refusal_with_action');
  return value as unknown as AgentReply;
}

@Injectable()
export class GatewayService {
  // D8-A04 网关 Mock：返回完整 v1.1 合法回复；真实上游输出必须复用 validateAgentReply。
  async chat(req: ChatRequest): Promise<AgentReply> {
    const input = req.playerInput.trim();
    const reply: AgentReply = {
      schema_version: 'agent_reply.v1.1',
      speech: input.includes('天气') ? '海风正好，船长。' : '先把手上的活干完，船长。',
      dialogue_intent: input.includes('天气') ? 'inform' : 'answer',
      emotion: input.includes('天气') ? 'happy' : 'neutral',
      affinity_change: input.includes('天气') ? 1 : 0,
      relation_proposal: { kind: 'none', target_ref: 'none', stance: 'none', basis: '无关系提议' },
      negotiation_tone: 'neutral',
      refusal_reason: 'none',
      action_intent: { name: 'none', actor_ref: 'none', target_ref: 'none', parameters: {} },
    };
    return this.validateReply(reply, { enforceAuthorization: false });
  }

  validateReply(reply: unknown, context: ReplyValidationContext = {}): AgentReply {
    return validateAgentReply(reply, context);
  }
}
