using System;
using System.Collections.Generic;
using System.Text;

namespace SatIO
{
    [Serializable()]
    public class DoorIO
    {
        /// <summary>
        /// IDを設定・取得
        /// </summary>
        public int ID { get; set; }

        public VectorIO Position { get; set; }

        /// <summary>
        /// リソースへのパス
        /// </summary>
        public string ResourcePath { get; set; }

        /// <summary>
        /// 遷移先のマップ名
        /// </summary>
        public string MoveToMap { get; set; }

        /// <summary>
        /// 遷移先の指定にDoor IDを使用するか
        /// </summary>
        public bool IsUseMoveToID { get; set; }

        /// <summary>
        /// 遷移先のDoor ID
        /// </summary>
        public int MoveToID { get; set; }

        /// <summary>
        /// 解放条件スクリプト
        /// </summary>
        public string KeyScriptPath { get; set; }

        /// <summary>
        /// 遷移先の座標
        /// </summary>
        public VectorIO MoveToPosition { get; set; }
    }
}
