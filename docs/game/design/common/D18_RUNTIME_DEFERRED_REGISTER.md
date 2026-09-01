# D18 Runtime Deferred Register

> 这是 D18 逻辑阶段的延期登记，不是资源导入、Prefab、场景、Player 或发布批准。
> 详细历史依据保留在 [D18_BINARY_AND_IMPORT_DEFERRAL.md](D18_BINARY_AND_IMPORT_DEFERRAL.md)。

## Register metadata

- Register owner: D18 logic governance
- Source task: `D18-L48` and the `D18-L53-C02` follow-up review
- State: `RUNTIME_DEFERRED`
- Logic consequence: static provenance/catalog evidence may be audited; no entry is promoted to `RuntimeApproved` or `ReleaseApproved` here.

## Deferred items

The following require a later Runtime-owned evidence pass and are deliberately not claimed by D18 logic work:

- real model/mesh import, scale, axis, bounds, collision and LOD verification;
- texture/material color space, compression, mipmap and PBR verification;
- skeleton/avatar/animation extraction and root-motion verification;
- Canvas/UI/font rendering and audio mixer/playback verification;
- save/restore, input route, screenshots, human visual acceptance and device checks;
- FPS, 1% low, load time, memory/VRAM, quality-tier and release-package checks;
- final per-file hash confirmation for any binary selected for Runtime consumption.

## Entry rule

An item remains deferred unless a later task supplies the corresponding real Runtime evidence. `Candidate`, `IntakeEligible` and static source/provenance records do not satisfy this register and must not be rewritten as runtime availability.
