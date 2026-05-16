# ADR 0001: FPSカメラ（Cinemachine 3.1.6）のセットアップ

- **Date**: 2026-05-16
- **Status**: Accepted
- **Related Spec**: `.github/Specs/Impact_Sports_CLASH_GDD_v1.md` の「プレイヤーの基本操作」
- **Related Plan**: `.github/Planning/Shooter_Prototype_Plan.md` の「2. プレイヤーの基本操作（FPS基盤）」

## Context (背景と課題)
- シュータープロトタイプにおいて、プレイヤーの視界となる一人称視点（FPSカメラ）を構築する必要があった。
- 独自のスクリプトでマウス入力から四元数を計算してカメラを回すアプローチもあるが、後々の視点の揺れ（ヘッドボブ）やFOV設定、感度調整などの拡張性を考慮し、Unity 6標準の Cinemachine 3.1.6 を活用することとした。

## Decision (決定事項)
- `CinemachineCamera` をシーンに配置し、以下の構成で実装した。
  - **Tracking Target**: プレイヤーキャラクターの子オブジェクト（頭部位置）に設定。
  - **Position Control**: `Hard Lock to Target` を使用し、頭部位置へ完全に同期。
  - **Rotation Control**: `Pan Tilt` を使用し、首振りを実現。
  - **入力制御**: `Cinemachine Input Axis Controller` をアタッチし、Input Systemのアクションを `Pan Tilt` 軸に紐付けた（感度やインバートは `Gain` パラメータで制御）。

## Consequences (結果と影響)
- **メリット**: 
  - カメラ制御用のカスタムスクリプトを書かずに、高品質なマウス操作のFPSカメラ基盤が完成した。
  - 後からダメージ時のカメラ揺れ（Noise）や、ダッシュ時のFOV変更などをコンポーネントベースで容易に追加できる設計になった。
- **トレードオフ**: 
  - Cinemachine 3.1 の新アーキテクチャ（Input Axis Controllerの分離など）に関する独自仕様の把握が必要だった（本ADRの学習メモを整備することで解決済み）。

## Learnings (学習メモ)
事前調査で整理した、Unity 6（Cinemachine 3.1.6）`CinemachineCamera` の主要パラメータ備忘録。

### 1. 基本設定 (Status & Core)
- **Priority**: アクティブ判定用。数値が最大のものがメインカメラになる。
- **Status**: 現在のカメラの動作状態（Live, Standby, Disabled）を表示。
- **Blend Hint**: 別のカメラから切り替わる際、どのようにブレンド移動するか（直線、球面、円柱など）のヒントを指定。
- **Tracking Target**: 旧 Follow/LookAt 統合版。追従対象のTransformをセット。
- **Standby Update**: 非アクティブ時の更新頻度（`Never` または `Round Robin` 推奨）。

### 2. Lens (レンズ)
- **FOV (視野角)**: FPSは一般に70〜90。スクリプト操作でズーム演出が可能。
- **Near Clip**: 描画の最短距離。FPSでは腕のめり込み防止のため `0.01` など極小値に。
- **Dutch**: カメラのZ軸回転（ロール/傾き）。ダイナミックな構図や、被弾時の傾き演出に利用。
- **Mode Override**: ベースとなるメインカメラの設定を上書きし、Orthographic（平行投影）やPhysical Camera設定をこのカメラ専用に強制する。

### 3. Global Settings (全体設定)
- **Save During Play**: チェックを入れると、**プレイモード中にいじったパラメータ変更がプレイ終了後も保存される**（調整時に非常に便利）。
- **Game View Guides**: Gameビューにフレーミング用のガイド枠（デッドゾーン等）を表示する機能。以下の選択肢がある：
  - `Disabled`: ガイドを非表示。
  - `Passive`: ガイドを表示するのみ（視覚的な確認用）。
  - `Interactive`: ガイドを表示し、**Gameビュー上で直接枠をマウスドラッグしてゾーンの広さを調整できる**（直感的な調整が可能）。

### 4. Position Control (位置制御)
- **Hard Lock to Target**: ターゲット位置に完全固定（★FPSの頭部追従に最適）。
- **Follow**: ターゲットとの一定の相対位置を保ちながら追従する標準的な追従モジュール。
- **Third Person Follow**: 肩越し視点（TPS）用。カメラとキャラの距離や障害物回避の基盤。
- **Orbital Follow**: プレイヤーがカメラをターゲットの周囲で操作（オービット）できる。
- **Spline Dolly**: （旧 Tracked Dolly）事前に引いたスプライン（パス）に沿ってカメラを移動させる。
- **Position Composer**: カメラを回転させず、ターゲットが画面内の特定位置（スクリーン座標）に収まるよう「位置を移動」してフレーミングする。
- **None (Do Nothing)**: 追従せず、手動で配置した位置に留まる。

