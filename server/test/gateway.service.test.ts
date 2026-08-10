import assert from 'node:assert/strict';
import { test } from 'node:test';
import { GatewayService, P2A_ACTIONS, validateAgentReply, type AgentReply, type ReplyValidationContext } from '../src/modules/gateway/gateway.service.js';

function baseReply(): AgentReply {
  return {
    schema_version: 'agent_reply.v1.1',
    speech: '收帆。',
    dialogue_intent: 'suggest',
    emotion: 'determined',
    affinity_change: 0,
    relation_proposal: { kind: 'none', target_ref: 'none', stance: 'none', basis: '无关系提议' },
    negotiation_tone: 'firm',
    refusal_reason: 'none',
    action_intent: { name: 'none', actor_ref: 'none', target_ref: 'none', parameters: {} },
  };
}

function trimSailReply(): AgentReply {
  const reply = baseReply();
  reply.action_intent = {
    name: 'trim_sail',
    actor_ref: 'crew:deckhand-1',
    target_ref: 'ship:active',
    parameters: { sail: 'main', setting: 'full' },
  };
  return reply;
}

function authorizedContext(): ReplyValidationContext {
  return {
    authorizedActorRefs: new Set(['crew:deckhand-1']),
    authorizedTargetRefs: new Set(['ship:active']),
    permissions: new Set(['ship.sail']),
    satisfiedPrerequisites: new Set(['ship.present', 'crew.on_duty']),
  };
}

test('gateway returns a complete v1.1 AgentReply', async () => {
  const reply = await new GatewayService().chat({ crewId: 'crew-1', playerInput: '天气如何' });
  assert.equal(reply.schema_version, 'agent_reply.v1.1');
  assert.equal(reply.action_intent.name, 'none');
  assert.equal(P2A_ACTIONS.size, 60);
  assert.deepEqual(validateAgentReply(reply, { enforceAuthorization: false }), reply);
});

test('gateway rejects illegal fields and out-of-range values', () => {
  const extra = baseReply() as AgentReply & Record<string, unknown>;
  extra.money = 10;
  assert.throws(() => validateAgentReply(extra), /invalid_agent_reply_fields/);

  const outOfRange = baseReply();
  outOfRange.affinity_change = 11;
  assert.throws(() => validateAgentReply(outOfRange), /invalid_agent_reply_affinity/);
});

test('gateway rejects unknown actions, invalid parameters and forbidden results', () => {
  const unknown = baseReply();
  unknown.action_intent = { name: 'hack_world_state', actor_ref: 'crew:deckhand-1', target_ref: 'ship:active', parameters: {} };
  assert.throws(() => validateAgentReply(unknown, { enforceAuthorization: false }), /unknown_action/);

  const invalidParameter = trimSailReply();
  invalidParameter.action_intent.parameters.setting = 'impossible';
  assert.throws(() => validateAgentReply(invalidParameter, { enforceAuthorization: false }), /invalid_action_parameters/);

  const forbidden = trimSailReply();
  forbidden.action_intent.parameters.weather = 'storm';
  assert.throws(() => validateAgentReply(forbidden, { enforceAuthorization: false }), /forbidden_result_field/);
});

test('gateway rejects unauthorized actors, targets, permissions and prerequisites', () => {
  const reply = trimSailReply();
  assert.doesNotThrow(() => validateAgentReply(reply, authorizedContext()));

  const unauthorizedActor = authorizedContext();
  unauthorizedActor.authorizedActorRefs = new Set(['crew:other']);
  assert.throws(() => validateAgentReply(reply, unauthorizedActor), /unauthorized_actor/);

  const unauthorizedTarget = authorizedContext();
  unauthorizedTarget.authorizedTargetRefs = new Set(['ship:other']);
  assert.throws(() => validateAgentReply(reply, unauthorizedTarget), /unauthorized_target/);

  const noPermission = authorizedContext();
  noPermission.permissions = new Set();
  assert.throws(() => validateAgentReply(reply, noPermission), /permission_denied/);

  const missingPrerequisite = authorizedContext();
  missingPrerequisite.satisfiedPrerequisites = new Set(['ship.present']);
  assert.throws(() => validateAgentReply(reply, missingPrerequisite), /missing_prerequisite/);
});

test('gateway rejects relation targets outside the authorized snapshot', () => {
  const reply = baseReply();
  reply.relation_proposal = { kind: 'trust', target_ref: 'crew:other', stance: 'build', basis: '请求合作' };
  assert.throws(() => validateAgentReply(reply, { authorizedTargetRefs: new Set(['crew:known']) }), /unauthorized_target/);
});
