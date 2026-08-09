import { Injectable } from '@nestjs/common';

const FACTIONS = ['england', 'spain', 'france', 'portugal', 'pirates', 'tribes', 'smugglers'] as const;
const WEAPONS = ['round_shot', 'chain_shot', 'grape_shot'] as const;
const BEHAVIORS = ['foraging', 'alert', 'attacking', 'breeding_hint'] as const;
const DISEASES = ['scurvy', 'malaria', 'yellow_fever', 'dysentery', 'consumption'] as const;

@Injectable()
export class D4Service {
  factionState(input: Record<string, unknown>) {
    const reputation: Record<string, number> = {};
    const raw = this.record(input.reputation);
    for (const faction of FACTIONS) reputation[faction] = this.clampInt(raw[faction], -100, 100, 0);
    return {
      reputation,
      atWar: this.stringList(input.atWar),
      embargoedPorts: this.stringList(input.embargoedPorts),
      privateerPermits: this.stringList(input.privateerPermits).filter((id) => FACTIONS.includes(id as (typeof FACTIONS)[number])),
      wantedLevel: this.clampInt(input.wantedLevel, 0, 100, 0),
    };
  }

  combatState(input: Record<string, unknown>) {
    const status = typeof input.status === 'string' ? input.status : 'idle';
    const weapon = WEAPONS.includes(input.weapon as (typeof WEAPONS)[number]) ? input.weapon : 'round_shot';
    return {
      status,
      enemy_id: this.string(input.enemy_id ?? input.enemyId),
      player_hull: this.nonNegative(input.player_hull ?? input.playerHull),
      enemy_hull: this.nonNegative(input.enemy_hull ?? input.enemyHull),
      distance: this.nonNegative(input.distance),
      loaded_count: this.nonNegative(input.loaded_count ?? input.loadedCount),
      weapon,
      enemy_crew: this.nonNegative(input.enemy_crew ?? input.enemyCrew),
      player_crew: this.nonNegative(input.player_crew ?? input.playerCrew),
    };
  }

  ecologyState(input: Record<string, unknown>) {
    const animals = Array.isArray(input.animals) ? input.animals.filter((item): item is Record<string, unknown> => this.isRecord(item)).map((animal) => ({
      animal_id: this.string(animal.animal_id ?? animal.animalId),
      behavior: BEHAVIORS.includes(animal.behavior as (typeof BEHAVIORS)[number]) ? animal.behavior : 'foraging',
      health: this.clampInt(animal.health, 0, 100, 100),
      observed: animal.observed === true,
      collection_value: this.nonNegative(animal.collection_value ?? animal.collectionValue),
    })) : [];
    return { habitat_id: this.string(input.habitat_id ?? input.habitatId), animals, collection_count: this.nonNegative(input.collection_count ?? input.collectionCount) };
  }

  healthState(input: Record<string, unknown>) {
    const crew = Array.isArray(input.crew) ? input.crew.filter((item): item is Record<string, unknown> => this.isRecord(item)).map((member) => ({
      crew_id: this.string(member.crew_id ?? member.crewId),
      health: this.clampInt(member.health, 0, 100, 100),
      fatigue: this.clampInt(member.fatigue, 0, 100, 0),
      diseases: this.stringList(member.diseases).filter((id) => DISEASES.includes(id as (typeof DISEASES)[number])),
    })) : [];
    return { crew, medical_supplies: this.nonNegative(input.medical_supplies ?? input.medicalSupplies) };
  }

  mainlineState(input: Record<string, unknown>) {
    const flags: Record<string, boolean> = {};
    const rawFlags = this.record(input.flags);
    for (const [key, value] of Object.entries(rawFlags)) flags[key] = value === true;
    return {
      chapter: this.clampInt(input.chapter, 1, 3, 1),
      mission_index: this.clampInt(input.mission_index ?? input.missionIndex, 0, 9, 0),
      flags,
      completed_missions: this.stringList(input.completed_missions ?? input.completedMissions),
    };
  }

  private string(value: unknown): string { return typeof value === 'string' ? value.trim() : ''; }

  private stringList(value: unknown): string[] {
    return Array.isArray(value) ? value.filter((item): item is string => typeof item === 'string').map((item) => item.trim()).filter(Boolean) : [];
  }

  private record(value: unknown): Record<string, unknown> { return this.isRecord(value) ? value : {}; }

  private isRecord(value: unknown): value is Record<string, unknown> { return typeof value === 'object' && value !== null && !Array.isArray(value); }

  private nonNegative(value: unknown): number { return this.clampInt(value, 0, Number.MAX_SAFE_INTEGER, 0); }

  private clampInt(value: unknown, min: number, max: number, fallback: number): number {
    return typeof value === 'number' && Number.isFinite(value) ? Math.max(min, Math.min(max, Math.trunc(value))) : fallback;
  }
}
