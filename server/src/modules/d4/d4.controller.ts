import { BadRequestException, Body, Controller, HttpCode, Post } from '@nestjs/common';
import { D4Service } from './d4.service';

@Controller()
export class D4Controller {
  constructor(private readonly d4: D4Service) {}

  @Post('factions/state')
  @HttpCode(200)
  factionState(@Body() body: Record<string, unknown>) {
    if (!body || typeof body !== 'object') throw new BadRequestException('faction state is required');
    return this.d4.factionState(body);
  }

  @Post('combat/resolve')
  @HttpCode(200)
  combat(@Body() body: Record<string, unknown>) {
    if (!body || typeof body !== 'object') throw new BadRequestException('combat state is required');
    return this.d4.combatState(body);
  }

  @Post('animals/encounter')
  @HttpCode(200)
  animals(@Body() body: Record<string, unknown>) {
    if (!body || typeof body !== 'object') throw new BadRequestException('ecology state is required');
    return this.d4.ecologyState(body);
  }

  @Post('medical/treat')
  @HttpCode(200)
  medical(@Body() body: Record<string, unknown>) {
    if (!body || typeof body !== 'object') throw new BadRequestException('health state is required');
    return this.d4.healthState(body);
  }

  @Post('mainline/advance')
  @HttpCode(200)
  mainline(@Body() body: Record<string, unknown>) {
    if (!body || typeof body !== 'object') throw new BadRequestException('mainline state is required');
    return this.d4.mainlineState(body);
  }
}
