import { Module } from '@nestjs/common';
import { D5Controller } from './d5.controller';
import { D5Service } from './d5.service';

@Module({ controllers: [D5Controller], providers: [D5Service] })
export class D5Module {}
