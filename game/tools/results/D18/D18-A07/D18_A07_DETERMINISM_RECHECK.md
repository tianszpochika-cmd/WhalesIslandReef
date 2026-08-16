# D18-A07 Determinism Recheck

- 日期：2026-08-14
- 命令：`python tools/d18_placeholder_replacement_map.py --project-root . --output-map docs/game/design/common/assets/replacement/d18_placeholder_replacement_map.json --result-dir game/tools/results/D18/D18-A07`
- 第一次 digest：`e908eef063e1019fa19e273cb5234c955913dd781855277408821ac2b5d68c75`
- 第二次 digest：`e908eef063e1019fa19e273cb5234c955913dd781855277408821ac2b5d68c75`
- 输入/输出：2,264 / 2,264
- mapped/unmapped：1,107 / 1,157
- 结论：`stable=True`

两次运行均只读 Registry、候选 manifest 和 Unity 文本消费者；没有改写 Registry、`asset_state`、Prefab、场景或正式资源。

最终修订重跑（包含玩家 P0 控制点）：digest `e908eef063e1019fa19e273cb5234c955913dd781855277408821ac2b5d68c75`，`stable=True`。
