# ADR 0002: Player ステート駆動設計

- **Date**: 2026-05-15 (更新: 2026-05-21)
- **Status**: Accepted
- **Related Spec**: .github/Specs/Impact_Sports_CLASH_GDD_v1.md の Player 制御関連
- **Related Plan**: .github/Planning/Player_State_Architecture_Plan.md

## 概要
Playerの振る舞いを状態ごとに分離することで可読性・拡張性・テスト性を向上させる。既存の `PlayerController` の責務分散を目的とし、以下の役割分担を採用する。

## Decision (決定事項)
- State層 (`PlayerState`)
  - 各状態は `Enter() / Execute() / FixedExecute() / Exit()` を持つ。例: `PlayerIdleState`, `PlayerMoveState`, `PlayerShootState`。
  - 各Stateは必要な入力を自分でポーリングする（Pull型）。

- Context層 (`IPlayerContext` / `PlayerContext`)
  - `PlayerInput` / `CapsuleCollider` / `Rigidbody` / `Transform` / `SO_PlayerSettings` 等の参照を集約して提供する。外部から直接多数の値を参照させず、必要最小限のAPIを公開する。
  - 状態インスタンスはキャッシュして再利用する（`_stateCache`）ことでGC発生を抑える。
  - テストやプール運用のために、状態インスタンス生成を差し替え可能にする `RegisterStateFactory<T>(Func<IPlayerContext, PlayerState>)` を提供する。

- Settings層 (`SO_PlayerSettings`)
  - 移動速度や最大速度、摩擦、加速度、射撃の力やクールダウン等のパラメータを `ScriptableObject` にまとめることで、実装側のハードコーディングを回避し調整を容易にする。

- Coordinator (`PlayerController`)
  - 必須コンポーネントの検証、`PlayerContext` の初期化、および現在の状態の `Execute()` / `FixedExecute()` を毎フレーム呼び出す役割に限定する。

- 入力定義
  - `InputActionId` (enum) と対応するアクション名を定義し、状態が `PlayerInput` から必要な `InputAction` を取得して使用する。
  - 入力デッドゾーンや停止判定などの閾値は `PlayerConfig` に定義する（例: `INPUT_DEADZONE`, `STOP_VELOCITY_THRESHOLD`）。

## Implementation notes（実装の要点）
## Implementation notes（実装の要点）

- `PlayerController`（Coordinator）
  - Awakeで必須コンポーネント（`PlayerInput`/`CapsuleCollider`/`Rigidbody`/`releasePoint`/`holdPoint`/`SO_PlayerSettings`）の検証を行い、不足時は無効化してログを出す。
  - `PlayerContext` は `releasePoint.transform` と `holdPoint.transform` を渡し、`PlayerBallHolder` を含むコンテナを初期化する。
  - Startで `PlayerBallHolder.InitializeBalls(settings.InitialBallCount)` を呼び、初期所持ボールを生成する。
  - OnCollisionEnterでボールタグを検出したら `BallHolder.TryAddBall()` を試行し、成功時は該当ボールを破壊して所持数を増やす。

- `PlayerState`（共通基底）
  - コンストラクタで `PlayerInput` から `InputAction` を取得してキャッシュする（`Shoot`/`Move`/`Dodge` など）。
  - 共通ヘルパーとして `IsShotTriggered()`, `GetMoveInput()`, `IsDodgeTriggered()` を提供し、各Stateはそれを利用して遷移判定を行う。

- `PlayerIdleState` / `PlayerMoveState` / `PlayerShootState` / `PlayerDodgeState` の挙動
  - `PlayerIdleState`
    - `Enter()` で `Rigidbody.linearVelocity` を零にし、`Execute()` で射撃・回避・移動入力を監視して該当Stateへ遷移する。

  - `PlayerMoveState`
    - `Execute()` で射撃入力かつ `BallHolder.CanShoot` の場合は `PlayerShootState` へ遷移。回避入力なら `PlayerDodgeState` へ遷移。
    - 停止判定（`Rigidbody.linearVelocity.magnitude < PlayerConfig.STOP_VELOCITY_THRESHOLD`）で `PlayerIdleState` へ遷移。
    - `FixedExecute()` で `GetMoveInput()` をワールド方向（カメラ基準の補正はTODO）に変換し、摩擦（`GrandFriction`）、加速度（`Acceleration`）、最大速度（`MaxMoveSpeed`）を計算して `Rigidbody.linearVelocity` を設定する。

  - `PlayerShootState`
    - `Enter()` で `BallHolder.TryConsumeBall()` を呼び、所持がなければ警告を出して終了する。
    - 発射は `Instantiate(settings.BallPrefab, Context.releasePointTransform.position, Quaternion.identity)` で行い、生成直後にプレイヤー本体との衝突を `Physics.IgnoreCollision` で無視する。
    - 発射時は `rb.AddForce(Camera.main.transform.forward * settings.ThrowForce, ForceMode.Impulse)` を使う（要オブジェクトプール検討）。
    - `Execute()` で射出後の遷移を扱い、入力があれば `PlayerMoveState`、無ければ `PlayerIdleState` に戻る。

  - `PlayerDodgeState`
    - `Enter()` で回避方向を `ResolveDodgeDirection()` で決定し `_timer` をリセット、`ApplyDodgeVelocity()` で速度を設定する。
    - `Execute()` で `Context.Settings.DodgeDuration` 超過を監視し、終了時に移動入力があれば `PlayerMoveState`、無ければ `PlayerIdleState` に遷移する。
    - `FixedExecute()` でも `ApplyDodgeVelocity()` を呼び続け、`Rigidbody.linearVelocity` のX/Zを回避速度（`DodgeSpeed`）で上書きする。

