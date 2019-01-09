# ファイル定義
## ファイル形式一覧
| 形式名 | 用途 | 拡張子 |
|:---:|:----:|:----:|
| Map | マップデータ | .map |
| BackGround | 背景 | .bg |
| MapObject | マップ上の動的オブジェクト | .obj |
| CharacterImage | イベント時に表示するキャラクターグラフィック | .ci |
| Player | プレイヤー | .pc |
| EventObject | イベント用MapObject拡張 | .eobj |

## 共通
スクリプトファイル以外は、通常時はXML形式で扱い、本番配置のときのみバイナリに変換する。  
変換はパッケージ化するときに行う。

## Map
マップデータを保持する。  
バイナリ・XML形式で保存する。
| 要素　| 概要 |
|:---:|:---|
|Name|マップ名|
|Size|マップサイズ|
|BGM|初期BGM|
|CollisionBoxes|四角形の障害物|
|CollisionTriangles|三角形の障害物|
|Doors|マップ遷移用オブジェクト|
|MapObjects|動的オブジェクト|
|NPCMapObjects|イベント用動的オブジェクト( **破壊的変更予定** )|
|MapEvents|イベント・シナリオ|
|BackGrounds|背景|
|CameraRestrictions|カメラ制限領域|
|SavePoints|セーブポイント|

## BackGround
背景
動的にアニメーションさせる。
C#スクリプトで制御する。  
 **TODO:仕様策定**

## MapObject
マップ用動的オブジェクト  
C#スクリプトで制御する。  
 **TODO:仕様策定**

## CharacterImage
イベント用キャラクターグラフィック  
バイナリ・XML形式で保存する。
| 要素　| 概要 |
|:---:|:---|
|Name|キャラクター名|
|BaseImagePath|固定画像のパス|
|DiffImagePaths|差分名をキーとした差分画像へのパスのDictionary|

## Player
プレイヤーの挙動が記述されたファイル  
C#スクリプトで動作する。  
 **TODO:仕様策定**

## EventObject
イベント用MapObject。  
イベント時動かすことが可能になる。   
C#スクリプトで動作する。  
 **TODO:仕様策定**