# ADR 0002: Player ステート駆動設計

- **Date**: 2026-05-15
- **Status**: Accepted
- **Related Spec**: .github/Specs/Impact_Sports_CLASH_GDD_v1.md の Player 制御関連
- **Related Plan**: .github/Planning/Player_State_Architecture_Plan.md

## Context (背景と課題)
- 従来のPlayer制御では、`PlayerController` に入力・移動・攻撃・状態管理などの複数責務が集中しやすかった。
- 今後（Shoot, Dash, Skillなどの）アクション追加を想定した場合、単一クラスへのロジック集中は「状態依存のバグ増加」「テストの困難化」「保守コストの増大」を招く懸念があった。

## Decision (決定事項)
- **State Pattern の導入**: 状態駆動型アーキテクチャ（Player State Driven Architecture）を採用し、責務を以下の層に分離する。
  - **State層 (`PlayerState`)**: 状態ごとのロジック（`Enter`, `Execute`, `FixedExecute`, `Exit`）を独立管理（例: `PlayerIdleState`, `PlayerShootState`）。
  - **InputSystemの統合**: `PlayerState` のコンストラクタで `InputActionId` (Enum) を用いて `InputAction` の参照をキャッシュ。各状態が直接入力（`triggered` や `ReadValue`）をポーリングできるPull型の入力監視設計を採用。
  - **Context層 (`IPlayerContext` / `PlayerContext`)**: コンポーネント参照（Input, Collider, Transform）や状態管理を集約。状態遷移時のGC回避のため、インスタンスのキャッシュ（`_stateCache`）とカスタムファクトリ機能（`RegisterStateFactory<T>`）を実装。
  - **Settings層 (`SO_PlayerSettings`)**: 移動速度やクールタイム等のパラメータを ScriptableObject に切り出し、コード修正なしで調整可能にする。
  - **Coordinator (`PlayerController`)**: 実際のゲームロジックは持たず、Stateの更新管理と初期化のみを担当する。

## Consequences (結果と影響)
- **メリット**: 
  - **保守性と拡張性の向上**: 状態ごとにクラスが分離されたため、新規アクション（Dash, Damage等）の追加が安全かつ容易になった。
  - **入力処理の局所化（Pull型）**: `PlayerController` が入力を毎フレーム受け取って各状態に渡すのではなく、State自身が自律的に入力を取得するため、アクション追加時にControllerを改修する必要がなくなった。
  - **テスト容易性**: ロジックがState単位に分割されたことで、依存をモック化したユニットテストが書きやすくなった。
  - **調整の効率化**: ScriptableObject化により、プランナー等でもゲームバランスの調整が可能になった。
- **トレードオフ**: 
  - **ファイル・クラス数の増加**: 状態が増えるごとにスクリプトファイルが増加し、小規模な変更でも全体構造の把握（学習コスト）が必要になる。
  - **Contextの肥大化リスク**: 参照管理を怠ると、あらゆる情報が `PlayerContext` に集まり「神オブジェクト」化する恐れがある。

## Learnings (学習メモ)

### 1. Input System とは？
Unityの新しい入力管理パッケージ。従来の `Input.GetKeyDown()` のようなハードコードから脱却し、**「物理的なボタン（キーボードやパッド）」と「ゲーム内のアクション（JumpやShoot）」を切り離して管理**できるのが最大の特徴。
これにより、マルチプラットフォーム対応やキーコンフィグの実装が格段に容易になる。

### 2. PlayerInput コンポーネントとは？
Input System を手軽に扱うための統合コンポーネント。
- **主な役割**: 作成した入力設定ファイル（`.inputactions`）を読み込み、現在接続されているデバイスや、操作スキーム（マウス⇔パッド）の切り替えを**自動で管理**してくれる。
- **Behavior (通知方式)**: ボタンが押された際、通常はUnityEvent等でスクリプトに通知（Push型）を行う設定ができる。
- **本プロジェクトでの使い方**: 今回のState設計では通知機能は使わず、各Stateが直接 `PlayerInput` にアクセスして「今ボタンが押されているか（`triggered`）」を抜き取る（Pull型）運用をしている。これにより、アクションが増えてもControllerが肥大化しない。

## References (参考資料)
- （任意）関連ドキュメントや参考記事のURLを列挙してください。