- `PlayerContext` の注意点
  - `BallHolder`（`PlayerBallHolder`）を持ち、`CanShoot/TryConsumeBall/TryAddBall` 等のボール管理APIを通して射撃ロジックと分離する。
  - 状態生成はファクトリ登録を優先し、未登録時はリフレクションでインスタンス化する。生成済み状態は `_stateCache` で保持して再利用する。
  - `TransitionTo<T>()` は現在状態の `Exit()` → 新状態の `Enter()` を順に呼ぶ。

- `SO_PlayerSettings` の主要フィールド（参照実装）
  - Movement: `MoveSpeed`, `MaxMoveSpeed`, `GrandFriction`, `Acceleration`
  - Dodge: `DodgeSpeed`, `DodgeDuration`
  - Shooting: `ThrowForce`, `CooldownTime`, `BallPrefab`
  - Ball Hold: `MaxHoldCount`, `InitialBallCount`


## Consequences (結果と影響)
- 利点
  - 境界が明確になり新機能（Dash, Skill, Damage 等）の追加が容易。
  - State単位でのユニットテストが書きやすくなる。
  - `SO_PlayerSettings` によるゲームデザイナー側での調整が可能。

- トレードオフ / リスク
  - 状態数の増加に伴いファイル・クラス数が増える。
  - `PlayerContext` に責務を詰め込みすぎると肥大化するリスクがあるため、公開APIは最小限に留める。

## Learnings（学習メモ）
- Input System
  - Unityの新しい入力フレームワーク。物理デバイス（キーボード/GamePad）とゲーム内アクションを分離し、マルチデバイス・キーコンフィグ対応を容易にする。
  - Pull型（Stateが `PlayerInput` をポーリング）とPush型（イベント通知）の違いを理解して使い分ける。状態ロジックを局所化したい本設計ではPull型が有利。

- PlayerInput
  - `PlayerInput` コンポーネントはアクションマップと接続デバイス、操作スキームを管理する。Stateから直接 `triggered` や `ReadValue<T>()` を使う運用が本設計と親和性が高い。
  - 複数スキームや未割当アクションの扱いに注意し、デフォルトのアクションマップ名（`InputActionId`）と一致させること。

- Rigidbody（物理制御）
  - 物理演算は `FixedUpdate` で扱う。`Rigidbody.linearVelocity` を直接書き換える場合はY軸（重力やジャンプ）を適切に維持すること。
  - 力（AddForce）と速度の直接設定を混用すると挙動が不安定になる場合があるため、用途を明確に分ける。
  - 摩擦や滑らかな減速は `Vector3.Lerp` 等を使って固定更新で補間することで安定する。

- State Machine 設計上の注意
  - 各Stateは `Enter/Execute/FixedExecute/Exit` の責務に限定し、副作用はContext経由で最小限にする。
  - 状態インスタンスをキャッシュして再利用することでGC発生を抑え、`RegisterStateFactory` による差し替えでテスト・オブジェクトプールに対応可能。
  - `PlayerContext` の公開APIは必要最小限に留め、肥大化（God Object化）を避ける。
  - 遷移条件は明確かつ単純に保ち、副次的な状態遷移のループや競合を避けるために優先順位を設計しておく。

- 実運用の学び
  - 小さなStateから実装を始め、共通処理はContextやユーティリティに抽出することで保守性が向上する。
  - 設定値は `SO_PlayerSettings` に集約し、プレイフィール調整をコード変更なしで行えるようにする。

## References (参考実装)
- Constant / InputActionId, PlayerConfig:
  - https://github.com/R-production004682/Impact_Sports_CLASH/blob/14a198e5d97f862c3149b33b8ff5825daa65017a/Impact_Sports_CLASH/Assets/Script/Constant/Constant.cs
- PlayerContext:
  - https://github.com/R-production004682/Impact_Sports_CLASH/blob/14a198e5d97f862c3149b33b8ff5825daa65017a/Impact_Sports_CLASH/Assets/Script/Player/Context/PlayerContext.cs
- SO_PlayerSettings:
  - https://github.com/R-production004682/Impact_Sports_CLASH/blob/14a198e5d97f862c3149b33b8ff5825daa65017a/Impact_Sports_CLASH/Assets/Script/Player/SO/SO_PlayerSettings.cs
- PlayerIdleState / PlayerMoveState:
  - https://github.com/R-production004682/Impact_Sports_CLASH/blob/14a198e5d97f862c3149b33b8ff5825daa65017a/Impact_Sports_CLASH/Assets/Script/Player/State/PlayerIdleState.cs
  - https://github.com/R-production004682/Impact_Sports_CLASH/blob/14a198e5d97f862c3149b33b8ff5825daa65017a/Impact_Sports_CLASH/Assets/Script/Player/State/PlayerMoveState.cs
- PlayerController (Coordinator):
  - https://github.com/R-production004682/Impact_Sports_CLASH/blob/14a198e5d97f862c3149b33b8ff5825daa65017a/Impact_Sports_CLASH/Assets/Script/Player/PlayerController.cs

