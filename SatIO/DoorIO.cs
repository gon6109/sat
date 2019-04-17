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
        public int ID;

        public VectorIO Position;

        /// <summary>
        /// リソースへのパス
        /// </summary>
        public string ResourcePath;

        /// <summary>
        /// 遷移先のマップ名
        /// </summary>
        public string MoveToMap;

        /// <summary>
        /// 遷移先の指定にDoor IDを使用するか
        /// </summary>
        public bool IsUseMoveToID;

        /// <summary>
        /// 遷移先のDoor ID
        /// </summary>
        public int MoveToID;

        /// <summary>
        /// 解放条件スクリプト
        /// </summary>
        public string KeyScriptPath;

        /// <summary>
        /// 遷移先の座標
        /// </summary>
        public VectorIO MoveToPosition;
    }
}
