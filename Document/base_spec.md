# 基本仕様
以下の構成で成り立っている(下の要素は上の要素に依存している)

| 要素名 | 概要 |
|:---:|:---|
|SatIO|シリアライズ用クラス|
|SatPlayer|エディタで作成したデータをロード・プレイするための処理を行う|
|SatCore|エディタでAltseedなどOS非依存の処理を行う。UI配置用の属性の定義も行う|
|SatUI|エディタにおけるView|
