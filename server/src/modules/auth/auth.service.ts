import { Injectable } from '@nestjs/common';

@Injectable()
export class AuthService {
  // 占位：Steam OpenID 关联登录 / 邮箱注册 将在 Steamworks 账号就绪、
  // DB schema（users/steam_links/sessions）落地后实现。
  async steamLogin(authTicket?: string) {
    return { status: 'not_implemented', hint: '待 Steamworks 回调与 DB 迁移' };
  }

  async emailLogin(body: { email?: string; password?: string }) {
    return { status: 'not_implemented', hint: '待 DB 迁移与 JWT 签发' };
  }
}
