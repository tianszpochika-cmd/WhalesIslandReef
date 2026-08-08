import { Controller, Post, Body, HttpCode } from '@nestjs/common';
import { AuthService } from './auth.service';

@Controller('auth')
export class AuthController {
  constructor(private readonly auth: AuthService) {}

  // POST /api/v1/auth/steam —— Steam 关联登录（主账号）
  @Post('steam')
  @HttpCode(200)
  steamLogin(@Body() body: { auth_ticket?: string }) {
    return this.auth.steamLogin(body.auth_ticket);
  }

  // POST /api/v1/auth/email —— 邮箱注册/登录（官网用户）
  @Post('email')
  @HttpCode(200)
  emailLogin(@Body() body: { email?: string; password?: string }) {
    return this.auth.emailLogin(body);
  }
}
