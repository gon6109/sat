using System;
using System.Collections.Generic;
using System.Text;

namespace SatScript.Common
{
    /// <summary>
    /// スクリプト用データ
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class ScriptDataContainer<TKey, TValue> : Dictionary<TKey, TValue>
    {
        /// <summary>
        /// データを取得設定する
        /// </summary>
        /// <param name="key">オブジェクト</param>
        /// <returns>データ</returns>
        public new TValue this[TKey key]
        {
            get => ContainsKey(key) ? base[key] : default;
            set => base[key] = value;
        }
    }
}
