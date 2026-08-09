import { Module } from '@nestjs/common';
import { D4Controller } from './d4.controller';
import { D4Service } from './d4.service';

@Module({
  controllers: [D4Controller],
  providers: [D4Service],
  exports: [D4Service],
})
export class D4Module {}
