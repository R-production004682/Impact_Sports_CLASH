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
        Dodge,
        Interact,
        Catch
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
            "Dodge",
            "Interact",
            "Catch"
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

    public static class PlayerConfig
    {
        // 入力されているかどうかの閾値
        public const float INPUT_DEADZONE = 0.1f;

        // 停止とみなす速度の閾値
        public const float STOP_VELOCITY_THRESHOLD = 0.1f;

        // 接地判定に使うレイヤー名
        public const string GROUND_LAYER_NAME = "Ground";
    }
}