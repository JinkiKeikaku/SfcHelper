using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SfcHelper
{

    public static class SxfConst
    {
        //
        public const int MaxLayer = 256;				/*最大レイヤ数*/
        public const int MaxPreLineType = 16;			/*最大既定義線種数*/
        public const int MaxPreColor = 16;    		/*最大既定義色数*/
        public const int MaxPreLineWidth = 9; 			/*最大既定義線幅数*/
        public const int MaxLineWidth = 16; 			/*最大線幅数(既定義含む)*/
        public const int MaxTextFont = 1024;			/*最大文字フォント数*/

        public const int MaxUserLineType = 16;			/*最大ユーザ定義線種数*/
        public const int MaxUserColor = 240;			/*最大ユーザ定義色数*/
        public const int MaxColor = 256;				/*最大色数*/
        public const int MaxLineType = 32;				/*最大線種数*/

        //最大ピッチ数
        public const int MaxPitch = 8;				/*最大ピッチ数*/
        //最大RGB値
        public const int MaxRGB = 255;				/*最大RGB値*/
        //最大セグメント数
        public const int MaxSegment = 8;
        //最小セグメント数
        public const int MinSegment = 2;

        //double型の上限と下限
        public const double MinDouble = -1000000000000000.0;	/*double下限*/
        public const double MaxDouble = 1000000000000000.0;	/*double上限*/

        public const double LmtChkDouble = 5.0e-015;			/**/
        public const double MinChkDouble = 4.0e-015;			/*double下限*/
        public const double MaxChkDouble = 999999999999999.5;	/*double上限*/

        //名前の最大名称長(Cではバッファサイズで\0含めての数。)
        public const int MaxSheetName = 257;				/*図面名の最大名称長*/
        public const int MaxLayerName2 = 257;				/*レイヤ名の最大名称長(レベル2)、レベル１は32文字*/
        public const int MaxLineTypeName = 257;			/*線種名の最大名称長*/
        public const int MaxColourName = 257;				/*色名の最大名称長*/
        public const int MaxTextFontName = 257;				/*文字フォント名の最大名称長*/
        public const int MaxFigureName = 257;				/*複合図形名の最大名称長*/
        public const int MaxSymbolName = 257;				/*既定義シンボル名の最大名称長*/
        public const int MaxHatchName = 257;				/*ハッチング名の最大名称長*/
        public const int MaxPreSymbolName = 257;			/*既定義シンボル名（ハッチングパターンに使用）の最大名称長*/
        public const int MaxText = 257;				/*最大文字数*/

        //
        public const int MaxHatchNumber = 4;				/*ハッチング線の最大パターン数*/

        //角度の上限と下限
        public const double MaxAngle = 360.0;
        public const double MinAngle = 0.0;

        //スラント角度の上限と下限
        public const double MaxSlant = 85.0;
        public const double MinSlant = -85.0;

        //ヘッダ情報(ファイル名など)をUNICODEに変換後の最大文字数
        public const int MaxUnicodeName = 1280;

    }
}
