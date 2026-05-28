# ADR 0003: キャッチ判定における当たり判定方式の選定

- **Date**: 2026-05-28
- **Status**: Accepted
- **Related Spec**: `.github/Specs/Impact_Sports_CLASH_GDD_v1.md` の「ボールキャッチ操作」
- **Related Plan**: なし

## Context (背景と課題)
- キャッチ判定を「ボール接触による自動キャッチ」から「移動入力なし + Dodge入力 = キャッチアクション」に改修するにあたり、キャッチウィンドウ中にプレイヤー前方のボールを検出する仕組みが必要になった。
- 候補として「子オブジェクトに SphereCollider (Trigger) を配置して ON/OFF する方式」と「`Physics.OverlapSphere` を毎物理フレーム呼び出す方式」の2つが挙がった。
- キャッチウィンドウが 0.2〜0.3 秒と短く、ボールの飛来速度も速いため、判定の確実性が最重要要件となる。

## Decision (決定事項)
- **`Physics.OverlapSphereNonAlloc` を採用する。**
- キャッチ専用ステート (`PlayerCatchState`) の `FixedExecute()` 内で毎物理フレーム呼び出し、プレイヤー前方の球形範囲に `"Ball"` タグのコライダーが存在するか検査する。
- `static Collider[]` バッファを再利用することで GC アロケーションをゼロにする。
- SphereCollider (Trigger) 方式は不採用とする。

### 不採用とした理由：SphereCollider (Trigger) 方式

1. **有効化タイミングの問題**
   - コライダーを `enabled = true` にした瞬間、既にボールが重なっていた場合 `OnTriggerEnter` は発火しない。
   - `OnTriggerStay` で補えるが、次の物理ステップ（最大 `fixedDeltaTime` = 0.02秒）まで検出が遅延する。
   - キャッチウィンドウが 0.25 秒程度の設計において、1物理フレームの遅延は無視できない。

2. **責務の分散**
   - トリガー検出用のスクリプト（子オブジェクト）と CatchState の間でフラグ共有やイベント通知が必要になり、責務が分散する。
   - OverlapSphere であればステート内で判定と結果処理が完結し、既存のステートマシンパターンと自然に統合できる。

3. **追加 GameObject の管理コスト**
   - SphereCollider 方式では子オブジェクト＋専用 MonoBehaviour が必要。
   - OverlapSphere は追加の GameObject が不要。

### エディタ上の可視化
- SphereCollider 方式はコライダーのワイヤーフレームが自動表示される点で有利だが、OverlapSphere でも `OnDrawGizmosSelected` で同等の可視化を数行で実現できるため、差は軽微と判断した。

## Consequences (結果と影響)

- **メリット**:
  - 呼び出したフレームで即座に結果が返るため、短いキャッチウィンドウでも確実にボールを検出できる。
  - ステート内で判定が完結し、既存のステートマシン設計（ADR 0002）との整合性が高い。
  - `NonAlloc` 版により GC ゼロで運用できる。
  - 将来的に判定形状を変更したい場合も `OverlapCapsule` / `OverlapBox` への差し替えが容易。

- **トレードオフ**:
  - Scene ビューでの自動表示がないため、可視化には `OnDrawGizmosSelected` の実装が必要（軽微）。
  - `static Collider[]` バッファのサイズ（現在8）を超える同時検出が発生した場合、超過分は無視される。実用上問題ないが、仕様変更時に見直しが必要。

## Learnings (学習メモ)

- `Physics.OverlapSphere` vs `Physics.OverlapSphereNonAlloc`
  - 前者は `Collider[]` を毎回新規アロケーションして返すため GC が発生する。後者は事前確保したバッファに書き込むためゼロアロケーション。頻繁に呼ぶ場合は `NonAlloc` 一択。

- SphereCollider (Trigger) の `OnTriggerEnter` 発火条件
  - **コライダーが有効化された時点で既に重なっている物体に対しては `OnTriggerEnter` は発火しない。** これは Unity の物理エンジンの仕様であり、`OnTriggerStay` は次の物理ステップ以降に呼ばれる。
  - 「一瞬だけ判定を有効にする」ユースケースでは、この1フレーム遅延が致命的になりうる。

- レイヤーマスクによるフィルタリング
  - `OverlapSphere` は第3引数で `LayerMask` を指定でき、不要なコライダーを物理エンジンレベルで除外できる。現時点では `"Ball"` タグによるフィルタで十分だが、オブジェクト数が増えた場合はレイヤーマスク併用を検討する。

## References (参考資料)
- Unity公式: [Physics.OverlapSphereNonAlloc](https://docs.unity3d.com/ScriptReference/Physics.OverlapSphereNonAlloc.html)
- Unity公式: [Collider.OnTriggerEnter](https://docs.unity3d.com/ScriptReference/Collider.OnTriggerEnter.html)
- 本プロジェクト ADR 0002: Player ステート駆動設計
