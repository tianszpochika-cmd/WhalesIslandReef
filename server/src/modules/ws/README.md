# ws/ —— WebSocket 模块（待建）

职责：对话流式推送（SSE/WS）、游戏内实时通道（多人阶段扩展）。

当前**不创建源码**的原因：需要额外依赖 `@nestjs/websockets` + `socket.io`，
且对话流式方案（WS vs SSE）待 gateway 上游接入时一起定。

启用时机：需求文档定稿 → gateway 实现对话链路 → 再按需引入。
