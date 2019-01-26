# MapObject API仕様
## SatScript.IMapObject
スクリプト用インターフェース
GlobalTypeで使う。

| Member | Type | 概要 |
| --- | --- | --- |
| Tag | ```string{get;set;}``` | オブジェクト認識用 | 
| Position | ```Vector{get;set;}``` | 現在座標 | 
| Color | ```Color{get;set;}``` | 色 |
| HP | ```int{get;set;}``` | HP初期値(100) |
| State | ```string{get;set;}``` | アニメーション状態 |
| AddAnimationPart | ```void(string animationGroup, string extension, int sheets, string partName, int interval)``` | アニメーションパートを追加する |
| MapObjectType | ```MapObjectType{get;set;}``` | マップオブジェクトのタイプ |
| Collision | ```ICollision{get;}``` | 衝突判定情報 |
| Velocity | ```Vector{get;set;}``` | 速度 | 
| SetForce | ```void(Vector direct, Vector position)``` | 指定点に力を加える |
| SetImpulse | ```void(Vector direct, Vector position)``` | 指定点に衝撃を加える |
| CollisionGroup | ```short{get;set;}``` | 衝突グループ |
| CollisionCategory | ```ushort{get;set;}``` | 衝突カテゴリー |
| CollisionMask | ```ushort{get;set;}``` | 衝突カテゴリー用マスク | 
| IsAllowRotation | ```bool{get;set;}``` | 回転を許可する | 
| Sensors | ```Dictionary<string, ISensor>{get;}``` | センサー情報を設定・取得 |
| Update | ```Action<IMapObject>{get;set;}``` | OnUpdate時に行われるデリゲート |
| IsReceiveDamage | ```bool{get;set;}``` | ダメージを受けるか |
| Camp | ```OwnerType{get;set;}``` | 陣営 |
| SetChild | ```void(string name, string scriptpath)``` | 子MapObjectを設定する|
| CreateChild | ```void(string name, Vector position)``` | 子オブジェクトを配置 |
| LoadEffect | ```void(string animatonGroup, string extension, int sheets, string name, int interval)``` | エフェクトをロードする |
| SetEffect | ```void(string name, Vector position)``` | エフェクトを配置する |
| Dispose | ```void()``` | オブジェクトを消去する |
## SatScript.ISensor
| Member | Type | 概要 |
| --- | --- | --- |
| Position | ```Vector{get;set;}``` | 親MapObjectとの相対座標 | 
| Collision | ```ICollsion{get;}``` | 衝突判定 |
| Radius | ```float{get;set;}``` | センサーサイズ(半径) |
## SatScript.MapObject
各スクリプトからMapObject(EventObject)の情報を取得するためのAPI

| Member | Type | 概要 |
| --- | --- | --- |
| MapObjects | ```static IEnumerable<MapObject>``` | 現在稼働しているMapObject |
| Tag | ```string{get;}``` | オブジェクト認識用 | 
| Position | ```Vector{get;}``` | 現在座標 | 
| HP | ```int{get;}``` | HP初期値(100) |
| State | ```string{get;}``` | アニメーション状態 |
| Color | ```Color{get;}``` | 色 |
| MapObjectType | ```MapObjectType{get;}``` | マップオブジェクトのタイプ |
| Collision | ```ICollision{get;}``` | 衝突判定情報 |
| Velocity | ```Vector{get;}``` | 速度 | 
| IsAllowRotation | ```bool{get;}``` | 回転を許可する | 
| IsReceiveDamage | ```bool{get;}``` | ダメージを受けるか |
| Camp | ```OwnerType{get;}``` | 陣営 |
| Dispose | ```void()``` | オブジェクトを消去する |
