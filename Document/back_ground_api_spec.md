# BackGround API仕様
#SatScrpit.IBackGround
| Member | Type | 概要 |
| --- | --- | --- |
| State | ```string{get;set;}``` | アニメーションの状態 |
| Update | ```Action<IbackGround>{get;set;}``` | OnUpdate時に呼び出される関数のデリゲート |
| AddAnimationPart | ```void(string animationGroup, string extension, int sheets, string partName, int interval)``` | アニメーションパートを追加する |