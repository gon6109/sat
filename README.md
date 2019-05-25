# Scroll Action Tool

横スクロールアクション向けマップエディター

## 主な機能

- マップ作成
- マップのプレビュー
- C#スクリプトで各種オブジェクトの作成
- 各種オブジェクトのプレビュー
- シナリオの記述

# 動作環境

- .Net Framework 4.6.2
- Windows OS

ビルド時

- Visual Studio 2019/2017

## 導入

### 自分でビルドする場合

コンソールで以下実行

```
git clone https://github.com/gon6109/sat.git
cd sat
git submodule update --init --recursive
cd BaseComponent
nuget restore BaseComponet.sln
```

sat.slnを開いて、ビルドしてください。

### バイナリをダウンロードする場合

[ここ](https://github.com/gon6109/sat/releases)から
一番上の `{数字}.zip` をダウンロードしてください。

[![Build status](https://dev.azure.com/gooooon/sat-ci/_apis/build/status/sat-ci-.NET%20Desktop-CI-windows)](https://dev.azure.com/gooooon/sat-ci/_build/latest?definitionId=1)