### 5. Rotation Control (回転制御)
- **Pan Tilt**: （旧 POV）マウス入力等による首振り用。★FPSの視点移動に最適。
- **Rotation Composer**: （旧 Composer）ターゲットが常に画面の特定位置に収まるよう「カメラを回転」させてフレーミングする（デッドゾーンやソフトゾーンが使える）。
- **Hard Look At**: ターゲットの中心を強制注視（減衰や遊びがない）。
- **Spline Dolly Look At Targets**: スプライン（パス）上のターゲット等を見るための回転制御。
- **Rotate With Follow Target**: ターゲットの回転に合わせてカメラも回転する（Follow系と一緒に使いやすい）。
- **None (Do Nothing)**: 回転せず、現在の向きを維持する。

### 6. Extensions (拡張機能)
- **Noise (Basic Multi Channel Perlin)**: 手ブレ、ヘッドボブ、被弾や爆発時のカメラ揺れ。
- **Collider**: TPS等で、カメラとキャラ間の壁・障害物を検知してカメラを前に寄せる機能。
- **Volume Settings**: このカメラ稼働時専用のPost Processing（被写界深度や色調）を適用。

### 7. Pan Tilt の詳細パラメータ (CM 3.1.6)
FPS視点の構築に必須となる `Pan Tilt` モジュール特有のパラメータ群。
※**重要**: CM 3.1では Pan Tilt 自身は入力を受け取らず、別途カメラに `Cinemachine Input Axis Controller` コンポーネントをアタッチしてマウス/スティック入力を流し込む設計に変更されている。

- **Reference Frame**: 回転の基準となる座標系（`Parent Object`, `World`, `Tracking Target` 等から選択）。
- **Pan Axis / Tilt Axis**: 左右（Pan）と上下（Tilt）それぞれの回転軸設定。
  - **Value**: 現在の角度（度数法）。
  - **Range**: 回転の最小/最大制限。**Tilt（上下の首振り）は仕様上必ず [-90, 90] の範囲に収めること**。
  - **Center**: リセンター（自動振り向き）時の目標となる中心値。
  - **Wrap**: チェックを入れると、Rangeの端に到達した時に反対側へループする（360度見回せるPan軸でよく使う）。
- **Recentering**: 無操作時に自動で正面（Centerやターゲット方向）へ視点を戻す機能。
  - **Wait**: 無操作になってから視点が戻り始めるまでの待機時間（秒）。
  - **Recentering Time**: 視点が戻るまでにかかる時間。
  - **Recenter Target**: 戻る先の基準（設定した `Center` の値か、ターゲットの正面方向か等）。

### 8. Cinemachine Input Axis Controller (CM 3.1.6)
CM 3.1 において、カメラ（Pan Tilt や Orbital Follow など）にプレイヤーの入力（マウスやゲームパッド）を伝えるための必須コンポーネント。これを `CinemachineCamera` と同じオブジェクトにアタッチして設定する。

- **基本設定**:
  - **Scan Recursively**: 子オブジェクトも含めて操作可能な軸を探す。
  - **Suppress Input While Blending**: カメラ切り替えのブレンド（遷移）中に入力を無視するかどうか。
- **Driven Axes**: カメラにアタッチされたモジュール（Pan Tilt等）から、自動的に操作可能な軸（`Look X (Pan)` や `Look Y (Tilt)`）が検出され表示される。
  - **Input Action**: `Player/Look` などの Input Action Reference をここに紐付ける。
  - **Gain**: 入力値に乗算される値（いわゆる **マウス感度 / Sensi** に相当）。上下の視点移動（Tilt）を反転（インバート）させたい場合は、ここの数値をマイナス（例：`-1`）にする。
  - **Accel Time / Decel Time**: 入力の加速・減速（スムージング）にかかる時間。0にすると入力がダイレクトに反映され、数値を上げるとカメラの動きに慣性がつく。
  - **Cancel Delta Time**: 入力値に対する Delta Time（フレーム間の時間差）の自動乗算をキャンセルするかどうか。
- **Player Index**: ローカルマルチプレイ（画面分割など）で、どのプレイヤーの入力を受け付けるか（0, 1, 2...）。デフォルトの `-1` は全プレイヤー（または最初にアクティブなプレイヤー）を意味する。
- **Auto Enable Inputs**: チェックを入れると、紐付けた Input Action をカメラ起動時に自動で有効化（`Enable()`）してくれる。自作スクリプト側で手動でInput Systemを有効化する手間が省けるため、基本ONでOK。

## References (参考資料)
- Unity公式: Cinemachine 3.x マニュアル
- 事前調査ログ（AIアシスタントとの対話）
