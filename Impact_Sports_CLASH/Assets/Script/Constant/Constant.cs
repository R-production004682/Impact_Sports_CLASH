namespace Clash.Constants
{
    /// <summary>
    /// 入力アクションマップの識別子
    /// </summary>
    public enum InputActionId
    {
        None = 0,
        Player,
        UI,
        Move,
        Shoot,
        Jump,
        Interact
    }

    public static class InputActionMapExtensions
    {
        private static readonly string[] _actionNames =
        {
            "None",
            "Player",
            "UI",
            "Move",
            "Shoot",
            "Jump",
            "Interact"
        };

        /// <summary>
        /// ActionMap を高速に取得
        /// </summary>
        public static string ToIdName(this InputActionId id)
        {
            var index = (int)id;
            return (index >= 0 && index < _actionNames.Length) ? _actionNames[index] : _actionNames[0];
        }
    }
}