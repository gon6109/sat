# Player API仕様
## SatScript.IPlayer
スクリプト用インターフェース
| Member | Type | 概要 |
| --- | --- | --- |
| Name | ```string{get;set;}``` | キャラクター名 | 
| Position | ```Vector{get;set;}``` | 現在座標 | 
| HP | ```int{get;set;}``` | HP初期値(100) |
| State | ```string{get;set;}``` | アニメーション状態 |
| Collision | ```ICollision{get;}``` | 衝突判定情報 |
| AddAnimationPart | ```void(string animationGroup, string extension, int sheets, string partName, int interval)``` | アニメーションパートを追加する |
| IsColligedWithGround | ```bool{get;}``` | 地面と接しているか | 
| Velocity | ```Vector{get;set;}``` | 速度 | 
| SetForce | ```void(Vector direct, Vector position)``` | 指定点に力を加える |
| Update | ```Action<IPlayer>{get;set;}``` | OnUpdate時に行われるデリゲート |
| GetInputState | ```int(Inputs inputs)``` | 入力状態を取得（Event対応) |
| LoadEffect | ```void(string animatonGroup, string extension, int sheets, string name, int interval)``` | エフェクトをロードする |
| SetEffect | ```void(string name, Vector position)``` | エフェクトを配置する |
| IsEvent | ```bool{get;}``` | イベント時か |
## SatScript.Player
各スクリプトからPlayerの情報を取得するためのAPI
| Member | Type | 概要 |
| --- | --- | --- |
| Players | ```static IEnumerable<string>{get;}``` | 使用されているキャラ名 |
| CurrentPlayer | ```static SatScript.Player``` | 現在操作されているプレイヤー |
| Position | ```Vector{get;}``` | 現在座標 | 
| HP | ```int{get;}``` | HP初期値(100) |
| State | ```string{get;}``` | アニメーション状態 |
| Collision | ```ICollision{get;}``` | 衝突判定情報 |
| IsColligedWithGround | ```bool{get;}``` | 地面と接しているか | 
| Velocity | ```Vector{get;}``` | 速度 | 
| IsEvent | ```bool{get;}``` | イベント時か |
