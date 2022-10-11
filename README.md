# SfcHelper

## 概要
- SXFのSFCファイルの読み込み及び保存を行うライブラリです。SFCファイルについては
http://www.cals-ed.go.jp/sxf_ver3-1_specification_draft/
を参照してください。
- SfcTestはこのライブラリを利用したビューアです。ただし、描画で面倒な部分（矢印、線種、ハッチング、塗り、文字間隔など）は実装していません。
- SfcTestにForm1.cs内のファイルのOpenFile()で読み込み、WriteFile()で保存を実装しています。また、InitDrawing()に図形の登録方法を記しています。
- SxfConst.csにSXF規格の各種最大値などが記されています。
- SFCファイルのエンコードはシフトJISですが、このライブラリ内部はUNICODEを使っています。SXFの文字数の最大値はシフトJISで定義されるので注意してください。このライブラリでは文字列の文字数をチェックしていません。
![画面イメージ](https://github.com/JinkiKeikaku/Images/blob/main/SfcHelper01.png)
