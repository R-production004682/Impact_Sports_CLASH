# C# & Unity Coding Conventions

本ファイルは、プロジェクトにおけるC#スクリプトおよびUnityコンポーネントの実装に関するコーディング規約です。
コードを生成・提案・リファクタリングする際は、以下のルールを厳格に適用してください。

## 1. 命名規則 (Naming Conventions)
- **クラス、構造体、列挙型、メソッド、プロパティ**: `PascalCase`
- **インターフェース**: `I` から始める `IPascalCase` (例: `IDamageable`)
- **ローカル変数、メソッドの引数**: `camelCase`
- **プライベートフィールド**: `_camelCase` (アンダースコア始まり)
- **定数 (const / readonly)**: `PascalCase` または `UPPER_SNAKE_CASE`
- **波括弧 `{ }`**: Allmanスタイル (改行して配置する) を基本とする。

## 2. Unity特有のルール (Unity Specific Rules)
- **シリアライズとカプセル化**: 
  Inspectorで設定・参照したい変数は `public` にせず、必ず `[SerializeField] private` を使用すること。
  外部のクラスから値を参照させたい場合は、自動実装プロパティ (`public Type Name { get; private set; }`) を用いること。
- **タグの比較**: `gameObject.tag == "Player"` は使用せず、必ず `gameObject.CompareTag("Player")` を使用すること。
- **オブジェクト検索**: `GameObject.Find()` や `FindObjectOfType()` は極端に重いため、原則使用禁止。必要な参照はInspectorからアタッチするか、依存性注入を活用すること。

## 3. パフォーマンスとメモリ管理 (Performance & Memory)
- **GC Allocationの回避**: 
  `Update()`, `FixedUpdate()`, `LateUpdate()` などの毎フレーム呼ばれるメソッド内でのメモリ確保 (`new` キーワードによるインスタンス化、文字列の結合、LINQの使用) は厳禁とする。
- **コンポーネントのキャッシュ**:
  `GetComponent<T>()` を毎フレーム呼び出してはならない。必ず `Awake()` または `Start()` 内で取得し、プライベートフィールドにキャッシュすること。

## 4. 非同期処理 (Asynchronous Processing)
- **UniTaskの徹底**: 
  Unity標準の `Coroutine` (IEnumerator) は原則として使用しない。非同期処理、待機処理、および時間経過を伴うロジックは、すべて `UniTask` および `async/await` を使用して実装すること。
- キャンセルトークンの受け回し (`CancellationToken`) を意識し、オブジェクト破棄時に非同期処理が安全に停止するよう努めること。

## 5. ドキュメンテーション (Documentation)
- publicなメソッドや複雑なロジックを持つクラスには、必ずXMLドキュメントコメント (`/// <summary>`) を記述すること。
- コードの意図が直感的に読み取れない箇所には、簡潔なインラインコメントを残すこ

## 6. 名前空間 (Namespaces)
- **ファイルスコープ名前空間の利用**: 
  インデントのネストを浅く保ち可読性を上げるため、必ずC# 10の「ファイルスコープ名前空間」 (`namespace ProjectName.FeatureName;`) を使用すること。従来のブロックレベル (`namespace { ... }`) は使用しない。
- **ディレクトリ構造との一致**: 
  名前空間は `PascalCase` で記述し、スクリプトが配置されているフォルダ階層と完全に一致させること。(例: `Scripts/AI/LLM/` フォルダにある場合は `namespace ProjectName.AI.LLM;` とする)
- **グローバル名前空間の禁止**: 
  クラスやインターフェースをグローバル名前空間に配置することは厳禁とする。新しくスクリプトを生成する際は、必ず適切な名前空間を付与すること。