import { Injectable } from '@nestjs/common';

const ISLANDS = ['santa_margarita', 'green_cay', 'driftwood_isle', 'port_royal', 'havana', 'tortuga', 'martinique', 'madeira_roadstead', 'mangrove_key', 'treasure_cay', 'crown_fort', 'jaguar_isle'] as const;
const TRAITS = ['strong', 'weak', 'lung_disease', 'hardy_stomach', 'presbyopia', 'injured_leg', 'keen_eyes', 'heat_resistant', 'cold_sensitive', 'nimble', 'broad_shoulders', 'seasick', 'night_vision', 'scarred', 'frail', 'greedy', 'cowardly', 'loyal', 'alcoholic', 'suspicious', 'impulsive', 'cautious', 'generous', 'vengeful', 'optimistic', 'superstitious', 'disciplined', 'proud', 'patient', 'hot_tempered', 'curious', 'thrifty', 'ambitious', 'compassionate', 'fatalistic', 'old_sailor', 'former_privateer', 'fugitive', 'missionary', 'ex_navy', 'fallen_noble', 'doctor_apprentice', 'island_guide', 'dock_runner', 'cartographer', 'scurvy', 'malaria', 'yellow_fever', 'dysentery', 'consumption'] as const;
const PRESETS = ['dockhand_male', 'navigator_female', 'privateer', 'merchant', 'island_scout', 'navy_officer', 'old_sailor', 'scholar'] as const;

@Injectable()
export class D5Service {
  worldState(input: Record<string, unknown>) {
    return {
      currentIslandId: this.validId(input.currentIslandId, ISLANDS, 'santa_margarita'),
      discoveredIslands: this.stringList(input.discoveredIslands).filter((id) => ISLANDS.includes(id as (typeof ISLANDS)[number])),
      visitedIslands: this.stringList(input.visitedIslands).filter((id) => ISLANDS.includes(id as (typeof ISLANDS)[number])),
      mapRevision: this.nonNegative(input.mapRevision),
    };
  }

  traitState(input: Record<string, unknown>) {
    return {
      playerTraitIds: this.stringList(input.playerTraitIds).filter((id) => TRAITS.includes(id as (typeof TRAITS)[number])).slice(0, 4),
      crewTraitIds: this.recordOfStringLists(input.crewTraitIds),
    };
  }

  customizationState(input: Record<string, unknown>) {
    return {
      presetId: this.validId(input.presetId, PRESETS, 'dockhand_male'),
      skinTone: this.string(input.skinTone),
      hairStyle: this.string(input.hairStyle),
      hairColor: this.string(input.hairColor),
      outfit: this.string(input.outfit),
      faceScale: this.clampNumber(input.faceScale, 0.85, 1.15, 1),
      bodyScale: this.clampNumber(input.bodyScale, 0.85, 1.15, 1),
    };
  }

  performanceState(input: Record<string, unknown>) {
    return {
      quality: ['Low', 'Medium', 'High'].includes(String(input.quality)) ? input.quality : 'Medium',
      targetWidth: 1920,
      targetHeight: 1080,
      targetFps: 60,
      lastFps: this.nonNegativeNumber(input.lastFps),
      minFps: this.nonNegativeNumber(input.minFps),
      averageFps: this.nonNegativeNumber(input.averageFps),
      sampleCount: this.nonNegative(input.sampleCount),
      meetsTarget: input.meetsTarget === true && this.nonNegativeNumber(input.minFps) >= 60,
    };
  }

  cloudState(input: Record<string, unknown>) {
    return {
      appId: this.string(input.appId) || 'UNCONFIGURED',
      offlineMode: input.offlineMode !== false,
      cloudEnabled: input.cloudEnabled !== false,
      pendingCloudRevision: this.nonNegative(input.pendingCloudRevision),
      lastSync: this.string(input.lastSync) || 'never',
      unlockedAchievements: this.stringList(input.unlockedAchievements),
    };
  }

  private validId(value: unknown, allowed: readonly string[], fallback: string): string {
    const id = this.string(value);
    return allowed.includes(id) ? id : fallback;
  }

  private string(value: unknown): string { return typeof value === 'string' ? value.trim() : ''; }

  private stringList(value: unknown): string[] {
    return Array.isArray(value) ? value.filter((item): item is string => typeof item === 'string').map((item) => item.trim()).filter(Boolean) : [];
  }

  private recordOfStringLists(value: unknown): Record<string, string[]> {
    if (typeof value !== 'object' || value === null || Array.isArray(value)) return {};
    const result: Record<string, string[]> = {};
    for (const [key, values] of Object.entries(value)) result[key] = this.stringList(values).filter((id) => TRAITS.includes(id as (typeof TRAITS)[number])).slice(0, 4);
    return result;
  }

  private nonNegative(value: unknown): number { return this.clampInt(value, 0, Number.MAX_SAFE_INTEGER, 0); }

  private nonNegativeNumber(value: unknown): number { return this.clampNumber(value, 0, Number.MAX_VALUE, 0); }

  private clampInt(value: unknown, min: number, max: number, fallback: number): number {
    return typeof value === 'number' && Number.isFinite(value) ? Math.max(min, Math.min(max, Math.trunc(value))) : fallback;
  }

  private clampNumber(value: unknown, min: number, max: number, fallback: number): number {
    return typeof value === 'number' && Number.isFinite(value) ? Math.max(min, Math.min(max, value)) : fallback;
  }
}
