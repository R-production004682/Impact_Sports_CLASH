# GitHub Copilot Master Instructions

このファイルは、本プロジェクトにおけるGitHub Copilotの各種ガイドライン（指示書）のインデックス（目次）です。
Copilotは、ユーザーからの要求やコンテキストに応じて、以下のリストに定義された各ドメインの `instructions.md` を読み込み、そのルールを厳格に適用してください。

## 📂 インデックス（指示書リスト）

用途に合わせて、該当するパスのルールを参照すること。

### 1. コーディング規約 (Coding Conventions)
- **File Path**: `.github/CodingConventions/instructions.md`
- **Context**: C#スクリプトの作成、リファクタリング、Unityコンポーネントの実装時。
- **Description**: 命名規則、C#の標準規約、およびUnity特有のパフォーマンス最適化（GC回避、UniTaskの利用など）に関するルール。

### 2. アーキテクチャ決定記録 (ADR)
- **File Path**: `.github/ADR/instructions.md`
- **Context**: アーキテクチャの変更、新しいパッケージの導入、ECSやレンダリング手法などの重要な技術方針を決定したPRを作成・実装する時。
- **Description**: `docs/ADR/` フォルダへの記録ルールと、1PR=1ADRの原則、およびMarkdownのフォーマット定義。

### 3. ゲーム企画書・仕様書 (Specs)
- **File Path**: `.github/Specs/`
- **Context**: ゲームデザイン、メカニクス、クラス設計、ボールシステム等の設計意図やコンセプトを把握したい時。
- **Description**: 本プロジェクトのゲーム企画書（GDD）およびプレゼン資料。現段階は仮仕様であり、厳密に準拠すべきルールではない。実装されたコードが「何を目的としているか」の当たりをつけるための参考資料として活用すること。
- **Files**:
  - `Impact_Sports_CLASH_GDD_v1.md` — ゲーム企画書v1（仮仕様・コンセプト段階）
  - `Impact_Sports_CLASH_Slides.md` — プレゼン用スライド（Marp形式）

### 4. 計画・タスク管理 (Planning)
- **File Path**: `.github/Planning/`
- **Context**: 開発ロードマップ、マイルストーン、タスク分解、スプリント計画など。
- **Description**: 開発計画やタスクリストを格納するフォルダ。