# Collision API仕様
スクリプト側は各オブジェクトがもつ `ICollision`を触る
## ICollision
| Member | Type | 概要 |
| --- | --- | --- |
| IsColligedWithoOstacle | ```bool{get;}``` | 壁・床と衝突しているか | 
| IsColligedWithPlayer | ```bool{get;}``` | Playerと衝突しているか | 
| ColligingMapObjectTags | ```IEnumerable<string>{get;}``` | 衝突しているMapObject(EventObject)のTag一覧 |  