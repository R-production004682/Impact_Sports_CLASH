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