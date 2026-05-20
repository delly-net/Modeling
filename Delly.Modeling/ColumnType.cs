namespace Delly.Modeling
{
    /// <summary>
    /// 列类型
    /// </summary>
    public enum ColumnType : int
    {
        /// <summary>
        /// 未定义
        /// </summary>
        UNSET = 0x0000,

        /// <summary>
        /// 数值
        /// </summary>
        DECIMAL = 0x0100,

        /// <summary>
        /// 布尔型
        /// </summary>
        BOOL = DECIMAL + 1,

        /// <summary>
        /// 整型
        /// </summary>
        INTEGER = DECIMAL + 2,

        /// <summary>
        /// 长整型
        /// </summary>
        LONG = DECIMAL + 3,

        /// <summary>
        /// 时间
        /// </summary>
        TIME = 0x0200,

        /// <summary>
        /// 字符串
        /// </summary>
        VARCHAR = 0x0300,

        /// <summary>
        /// 文本
        /// </summary>
        TEXT = 0x0400,
    }
}


