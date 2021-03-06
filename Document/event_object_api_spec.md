# EventObject API仕様
## SatScript.IEventObject
スクリプト用インターフェース
GlobalTypeで使う。
基本的には、MapObjectと共通。(内部的にはMapObjectの継承であるため)

| Member | Type | 概要 |
| --- | --- | --- |
| Tag | ```string{get;set;}``` | オブジェクト認識用 | 
| Position | ```Vector{get;set;}``` | 現在座標 | 
| HP | ```int{get;set;}``` | HP初期値(100) |
| State | ```string{get;set;}``` | アニメーション状態 |
| Color | ```Color{get;set;}``` | 色 |
| AddAnimationPart | ```void(string animationGroup, string extension, int sheets, string partName, int interval)``` | アニメーションパートを追加する |
| MapObjectType | ```MapObjectType{get;set;}``` | マップオブジェクトのタイプ |
| Collision | ```ICollision{get;}``` | 衝突判定情報 |
| IsColligedWithGround | ```bool{get;}``` | 地面と接しているか |
| Velocity | ```Vector{get;set;}``` | 速度 | 
| SetForce | ```void(Vector direct, Vector position)``` | 指定点に力を加える |
| SetImpulse | ```void(Vector direct, Vector position)``` | 指定点に衝撃を加える |
| CollisionGroup | ```short{get;set;}``` | 衝突グループ |
| CollisionCategory | ```ushort{get;set;}``` | 衝突カテゴリー |
| CollisionMask | ```ushort{get;set;}``` | 衝突カテゴリー用マスク | 
| IsAllowRotation | ```bool{get;set;}``` | 回転を許可する | 
| Sensors | ```Dictionary<string, ISensor>{get;}``` | センサー情報を設定・取得 |
| Update | ```Action<IEventObject>{get;set;}``` | OnUpdate時に行われるデリゲート |
| IsReceiveDamage | ```bool{get;set;}``` | ダメージを受けるか |
| Camp | ```OwnerType{get;set;}``` | 陣営 |
| SetChild | ```void(string name, string scriptpath)``` | 子MapObjectを設定する(EventObject不可)|
| CreateChild | ```void(string name, Vector position)``` | 子オブジェクトを配置 |
| LoadEffect | ```void(string animatonGroup, string extension, int sheets, string name, int interval)``` | エフェクトをロードする |
| SetEffect | ```void(string name, Vector position)``` | エフェクトを配置する |
| Dispose | ```void()``` | オブジェクトを消去する |
| IsEvent | ```bool{get;}``` | イベント時であるか |
| GetInputState | ```int(Inputs inputs)``` | イベント時オブジェクトを動作させるために用いる |
