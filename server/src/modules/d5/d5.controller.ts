import { BadRequestException, Body, Controller, HttpCode, Post } from '@nestjs/common';
import { D5Service } from './d5.service';

@Controller()
export class D5Controller {
  constructor(private readonly d5: D5Service) {}

  @Post('world/islands')
  @HttpCode(200)
  world(@Body() body: Record<string, unknown>) {
    if (!body || typeof body !== 'object') throw new BadRequestException('world state is required');
    return this.d5.worldState(body);
  }

  @Post('profile/traits')
  @HttpCode(200)
  traits(@Body() body: Record<string, unknown>) {
    if (!body || typeof body !== 'object') throw new BadRequestException('trait state is required');
    return this.d5.traitState(body);
  }

  @Post('profile/customization')
  @HttpCode(200)
  customization(@Body() body: Record<string, unknown>) {
    if (!body || typeof body !== 'object') throw new BadRequestException('customization state is required');
    return this.d5.customizationState(body);
  }

  @Post('performance/report')
  @HttpCode(200)
  performance(@Body() body: Record<string, unknown>) {
    if (!body || typeof body !== 'object') throw new BadRequestException('performance state is required');
    return this.d5.performanceState(body);
  }

  @Post('steam/cloud')
  @HttpCode(200)
  cloud(@Body() body: Record<string, unknown>) {
    if (!body || typeof body !== 'object') throw new BadRequestException('steam state is required');
    return this.d5.cloudState(body);
  }
}
