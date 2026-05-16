# ADR 0001: Player ステート駆動設計

- **Date**: 2026-05-15
- **Status**: Accepted
- **Related Spec**: .github/Specs/Impact_Sports_CLASH_GDD_v1.md の Player 制御関連
- **Related Plan**: .github/Planning/Player_State_Architecture_Plan.md

## Context (背景と課題)

従来のPlayer制御では、PlayerControllerへ入力処理・移動処理・攻撃処理・状態管理・各種パラメータ管理など、
複数責務が集中する構成になりやすかった。

この構成では、機能追加やゲームプレイ拡張に伴い、
以下の問題が発生するリスクが高かった。

- PlayerControllerの肥大化
- 状態遷移ロジックの複雑化
- 状態依存バグの増加
- アクション追加時の修正影響範囲拡大
- テスト容易性の低下
- パラメータ調整時のコード依存
- 将来的なオンライン同期やAI制御対応の困難化

特に、
今後以下のようなAction追加を想定した場合、
単一Controllerへのロジック集中では保守コスト増大が懸念された。

- Shoot
- Dash
- Damage
- KnockBack
- Skill
- Charge
- Ultimate Action

そのため、
Player制御を「状態駆動型アーキテクチャ」へ移行する必要があった。

---

## Decision (決定事項)

Player制御基盤として、
State Pattern をベースとした Player State Driven Architecture を導入する。

また、
責務分離を目的として、
State / Context / Settings / Utility 層を独立構成として整理する。

---

### 1. PlayerState基底クラスの導入

`PlayerState.cs` をPlayer状態管理の抽象基底クラスとして定義する。

各Stateは以下ライフサイクルを持つ。

- Enter
- Update
- Exit

これにより、
状態単位でロジックを独立管理可能にする。

---

### 2. Stateクラスの分離

以下Stateを独立クラスとして実装する。

- `PlayerIdleState`
- `PlayerShootState`

各Stateは自身の責務のみを管理し、
PlayerControllerへロジックを集中させない構成とする。

これにより、
状態追加時の変更影響範囲を局所化する。

---

### 3. PlayerContextによる状態共有設計

`PlayerContext.cs` を導入し、
Player全体で共有される情報および依存参照を集約管理する。

例:

- PlayerController参照
- Rigidbody参照
- Input情報
- 各種Runtime情報
- State共有データ

StateはContext経由で必要情報へアクセスすることで、
直接依存を最小化する。

これにより以下を実現する。

- 疎結合化
- 状態間依存削減
- Mock化容易化
- テスト容易性向上

---

### 4. ScriptableObjectによる設定外部化

`SO_PlayerSettings.cs` を導入し、
PlayerパラメータをScriptableObjectとして外部管理する。

対象例:

- MoveSpeed
- ShootInterval
- RotationSpeed
- CoolTime
- その他ゲームバランス値

これにより、
コード修正不要でのバランス調整を可能にする。

また、
Prefab間で設定共有可能な構成とする。

---

### 5. Utility/Constant層の整理

以下を共通基盤層として分離する。

- `Algorithm.cs`
- `Constant.cs`

役割:

#### Algorithm.cs

- 汎用計算処理
- 数学ロジック
- 共通アルゴリズム

#### Constant.cs

- 定数管理
- Magic Number排除
- ゲーム内共通値管理

これにより、
再利用性および保守性を向上させる。

---

### 6. PlayerController責務の最小化

`PlayerController.cs` は、
Player制御全体のCoordinatorとして機能させる。

責務を以下へ限定する。

- State更新管理
- 初期化
- Unityイベント橋渡し
- Component参照保持

ゲームロジックそのものは、
可能な限りState側へ委譲する。

---

## Consequences (結果と影響)

### メリット

#### 保守性向上

状態ごとに責務分離されることで、
機能追加時の修正影響範囲を最小化できる。

---

#### 拡張容易性向上

以下State追加が容易になる。

- DashState
- DamageState
- KnockBackState
- SkillState
- ChargeState
- DeadState

---

#### テスト容易性向上

State単位でロジック検証可能となり、
ユニットテストおよびMock化が容易になる。

---

#### パラメータ調整容易化

ScriptableObject化により、
エンジニア以外でもゲームバランス調整が可能となる。

---

#### 可読性向上

責務が整理されることで、
コード探索コストを削減できる。

---

#### 将来的な機能拡張への適応

以下への発展を見据えた構成となる。

- Online同期
- Replay
- AI共有制御
- Animation State連携
- Ability System
- Data Driven Design

---

### トレードオフ

#### クラス数増加

State分離により、
小規模プロジェクトではクラス数増加コストが発生する。

---

#### 初期学習コスト

State PatternおよびContext設計への理解が必要となる。

---

#### State遷移管理複雑化

状態数増加に伴い、
遷移管理の整理が必要となる。

不適切な遷移設計は、
循環遷移や状態競合を引き起こす可能性がある。

---

#### Context肥大化リスク

責務整理を怠ると、
PlayerContextへ依存情報が集中する可能性がある。

そのため、
Context責務監視が継続的に必要となる。

---

#### ScriptableObject運用コスト

参照切れや設定共有事故など、
Unity特有のAsset管理コストが発生する。

---

#### GCおよび参照管理注意

頻繁なState生成設計を行う場合、
GC発生リスクがある。

そのため、
Stateインスタンス管理方式には注意が必要となる。

## Learnings (学習メモ)

- （任意）State Pattern導入で得られた知見や、実装時に発見した注意点を記載してください。

## References (参考資料)

- （任意）関連ドキュメントや参考記事のURLを列挙してください。
