# スクリプト仕様
## API一覧
| API | 概要 | MapObject | EventObject | BackGround | Player | 
| ---- | ---- | ---- | ---- | ---- | ---- |
| Common | ゲームの基本機能を提供する | T | T | T | T |
| Player | プレイヤーの制御に関する機能を提供する | T | T | T | F |
| MapObject | MapObjectの制御に関する機能を提供する | T | T | T | T |
| Collision | 衝突判定情報を提供する | T | T | F | T |
## GlobalType一覧
直接オブジェクトのメンバを触れないようインターフェースを定義する。

| GlobalType | 概要 | 
| ---- | ---- |
| IMapObject | MapObject用 |
| IPlayer | Player用 |
| IEventObject | IEventObject用 |
| IBackGround | BackGround用 | 
