using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Algorithm
{
    /// <summary>
    /// 必須コンポーネントやフィールドの null/不足チェックを提供するユーティリティ
    /// </summary>
    public static class ComponentValidator
    {
        /// <summary>
        /// 複数の UnityEngine.Object フィールドを一括でチェックし、
        /// 1つでも null があればエラーログを出力して false を返す
        /// </summary>
        /// <param name="targetComponent">チェックを実行している MonoBehaviour のインスタンス (ログに表示するため)</param>
        /// <param name="requiredObjects">チェック対象の UnityEngine.Object の配列</param>
        /// <returns>すべて正常に設定されていれば true</returns>
        public static bool ValidateAndLogRequired(MonoBehaviour targetComponent, params Object[] requiredObjects)
        {
            if (requiredObjects == null)
            {
                Debug.LogError(
                    ConsoleMessageHelper.ValidateRequiredComponent<object>(null),
                    targetComponent
                );
                return false;
            }

            if (requiredObjects.Any(obj => obj == null))
            {
                var missingObject = requiredObjects.First(obj => obj == null);

                Debug.LogError(
                    ConsoleMessageHelper.ValidateRequiredComponent(missingObject),
                    targetComponent
                );
                return false;
            }

            return true;
        }

        /// <summary>
        /// リストが null または空であるかをチェックし、エラーログを出力して false を返す
        /// </summary>
        /// <typeparam name="T">リストの型</typeparam>
        /// <param name="targetComponent">チェックを実行している MonoBehaviour のインスタンス</param>
        /// <param name="requiredLists">チェック対象のリスト</param>
        /// <returns>リストが有効であれば true</returns>
        public static bool ValidateAndLogRequiredList<T>(MonoBehaviour targetComponent, params List<T>[] requiredLists)
        {
            if (requiredLists == null)
            {
                Debug.LogError(
                    ConsoleMessageHelper.ValidateRequiredComponents<T>(null),
                    targetComponent
                );
                return false;
            }

            foreach (var list in requiredLists)
            {
                if (list.IsNullOrEmpty())
                {
                    Debug.LogError(
                        ConsoleMessageHelper.ValidateRequiredComponents(list),
                        targetComponent
                    );
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// コレクション（配列・リスト）のユーティリティ拡張
    /// null や空配列の判定など、共通的な安全チェックをまとめとくクラス
    /// </summary>
    public static class CollectionHelper
    {
        /// <summary>
        /// 配列が null または空であるかを判定する
        /// </summary>
        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return (array is null || array.Length == 0);
        }

        /// <summary>
        /// リストが null または空であるかを判定する
        /// </summary>
        public static bool IsNullOrEmpty<T>(this List<T> list)
        {
            return (list is null || list.Count == 0);
        }
    }

    /// <summary>
    /// コンソールに出力するメッセージのユーティリティ拡張
    /// </summary>
    public static class ConsoleMessageHelper
    {
        /// <summary>
        /// 汎用必須コンポーネント不足メッセージ
        /// </summary>
        public static string ValidateRequiredComponent<T>(T missingTargetObject)
        {
            var typeName = typeof(T).Name;

            if (missingTargetObject == null)
            {
                return $"{typeName} 必須コンポーネントが不足しています。";
            }

            // null ではないがバリデーションに失敗したという特殊なケース用のメッセージ
            return $"{typeName} コンポーネントが設定されていません。";
        }

        /// <summary>
        /// 汎用必須コンポーネント（Collections）不足メッセージ
        /// </summary>
        public static string ValidateRequiredComponents<T>(List<T> missingTargetObjects)
        {
            var elementType = typeof(T).Name;
            if (missingTargetObjects == null)
            {
                return $"必須の {elementType} コレクションが null、または不足しています。";
            }

            if (missingTargetObjects.Count == 0)
            {
                return $"{elementType} コレクションが空です。要素を追加してください。";
            }

            // 汎用フォールバック
            return $"{elementType} コレクションが正しくありません。";
        }
    }
}