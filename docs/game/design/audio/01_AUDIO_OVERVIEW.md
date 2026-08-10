# 音频系统总览

## 1. 音频分类

- 环境底噪；
- 船舶；
- 角色动作；
- NPC；
- 战斗；
- 天气；
- UI；
- 交互；
- 音乐；
- 语音；
- 低资源降级音。

## 2. 音频事件字段

- audio_event_id；
- category；
- habitat；
- state；
- intensity；
- distance；
- directionality；
- loop；
- priority；
- mixer_bus；
- subtitle_key；
- fallback；
- license；
- source_notes。

## 3. 混音

环境、音乐、船舶、战斗、对白和 UI 分独立总线。关键警报不能被风暴、炮声或音乐完全盖住。

## 4. 资产授权

每个外部音频记录作者、许可证、购买或下载日期、原始链接、署名要求和修改权限。
