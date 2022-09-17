using Microsoft.Win32.SafeHandles;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Formats.Asn1.AsnWriter;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace SfcHelper
{


    //------------------------------------------------
    //	用紙
    //------------------------------------------------
    public class SxfSheet
    {
        public SxfSheet(string name, int paperType, int orient, int width, int height)
        {
            Name = name;
            PaperType = paperType;
            Orient = orient;
            Width = width;
            Height = height;
        }
        /// <summary>
        /// 図面名
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// 用紙サイズ種別
        /// </summary>
        public int PaperType { get; }
        /// <summary>
        /// 縦／横区分　0:縦 1:横
        /// </summary>
        public int Orient { get; }
        /// <summary>
        /// 自由用紙横長
        /// </summary>
        public int Width { get; }
        /// <summary>
        /// 自由用紙縦長
        /// </summary>
        public int Height { get; }

        public List<SxfShape> Shapes { get; } = new();

        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString("drawing_sheet_feature", Name, PaperType, Orient, Width, Height);
        }
    }

    //------------------------------------------------
    //	複合図形定義
    //------------------------------------------------
    public class SxfSfigOrg
    {
        /// <summary>複合図形名</summary>
        public string Name;

        /// <summary>複合図形種別フラグ</summary>
        public int Flag;

        public SxfSfigOrg(string name, int flag)
        {
            Name = name;
            Flag = flag;
        }

        public List<SxfShape> Shapes { get; } = new();

        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString("sfig_org_feature", Name, Flag);
        }
    }



    //------------------------------------------------
    //	レイヤ
    //------------------------------------------------
    public class SxfLayer
    {
        /// <summary>
        /// レイヤ名
        /// </summary>
        public string Name;
        /// <summary>
        /// 表示/非表示フラグ 0:非表示　1:表示
        /// </summary>
        public int Flag;

        public SxfLayer(string name, int lflag)
        {
            Name = name;
            Flag = lflag;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString("layer_feature", Name, Flag);
            //            return @$"layer_feature(\'{Name}\','{Flag}')";
        }
    }

    public class SxfLineType
    {
        public SxfLineType(string name, double[] pitch)
        {
            Name = name;
            Pitch = pitch;
        }

        /// <summary>
        /// 線種名
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// ピッチ線分の長さ＋空白長さの繰り返し
        /// </summary>
        public double[] Pitch { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString("user_defined_font_feature", Name, Pitch.Length, Pitch);
            //var a = string.Join(",", Pitch);
            //return @$"user_defined_font_feature(\'{Name}\','{Pitch.Length}','({a})')";
        }
    }

    //------------------------------------------------
    //	既定義線種
    //------------------------------------------------
    public class SxfPreDefinedLineType : SxfLineType
    {
        public SxfPreDefinedLineType(string name, double[] pitch) : base(name, pitch)
        {
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $@"pre_defined_font_feature(\'{Name}\')";
        }
    }



    public class SxfColor
    {
        public SxfColor(int r, int g, int b)
        {
            Red = r;
            Green = g;
            Blue = b;
        }
        public int Red { get; }
        public int Green { get; }
        public int Blue { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $@"user_defined_colour_feature('{Red}','{Green}','{Blue}')";
        }
    }

    //------------------------------------------------
    //	既定義色
    //------------------------------------------------
    public class SxfPreDefinedColor : SxfColor
    {
        /// <summary>
        /// 色名
        /// </summary>
        public string Name { get; }

        public SxfPreDefinedColor(string name, int r, int g, int b) : base(r, g, b)
        {
            Name = name;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $@"pre_defined_colour_feature(\'{Name}\')";
        }
    }

    ////------------------------------------------------
    ////	ユーザ定義色
    ////------------------------------------------------
    //public class SxfUserdefinedColour
    //{
    //    /* Ｒ値 */
    //    public int red;
    //    /* Ｇ値 */
    //    public int green;
    //    /* Ｂ値 */
    //    public int blue;
    //}

    //------------------------------------------------
    //	線幅
    //------------------------------------------------
    public class SxfLineWidth
    {
        /// <summary>
        /// 線幅
        /// </summary>
        public double Width { get; }

        public SxfLineWidth(double width)
        {
            Width = width;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $@"width_feature('{Width}')";
        }
    }

    //------------------------------------------------
    //	文字フォント
    //------------------------------------------------
    public class SxfTextFont
    {
        public SxfTextFont(string name)
        {
            Name = name;
        }

        /* 文字フォント名 */
        public string Name { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $@"text_font_feature(\'{Name}\')";
        }

    }

    //------------------------------------------------
    //	図面表題欄
    //------------------------------------------------
    public class SxfAttribute
    {
        /* 事業名 */
        public string p_name;
        /* 工事名 */
        public string c_name;
        /* 契約区分 */
        public string c_type;
        /* 図面名 */
        public string d_title;
        /* 図面番号 */
        public string p_number;
        /* 図面種別 */
        public string d_type;
        /* 尺度 */
        public string d_scale;
        /* 図面作成年(西暦) */
        public int d_year;
        /* 図面作成月(西暦) */
        public int d_month;
        /* 図面作成日(西暦) */
        public int d_day;
        /* 受注会社名 */
        public string c_contractor;
        /* 発注事業者名 */
        public string c_owner;
    }


    public class SxfShape
    {
        /// <summary> レイヤコード </summary>
        public int Layer;

        public SxfShape(int layer)
        {
            Layer = layer;
        }
    }

    /// <summary>
    /// 点マーカ
    /// </summary>
    public class SxfPointMarker : SxfShape
    {
        /// <summary>色コード</summary>
        public int Color;
        /// <summary>配置位置X座標</summary>
        public double StartX;
        /// <summary>配置位置Y座標</summary>
        public double StartY;
        /// <summary>マーカコード </summary>
        public int MarkerCode;
        /// <summary> 回転角 </summary>
        public double RotateAngle;
        /// <summary> 尺度 </summary>
        public double Scale;

        /// <summary>
        /// 点マーカ
        /// </summary>
        /// <param name="layer">レイヤコード</param>
        /// <param name="color">色コード</param>
        /// <param name="start_x">配置位置X座標</param>
        /// <param name="start_y">配置位置Y座標</param>
        /// <param name="marker_code">マーカコード</param>
        /// <param name="rotate_angle">回転角</param>
        /// <param name="scale">尺度</param>
        public SxfPointMarker(
            int layer, int color,
            double start_x, double start_y,
            int marker_code, double rotate_angle, double scale
        ) : base(layer)
        {
            Color = color;
            StartX = start_x;
            StartY = start_y;
            MarkerCode = marker_code;
            RotateAngle = rotate_angle;
            Scale = scale;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString("point_marker_feature", Layer, Color, StartX, StartY, MarkerCode, RotateAngle, Scale);
        }
    }

    //------------------------------------------------
    //	線分
    //------------------------------------------------
    public class SxfLineShape : SxfShape
    {
        /// <summary>色コード </summary>
        public int Color;
        /// <summary>線種コード</summary>
        public int LineType;
        /// <summary>線幅コード</summary>
        public int LineWidth;
        /// <summary>始点Ｘ座標</summary>
        public double StartX;
        /* 始点Ｙ座標 */
        /// <summary>始点Ｙ座標</summary>
        public double StartY;
        /// <summary>終点Ｘ座標</summary>
        public double EndX;
        /// <summary>終点Ｙ座標</summary>
        public double EndY;

        public SxfLineShape(
            int layer, int color, int type, int line_width,
            double start_x, double start_y, double end_x, double end_y
        ) : base(layer)
        {
            this.Color = color;
            this.LineType = type;
            this.LineWidth = line_width;
            this.StartX = start_x;
            this.StartY = start_y;
            this.EndX = end_x;
            this.EndY = end_y;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString("line_feature", Layer, Color, LineType, LineWidth, StartX, StartY, EndX, EndY);
            //            return $@"line_feature('{layer}','{Color}','{LineType}','{LineWidth}','{StartX}','{StartY}','{EndX}','{EndY}')";
        }
    }

    //------------------------------------------------
    //	折線
    //------------------------------------------------
    public class SxfPolylineShape : SxfShape
    {
        /// <summary>色コード </summary>
        public int Color;
        /// <summary>線種コード</summary>
        public int LineType;
        /// <summary>線幅コード</summary>
        public int LineWidth;
        /// <summary> 頂点 X </summary>
        public readonly List<double> VertexX = new();
        /// <summary> 頂点 Y </summary>
        public readonly List<double> VertexY = new();

        /// <summary>
        /// 折線。
        /// 頂点数は頂点数から自動設定とする。当然、XとYの頂点数は同じ必要がある。        
        /// </summary>
        /// <param name="layer">レイヤコード</param>
        /// <param name="color">色コード</param>
        /// <param name="lineType">線種コード</param>
        /// <param name="lineWidth">線幅コード</param>
        /// <param name="verteX">頂点 X 。頂点数はY座標と同数であること。</param>
        /// <param name="verteY">頂点 Y 。頂点数はX座標と同数であること。</param>
        public SxfPolylineShape(
            int layer, int color, int lineType, int lineWidth, IReadOnlyList<double> verteX, IReadOnlyList<double> verteY
        ) : base(layer)
        {
            Color = color;
            LineType = lineType;
            LineWidth = lineWidth;
            VertexX.AddRange(verteX);
            VertexY.AddRange(verteY);
        }

        /// <summary>
        /// 頂点を追加する。
        /// </summary>
        public void AddVertex(double x, double y)
        {
            VertexX.Add(x);
            VertexY.Add(y);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString("polyline_feature", Layer, Color, LineType, LineWidth, VertexX.Count, VertexX, VertexY);
        }
    }

    //------------------------------------------------
    //	円
    //------------------------------------------------
    public class SxfCircleShape : SxfShape
    {
        /// <summary>色コード </summary>
        public int Color;
        /// <summary>線種コード</summary>
        public int LineType;
        /// <summary>線幅コード</summary>
        public int LineWidth;
        /// <summary>中心X座標</summary>
        public double CenterX;
        /// <summary>中心Y座標</summary>
        public double CenterY;
        /// <summary>半径</summary>
        public double Radius;

        public SxfCircleShape(
            int layer, int color, int lineType, int lineWidth, double centerX, double centerY, double radius
        ) : base(layer)
        {
            Color = color;
            LineType = lineType;
            LineWidth = lineWidth;
            CenterX = centerX;
            CenterY = centerY;
            Radius = radius;
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString("circle_feature", Layer, Color, LineType, LineWidth, CenterX, CenterY, Radius);
        }
    }

    //------------------------------------------------
    //	円弧
    //------------------------------------------------
    class SxfArcShape : SxfShape
    {
        /// <summary>色コード </summary>
        public int Color;
        /// <summary>線種コード</summary>
        public int LineType;
        /// <summary>線幅コード</summary>
        public int LineWidth;
        /// <summary>中心X座標</summary>
        public double CenterX;
        /// <summary>中心Y座標</summary>
        public double CenterY;
        /// <summary>半径</summary>
        public double Radius;
        /// <summary>向きフラグ</summary>
        public int Direction;
        /// <summary>始角</summary>
        public double StartAngle;
        /// <summary>終角</summary>
        public double EndAngle;

        public SxfArcShape(
            int layer, int color, int lineType, int lineWidth,
            double centerX, double centerY, double radius, int direction, double startAngle, double endAngle
        ) : base(layer)
        {
            Color = color;
            LineType = lineType;
            LineWidth = lineWidth;
            CenterX = centerX;
            CenterY = centerY;
            Radius = radius;
            Direction = direction;
            StartAngle = startAngle;
            EndAngle = endAngle;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString(
                "arc_feature", Layer, Color, LineType, LineWidth, CenterX, CenterY, Radius, Direction, StartAngle, EndAngle
            );
        }
    }

    //------------------------------------------------
    //	楕円
    //------------------------------------------------
    class SxfEllipseShape : SxfShape
    {
        /// <summary>色コード </summary>
        public int Color;
        /// <summary>線種コード</summary>
        public int LineType;
        /// <summary>線幅コード</summary>
        public int LineWidth;
        /// <summary>中心X座標</summary>
        public double CenterX;
        /// <summary>中心Y座標</summary>
        public double CenterY;
        /// <summary>X方向半径</summary>
        public double RadiusX;
        /// <summary>Y方向半径</summary>
        public double RadiusY;
        /// <summary>回転角</summary>
        public double RotateAngle;

        public SxfEllipseShape(
            int layer, int color, int lineType, int lineWidth,
            double centerX, double centerY, double radiusX, double radiusY, double rotateAngle
        ) : base(layer)
        {
            Color = color;
            LineType = lineType;
            LineWidth = lineWidth;
            CenterX = centerX;
            CenterY = centerY;
            RadiusX = radiusX;
            RadiusY = radiusY;
            RotateAngle = rotateAngle;
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString(
                "ellipse_feature", Layer, Color, LineType, LineWidth, CenterX, CenterY, RadiusX, RadiusY, RotateAngle
            );
        }

    }

    //------------------------------------------------
    //	だ円弧
    //------------------------------------------------
    class SxfEllipseArcShape : SxfShape
    {
        /// <summary>色コード </summary>
        public int Color;
        /// <summary>線種コード</summary>
        public int LineType;
        /// <summary>線幅コード</summary>
        public int LineWidth;
        /// <summary>中心X座標</summary>
        public double CenterX;
        /// <summary>中心Y座標</summary>
        public double CenterY;
        /// <summary>X方向半径</summary>
        public double RadiusX;
        /// <summary>Y方向半径</summary>
        public double RadiusY;
        /// <summary>向きフラグ</summary>
        public int Direction;
        /// <summary>回転角</summary>
        public double RotateAngle;
        /// <summary>始角</summary>
        public double StartAngle;
        /// <summary>終角</summary>
        public double EndAngle;

        public SxfEllipseArcShape(
            int layer, int color, int lineType, int lineWidth,
            double centerX, double centerY, double radiusX, double radiusY,
            int direction, double rotateAngle, double startAngle, double endAngle
        ) : base(layer)
        {
            Color = color;
            LineType = lineType;
            LineWidth = lineWidth;
            CenterX = centerX;
            CenterY = centerY;
            RadiusX = radiusX;
            RadiusY = radiusY;
            Direction = direction;
            RotateAngle = rotateAngle;
            StartAngle = startAngle;
            EndAngle = endAngle;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString(
                "ellipse_arc_feature", Layer, Color, LineType, LineWidth,
                CenterX, CenterY, RadiusX, RadiusY, Direction, RotateAngle, StartAngle, EndAngle
            );
        }
    }

    //------------------------------------------------
    //	文字要素
    //------------------------------------------------
    class SxfTextShape : SxfShape
    {
        /* レイヤコード */
        public int laye;
        /* 色コード */
        public int Color;
        /* 文字フォントコード */
        public int Font;
        /* 文字列 */
        public string Str;
        /* 文字配置基点Ｘ座標 */
        public double TextX;
        /* 文字配置基点Ｙ座標 */
        public double TextY;
        /* 文字範囲高 */
        public double Height;
        /* 文字範囲幅 */
        public double Width;
        /* 文字間隔 */
        public double Spc;
        /* 文字回転角 */
        public double Angle;
        /* スラント角 */
        public double Slant;
        /* 文字配置基点 */
        public int BPnt;
        /* 文字書き出し方向 */
        public int Direct;

        public SxfTextShape(
            int layer, int color, int font, string str, double textX, double textY,
            double height, double width, double spc, double angle, double slant, int bPnt, int direct
        ) : base(layer)
        {
            Color = color;
            Font = font;
            Str = str;
            TextX = textX;
            TextY = textY;
            Height = height;
            Width = width;
            Spc = spc;
            Angle = angle;
            Slant = slant;
            BPnt = bPnt;
            Direct = direct;
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString(
                "text_string_feature", Layer, Color, Font, Str,
                TextX, TextY, Height, Width, Spc, Angle, Slant, BPnt, Direct
            );
        }
    }

    /// <summary>
    /// スプライン曲線(3 次のベジェ曲線)
    /// </summary>
    public class SxfSplineShape : SxfShape
    {
        /// <summary>色コード </summary>
        public int Color;
        /// <summary>線種コード</summary>
        public int LineType;
        /// <summary>線幅コード</summary>
        public int LineWidth;
        /// <summary> 
        /// 頂点 X 。頂点数はY座標と同数であること。
        /// </summary>
        public readonly List<double> VertexX = new();
        /// <summary> 
        /// 頂点 Y 。頂点数はX座標と同数であること。
        /// </summary>
        public readonly List<double> VertexY = new();

        /// <summary>
        /// スプライン曲線(3 次のベジェ曲線)。
        /// 仕様では開閉区分を示すopen_closeフラグがあるが、参考値でcloseの時は開始点と終了点が同じ座標
        /// である。そのため、open_closeフラグは座標値の開始点、終点をみて同じかどうかで自動設定する。
        /// 頂点数は頂点数から自動設定とする。当然、XとYの頂点数は同じ必要がある。        
        /// </summary>
        /// <param name="layer">レイヤコード</param>
        /// <param name="color">色コード</param>
        /// <param name="lineType">線種コード</param>
        /// <param name="lineWidth">線幅コード</param>
        /// <param name="verteX">頂点 X 。頂点数はY座標と同数であること。</param>
        /// <param name="verteY">頂点 Y 。頂点数はX座標と同数であること。</param>
        public SxfSplineShape(
            int layer, int color, int lineType, int lineWidth, IReadOnlyList<double> verteX, IReadOnlyList<double> verteY
        ) : base(layer)
        {
            Color = color;
            LineType = lineType;
            LineWidth = lineWidth;
            VertexX.AddRange(verteX);
            VertexY.AddRange(verteY);
        }

        /// <summary>
        /// 頂点を追加する。制御点も含むことに注意。
        /// </summary>
        public void AddVertex(double x, double y)
        {
            VertexX.Add(x);
            VertexY.Add(y);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            //flagはcloseの時0，openで1
            var flag = (VertexX[0] == VertexX[^1] && VertexY[0] == VertexY[^1]) ? 0 : 1;
            return Helper.MakeFeatureString("spline_feature", Layer, Color, LineType, LineWidth, flag, VertexX.Count, VertexX, VertexY);
        }
    };


    //------------------------------------------------
    //	クロソイド曲線
    //------------------------------------------------
    public class SxfClothoidShape : SxfShape
    {
        /// <summary>色コード </summary>
        public int Color;
        /// <summary>線種コード</summary>
        public int LineType;
        /// <summary>線幅コード</summary>
        public int LineWidth;
        /// <summary>配置基点Ｘ座標</summary>
        public double BaseX;
        /// <summary>配置基点Ｙ座標</summary>
        public double BaseY;
        /// <summary>クロソイドパラメータ</summary>
        public double Parameter;
        /// <summary>向きフラグ</summary>
        public int Direction;
        /// <summary>回転角</summary>
        public double Angle;
        /// <summary>開始曲線長</summary>
        public double StartLength;
        /// <summary>終了曲線長</summary>
        public double EndLength;

        public SxfClothoidShape(
            int layer, int color, int lineType, int lineWidth, double baseX, double baseY,
            double parameter, int direction, double angle, double startLength, double endLength
        ) : base(layer)
        {
            Color = color;
            LineType = lineType;
            LineWidth = lineWidth;
            BaseX = baseX;
            BaseY = baseY;
            Parameter = parameter;
            Direction = direction;
            Angle = angle;
            StartLength = startLength;
            EndLength = endLength;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString(
                "clothoid_feature", Layer, Color, LineType, LineWidth,
                BaseX, BaseY, Parameter, Direction, Angle, StartLength, EndLength);
        }
    }

    //------------------------------------------------
    //	複合図形配置
    //------------------------------------------------
    public class SxfSfiglocShape : SxfShape
    {
        /// <summary> 複合図形名 </summary>
        public string Name;
        /// <summary> 配置位置X座標 </summary>
        public double X;
        /// <summary> 配置位置Y座標 </summary>
        public double Y;
        /// <summary> 回転角 </summary>
        public double Angle;
        /// <summary> X方向尺度 </summary>
        public double RatioX;
        /// <summary> Y方向尺度 </summary>
        public double RatioY;

        public SxfSfiglocShape(
            int layer, string name, double x, double y, double angle, double ratio_x, double ratio_y
        ) : base(layer)
        {
            Name = name;
            X = x;
            Y = y;
            Angle = angle;
            RatioX = ratio_x;
            RatioY = ratio_y;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString("sfig_locate_feature", Layer, Name, X, Y, Angle, RatioX, RatioY);
        }


    }


    //------------------------------------------------
    //	既定義シンボル
    //------------------------------------------------
    public class SxfExternallyDefinedSymbolShape : SxfShape
    {
        /// <summary>色コードフラグ </summary>
        public int ColorFlag;
        /// <summary>色コード </summary>
        public int Color;
        /// <summary>シンボル名</summary>
        public string Name;
        /// <summary>配置位置X座標</summary>
        public double X;
        /// <summary>配置位置Y座標</summary>
        public double Y;
        /// <summary>回転角</summary>
        public double Angle;
        /// <summary>倍率</summary>
        public double Scale;

        public SxfExternallyDefinedSymbolShape(
            int layer, int colorFlag, int color, string name,
            double x, double y, double angle, double scale
        ) : base(layer)
        {
            ColorFlag = colorFlag;
            Color = color;
            Name = name;
            X = x;
            Y = y;
            Angle = angle;
            Scale = scale;
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString("externally_defined_symbol_feature", Layer, ColorFlag, Color, Name, X, Y, Angle, Scale);
        }

    }

    //------------------------------------------------
    //	直線寸法
    //------------------------------------------------
    public class SxfLinearDimShape : SxfShape
    {
        /// <summary>色コード </summary>
        public int Color;
        /// <summary>線種コード</summary>
        public int LineType;
        /// <summary>線幅コード</summary>
        public int LineWidth;
        /* 寸法線始点Ｘ座標 */
        public double SunX1;
        /* 寸法線始点Ｙ座標 */
        public double SunY1;
        /* 寸法線終点Ｘ座標 */
        public double SunX2;
        /* 寸法線終点Ｙ座標 */
        public double SunY2;
        /* 補助線１の有無フラグ(０：無、１：有) */
        public int Flag2;
        /* 補助線１基点Ｘ座標 */
        public double Ho1X0;
        /* 補助線１基点Ｙ座標 */
        public double Ho1Y0;
        /* 補助線１基点Ｘ座標 */
        public double Ho1X1;
        /* 補助線１基点Ｙ座標 */
        public double Ho1Y1;
        /* 補助線１基点Ｘ座標 */
        public double Ho1X2;
        /* 補助線１基点Ｙ座標 */
        public double Ho1Y2;
        /* 補助線２の有無フラグ(０：無、１：有) */
        public int Flag3;
        /* 補助線２基点Ｘ座標 */
        public double Ho2X0;
        /* 補助線２基点Ｙ座標 */
        public double Ho2Y0;
        /* 補助線２基点Ｘ座標 */
        public double Ho2X1;
        /* 補助線２基点Ｙ座標 */
        public double Ho2Y1;
        /* 補助線２基点Ｘ座標 */
        public double Ho2X2;
        /* 補助線２基点Ｙ座標 */
        public double Ho2Y2;
        /* 矢印１コード */
        public int Arr1Code1;
        /* 矢印１内外コード(0:なし 1:外向き 2:内向き) */
        public int Arr1Code2;
        /* 矢印１配置始点Ｘ座標 */
        public double Arr1X;
        /* 矢印１配置始点Ｙ座標 */
        public double Arr1Y;
        /* 矢印１配置倍率 */
        public double Arr1R;
        /* 矢印２コード */
        public int Arr2Code1;
        /* 矢印２内外コード(0:なし 1:外向き 2:内向き) */
        public int Arr2Code2;
        /* 矢印２配置始点Ｘ座標 */
        public double Arr2X;
        /* 矢印２配置始点Ｙ座標 */
        public double Arr2Y;
        /* 矢印２配置倍率 */
        public double Arr2R;
        /* 寸法値の有無フラグ(０：無、１：有) */
        public int Flag4;
        /// <summary>文字フォントコード</summary>
        public int Font;
        /// <summary>文字列</summary>
        public string Str;
        /// <summary>文字列配置基点Ｘ座標</summary>
        public double TextX;
        /// <summary>文字列配置基点Ｙ座標</summary>
        public double TextY;
        /// <summary>文字範囲高</summary>
        public double Height;
        /// <summary>文字範囲幅</summary>
        public double Width;
        /// <summary>文字間隔</summary>
        public double Spc;
        /// <summary>文字列回転角</summary>
        public double Angle;
        /// <summary>スラント角度</summary>
        public double Slant;
        /// <summary>
        /// 文字配置基点
        ///   １：左下、２：中下、３：右下、
        ///   ４：左中、５：中中、６：右中、
        ///   ７：左上、８：中上、９：右上 
        /// </summary>
        public int BPnt;
        /// <summary>文字書出し方向(１：横書き、２：縦書き)</summary>
        public int Direct;

        public SxfLinearDimShape(
            int layer, int color, int lineType, int lineWidth,
            double sunX1, double sunY1, double sunX2, double sunY2,
            int flag2, double ho1X0, double ho1Y0, double ho1X1, double ho1Y1, double ho1X2, double ho1Y2,
            int flag3, double ho2X0, double ho2Y0, double ho2X1, double ho2Y1, double ho2X2, double ho2Y2,
            int arr1Code1, int arr1Code2, double arr1X, double arr1Y, double arr1R,
            int arr2Code1, int arr2Code2, double arr2X, double arr2Y, double arr2R, int flag4,
            int font, string str, double textX, double textY, double height, double width,
            double spc, double angle, double slant, int bPnt, int direct
        ) : base(layer)
        {
            Color = color;
            LineType = lineType;
            LineWidth = lineWidth;
            SunX1 = sunX1;
            SunY1 = sunY1;
            SunX2 = sunX2;
            SunY2 = sunY2;
            Flag2 = flag2;
            Ho1X0 = ho1X0;
            Ho1Y0 = ho1Y0;
            Ho1X1 = ho1X1;
            Ho1Y1 = ho1Y1;
            Ho1X2 = ho1X2;
            Ho1Y2 = ho1Y2;
            Flag3 = flag3;
            Ho2X0 = ho2X0;
            Ho2Y0 = ho2Y0;
            Ho2X1 = ho2X1;
            Ho2Y1 = ho2Y1;
            Ho2X2 = ho2X2;
            Ho2Y2 = ho2Y2;
            Arr1Code1 = arr1Code1;
            Arr1Code2 = arr1Code2;
            Arr1X = arr1X;
            Arr1Y = arr1Y;
            Arr1R = arr1R;
            Arr2Code1 = arr2Code1;
            Arr2Code2 = arr2Code2;
            Arr2X = arr2X;
            Arr2Y = arr2Y;
            Arr2R = arr2R;
            Flag4 = flag4;
            Font = font;
            Str = str;
            TextX = textX;
            TextY = textY;
            Height = height;
            Width = width;
            Spc = spc;
            Angle = angle;
            Slant = slant;
            BPnt = bPnt;
            Direct = direct;
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString(
                "linear_dim_feature", Layer, Color, LineType, LineWidth, SunX1, SunY1, SunX2, SunY2, Flag2,
                Ho1X0, Ho1Y0, Ho1X1, Ho1Y1, Ho1X2, Ho1Y2, Flag3,
                Ho2X0, Ho2Y0, Ho2X1, Ho2Y1, Ho2X2, Ho2Y2,
                Arr1Code1, Arr1Code2, Arr1X, Arr1Y, Arr1R, Arr2Code1, Arr2Code2, Arr2X, Arr2Y, Arr2R,
                Flag4, Font, Str, TextX, TextY, Height, Width, Spc,
                Angle, Slant, BPnt, Direct);
        }
    }

    //------------------------------------------------
    //  弧長寸法
    //------------------------------------------------
    public class SxfCurveDimShape : SxfShape
    {
        /// <summary>色コード </summary>
        public int Color;
        /// <summary>線種コード</summary>
        public int LineType;
        /// <summary>線幅コード</summary>
        public int LineWidth;
        /// <summary>寸法線原点X座標</summary>
        public double SunX;
        /// <summary>寸法線原点Y座標</summary>
        public double SunY;
        /// <summary>寸法線半径</summary>
        public double SunRadius;
        /// <summary>寸法線始角</summary>
        public double SunAngle0;
        /// <summary>寸法線終角</summary>
        public double SunAngle1;
        /// <summary>補助線１の有無フラグ(0:無 1:有)</summary>
        public int Flag2;
        /// <summary>補助線１基点Ｘ座標</summary>
        public double Ho1X0;
        /// <summary>補助線１基点Ｙ座標</summary>
        public double Ho1Y0;
        /// <summary>補助線１始点Ｘ座標</summary>
        public double Ho1X1;
        /// <summary>補助線１始点Ｙ座標</summary>
        public double Ho1Y1;
        /// <summary>補助線１終点Ｘ座標</summary>
        public double Ho1X2;
        /// <summary>補助線１終点Ｙ座標</summary>
        public double Ho1Y2;
        /// <summary>補助線２の有無フラグ(0:無 1:有)</summary>
        public int Flag3;
        /// <summary>補助線２基点Ｘ座標</summary>
        public double Ho2X0;
        /// <summary>補助線２基点Ｙ座標</summary>
        public double Ho2Y0;
        /// <summary>補助線２始点Ｘ座標</summary>
        public double Ho2X1;
        /// <summary>補助線２始点Ｙ座標</summary>
        public double Ho2Y1;
        /// <summary>補助線２終点Ｘ座標</summary>
        public double Ho2X2;
        /// <summary>補助線２終点Ｙ座標</summary>
        public double Ho2Y2;
        /// <summary>矢印１コード</summary>
        public int Arr1Code1;
        /// <summary>矢印１内外コード</summary>
        public int Arr1Code2;
        /// <summary>矢印１配置点Ｘ座標</summary>
        public double Arr1X;
        /// <summary>矢印１配置点Ｙ座標</summary>
        public double Arr1Y;
        /// <summary>矢印１配置倍率</summary>
        public double Arr1R;
        /// <summary>矢印２コード</summary>
        public int Arr2Code1;
        /// <summary>矢印２内外コード</summary>
        public int Arr2Code2;
        /// <summary>矢印２配置点Ｘ座標</summary>
        public double Arr2X;
        /// <summary>矢印２配置点Ｙ座標</summary>
        public double Arr2Y;
        /// <summary>矢印２配置倍率</summary>
        public double Arr2R;
        /// <summary>寸法値の有無フラグ(０：無、１：有)</summary>
        public int Flag4;
        /// <summary>文字フォントコード</summary>
        public int Font;
        /// <summary>文字列</summary>
        public string Str;
        /// <summary>文字列配置基点Ｘ座標</summary>
        public double TextX;
        /// <summary>文字列配置基点Ｙ座標</summary>
        public double TextY;
        /// <summary>文字範囲高</summary>
        public double Height;
        /// <summary>文字範囲幅</summary>
        public double Width;
        /// <summary>文字間隔</summary>
        public double Spc;
        /// <summary>文字列回転角</summary>
        public double Angle;
        /// <summary>スラント角度</summary>
        public double Slant;
        /// <summary>
        /// 文字配置基点
        ///   １：左下、２：中下、３：右下、
        ///   ４：左中、５：中中、６：右中、
        ///   ７：左上、８：中上、９：右上 
        /// </summary>
        public int BPnt;
        /// <summary>文字書出し方向(１：横書き、２：縦書き)</summary>
        public int Direct;

        public SxfCurveDimShape(
            int layer, int color, int lineType, int lineWidth,
            double sunX, double sunY, double sunRadius, double sunAngle0, double sunAngle1,
            int flag2, double ho1X0, double ho1Y0, double ho1X1, double ho1Y1, double ho1X2, double ho1Y2,
            int flag3, double ho2X0, double ho2Y0, double ho2X1, double ho2Y1, double ho2X2, double ho2Y2,
            int arr1Code1, int arr1Code2, double arr1X, double arr1Y, double arr1R,
            int arr2Code1, int arr2Code2, double arr2X, double arr2Y, double arr2R, int flag4,
            int font, string str, double textX, double textY, double height, double width,
            double spc, double angle, double slant, int bPnt, int direct
        ) : base(layer)
        {
            Color = color;
            LineType = lineType;
            LineWidth = lineWidth;
            SunX = sunX;
            SunY = sunY;
            SunRadius = sunRadius;
            SunAngle0 = sunAngle0;
            SunAngle1 = sunAngle1;
            Flag2 = flag2;
            Ho1X0 = ho1X0;
            Ho1Y0 = ho1Y0;
            Ho1X1 = ho1X1;
            Ho1Y1 = ho1Y1;
            Ho1X2 = ho1X2;
            Ho1Y2 = ho1Y2;
            Flag3 = flag3;
            Ho2X0 = ho2X0;
            Ho2Y0 = ho2Y0;
            Ho2X1 = ho2X1;
            Ho2Y1 = ho2Y1;
            Ho2X2 = ho2X2;
            Ho2Y2 = ho2Y2;
            Arr1Code1 = arr1Code1;
            Arr1Code2 = arr1Code2;
            Arr1X = arr1X;
            Arr1Y = arr1Y;
            Arr1R = arr1R;
            Arr2Code1 = arr2Code1;
            Arr2Code2 = arr2Code2;
            Arr2X = arr2X;
            Arr2Y = arr2Y;
            Arr2R = arr2R;
            Flag4 = flag4;
            Font = font;
            Str = str;
            TextX = textX;
            TextY = textY;
            Height = height;
            Width = width;
            Spc = spc;
            Angle = angle;
            Slant = slant;
            BPnt = bPnt;
            Direct = direct;
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString(
                "curve_dim_feature", Layer, Color, LineType, LineWidth, SunX, SunY, SunRadius, SunAngle0,
                SunAngle1, Flag2, Ho1X0, Ho1Y0, Ho1X1, Ho1Y1,
                Ho1X2, Ho1Y2, Flag3, Ho2X0, Ho2Y0, Ho2X1, Ho2Y1,
                Ho2X2, Ho2Y2, Arr1Code1, Arr1Code2, Arr1X, Arr1Y, Arr1R, Arr2Code1, Arr2Code2,
                Arr2X, Arr2Y, Arr2R, Flag4, Font, Str, TextX, TextY, Height, Width, Spc,
                Angle, Slant, BPnt, Direct);
        }
    }

    //------------------------------------------------
    //  角度寸法
    //------------------------------------------------
    public class SxfAngularDimShape : SxfShape
    {
        public int layer;              /* レイヤコード */
        /// <summary>色コード </summary>
        public int Color;
        /// <summary>線種コード</summary>
        public int LineType;
        /// <summary>線幅コード</summary>
        public int LineWidth;
        /// <summary>寸法線原点X座標</summary>
        public double SunX;
        /// <summary>寸法線原点Y座標</summary>
        public double SunY;
        /// <summary>寸法線半径</summary>
        public double SunRadius;
        /// <summary>寸法線始角</summary>
        public double SunAngle0;
        /// <summary>寸法線終角</summary>
        public double SunAngle1;
        /// <summary>補助線１の有無フラグ(0:無 1:有)</summary>
        public int Flag2;

        /// <summary>補助線１基点Ｘ座標</summary>
        public double Ho1X0;
        /// <summary>補助線１基点Ｙ座標</summary>
        public double Ho1Y0;
        /// <summary>補助線１始点Ｘ座標</summary>
        public double Ho1X1;
        /// <summary>補助線１始点Ｙ座標</summary>
        public double Ho1Y1;
        /// <summary>補助線１終点Ｘ座標</summary>
        public double Ho1X2;
        /// <summary>補助線１終点Ｙ座標</summary>
        public double Ho1Y2;
        /// <summary>補助線２の有無フラグ(0:無 1:有)</summary>
        public int Flag3;
        /// <summary>補助線２基点Ｘ座標</summary>
        public double Ho2X0;
        /// <summary>補助線２基点Ｙ座標</summary>
        public double Ho2Y0;
        /// <summary>補助線２始点Ｘ座標</summary>
        public double Ho2X1;
        /// <summary>補助線２始点Ｙ座標</summary>
        public double Ho2Y1;
        /// <summary>補助線２終点Ｘ座標</summary>
        public double Ho2X2;
        /// <summary>補助線２終点Ｙ座標</summary>
        public double Ho2Y2;

        /// <summary>矢印１コード</summary>
        public int Arr1Code1;
        /// <summary>矢印１内外コード</summary>
        public int Arr1Code2;
        /// <summary>矢印１配置点Ｘ座標</summary>
        public double Arr1X;
        /// <summary>矢印１配置点Ｙ座標</summary>
        public double Arr1Y;
        /// <summary>矢印１配置倍率</summary>
        public double Arr1R;
        /// <summary>矢印２コード</summary>
        public int Arr2Code1;
        /// <summary>矢印２内外コード</summary>
        public int Arr2Code2;
        /// <summary>矢印２配置点Ｘ座標</summary>
        public double Arr2X;
        /// <summary>矢印２配置点Ｙ座標</summary>
        public double Arr2Y;
        /// <summary>矢印２配置倍率</summary>
        public double Arr2R;

        /// <summary>寸法値の有無フラグ(０：無、１：有)</summary>
        public int Flag4;
        /// <summary>文字フォントコード</summary>
        public int Font;
        /// <summary>文字列</summary>
        public string Str;
        /// <summary>文字列配置基点Ｘ座標</summary>
        public double TextX;
        /// <summary>文字列配置基点Ｙ座標</summary>
        public double TextY;
        /// <summary>文字範囲高</summary>
        public double Height;
        /// <summary>文字範囲幅</summary>
        public double Width;
        /// <summary>文字間隔</summary>
        public double Spc;
        /// <summary>文字列回転角</summary>
        public double Angle;
        /// <summary>スラント角度</summary>
        public double Slant;

        /// <summary>
        /// 文字配置基点
        ///   １：左下、２：中下、３：右下、
        ///   ４：左中、５：中中、６：右中、
        ///   ７：左上、８：中上、９：右上 
        /// </summary>
        public int BPnt;
        /// <summary>文字書出し方向(１：横書き、２：縦書き)</summary>
        public int Direct;

        public SxfAngularDimShape(
            int layer, int color, int lineType, int lineWidth,
            double sunX, double sunY, double sunRadius, double sunAngle0, double sunAngle1,
            int flag2, double ho1X0, double ho1Y0, double ho1X1, double ho1Y1, double ho1X2, double ho1Y2,
            int flag3, double ho2X0, double ho2Y0, double ho2X1, double ho2Y1, double ho2X2, double ho2Y2,
            int arr1Code1, int arr1Code2, double arr1X, double arr1Y, double arr1R,
            int arr2Code1, int arr2Code2, double arr2X, double arr2Y, double arr2R, int flag4,
            int font, string str, double textX, double textY, double height, double width,
            double spc, double angle, double slant, int bPnt, int direct
        ) : base(layer)
        {
            Color = color;
            LineType = lineType;
            LineWidth = lineWidth;
            SunX = sunX;
            SunY = sunY;
            SunRadius = sunRadius;
            SunAngle0 = sunAngle0;
            SunAngle1 = sunAngle1;
            Flag2 = flag2;
            Ho1X0 = ho1X0;
            Ho1Y0 = ho1Y0;
            Ho1X1 = ho1X1;
            Ho1Y1 = ho1Y1;
            Ho1X2 = ho1X2;
            Ho1Y2 = ho1Y2;
            Flag3 = flag3;
            Ho2X0 = ho2X0;
            Ho2Y0 = ho2Y0;
            Ho2X1 = ho2X1;
            Ho2Y1 = ho2Y1;
            Ho2X2 = ho2X2;
            Ho2Y2 = ho2Y2;
            Arr1Code1 = arr1Code1;
            Arr1Code2 = arr1Code2;
            Arr1X = arr1X;
            Arr1Y = arr1Y;
            Arr1R = arr1R;
            Arr2Code1 = arr2Code1;
            Arr2Code2 = arr2Code2;
            Arr2X = arr2X;
            Arr2Y = arr2Y;
            Arr2R = arr2R;
            Flag4 = flag4;
            Font = font;
            Str = str;
            TextX = textX;
            TextY = textY;
            Height = height;
            Width = width;
            Spc = spc;
            Angle = angle;
            Slant = slant;
            BPnt = bPnt;
            Direct = direct;
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString(
                "angular_dim_feature", Layer, Color, LineType, LineWidth, SunX, SunY, SunRadius, SunAngle0,
                SunAngle1, Flag2, Ho1X0, Ho1Y0, Ho1X1, Ho1Y1,
                Ho1X2, Ho1Y2, Flag3, Ho2X0, Ho2Y0, Ho2X1, Ho2Y1,
                Ho2X2, Ho2Y2, Arr1Code1, Arr1Code2, Arr1X, Arr1Y, Arr1R, Arr2Code1, Arr2Code2,
                Arr2X, Arr2Y, Arr2R, Flag4, Font, Str, TextX, TextY, Height, Width, Spc,
                Angle, Slant, BPnt, Direct);
        }

    }

    //------------------------------------------------
    //  半径寸法
    //------------------------------------------------
    public class SxfRadiusDimShape : SxfShape
    {
        /// <summary>色コード </summary>
        public int Color;
        /// <summary>線種コード</summary>
        public int LineType;
        /// <summary>線幅コード</summary>
        public int LineWidth;

        /* 寸法線始点Ｘ座標 */
        public double SunX1;
        /* 寸法線始点Ｙ座標 */
        public double SunY1;
        /* 寸法線終点Ｘ座標 */
        public double SunX2;
        /* 寸法線終点Ｙ座標 */
        public double SunY2;

        /// <summary>矢印コード</summary>
        public int ArrCode1;
        /// <summary>矢印内外コード</summary>
        public int ArrCode2;

        /// <summary>矢印配置点Ｘ座標</summary>
        public double ArrX;
        /// <summary>矢印配置点Ｙ座標</summary>
        public double ArrY;
        /// <summary>矢印配置倍率</summary>
        public double ArrR;

        /// <summary>寸法値の有無フラグ(０：無、１：有)</summary>
        public int Flag;
        /// <summary>文字フォントコード</summary>
        public int Font;
        /// <summary>文字列</summary>
        public string Str;
        /// <summary>文字列配置基点Ｘ座標</summary>
        public double TextX;
        /// <summary>文字列配置基点Ｙ座標</summary>
        public double TextY;
        /// <summary>文字範囲高</summary>
        public double Height;
        /// <summary>文字範囲幅</summary>
        public double Width;
        /// <summary>文字間隔</summary>
        public double Spc;
        /// <summary>文字列回転角</summary>
        public double Angle;
        /// <summary>スラント角度</summary>
        public double Slant;
        /// <summary>
        /// 文字配置基点
        ///   １：左下、２：中下、３：右下、
        ///   ４：左中、５：中中、６：右中、
        ///   ７：左上、８：中上、９：右上 
        /// </summary>
        public int BPnt;
        /// <summary>文字書出し方向(１：横書き、２：縦書き)</summary>
        public int Direct;

        public SxfRadiusDimShape(
            int layer, int color, int lineType, int lineWidth,
            double sunX1, double sunY1, double sunX2, double sunY2, int arrCode1, int arrCode2,
            double arrX, double arrY, double arrR, int flag, int font, string str, double textX, double textY,
            double height, double width, double spc, double angle, double slant, int bPnt, int direct
        ) : base(layer)
        {
            Color = color;
            LineType = lineType;
            LineWidth = lineWidth;
            SunX1 = sunX1;
            SunY1 = sunY1;
            SunX2 = sunX2;
            SunY2 = sunY2;
            ArrCode1 = arrCode1;
            ArrCode2 = arrCode2;
            ArrX = arrX;
            ArrY = arrY;
            ArrR = arrR;
            Flag = flag;
            Font = font;
            Str = str;
            TextX = textX;
            TextY = textY;
            Height = height;
            Width = width;
            Spc = spc;
            Angle = angle;
            Slant = slant;
            BPnt = bPnt;
            Direct = direct;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString(
                "radius_dim_feature", Layer, Color, LineType, LineWidth, SunX1, SunY1, SunX2, SunY2, ArrCode1, ArrCode2, 
                ArrX, ArrY, ArrR, Flag, Font, Str, TextX, TextY, Height, Width, Spc,Angle, Slant, BPnt, Direct);
        }

    }

    //------------------------------------------------
    //  直径寸法
    //------------------------------------------------
    public class SxfDiameterDimShape : SxfShape
    {
        /// <summary>色コード </summary>
        public int Color;
        /// <summary>線種コード</summary>
        public int LineType;
        /// <summary>線幅コード</summary>
        public int LineWidth;

        /* 寸法線始点Ｘ座標 */
        public double SunX1;
        /* 寸法線始点Ｙ座標 */
        public double SunY1;
        /* 寸法線終点Ｘ座標 */
        public double SunX2;
        /* 寸法線終点Ｙ座標 */
        public double SunY2;

        /// <summary>矢印１コード</summary>
        public int Arr1Code1;
        /// <summary>矢印１内外コード</summary>
        public int Arr1Code2;
        /// <summary>矢印１配置点Ｘ座標</summary>
        public double Arr1X;
        /// <summary>矢印１配置点Ｙ座標</summary>
        public double Arr1Y;
        /// <summary>矢印１配置倍率</summary>
        public double Arr1R;
        /// <summary>矢印２コード</summary>
        public int Arr2Code1;
        /// <summary>矢印２内外コード</summary>
        public int Arr2Code2;
        /// <summary>矢印２配置点Ｘ座標</summary>
        public double Arr2X;
        /// <summary>矢印２配置点Ｙ座標</summary>
        public double Arr2Y;
        /// <summary>矢印２配置倍率</summary>
        public double Arr2R;

        /// <summary>寸法値の有無フラグ(０：無、１：有)</summary>
        public int Flag;
        /// <summary>文字フォントコード</summary>
        public int Font;
        /// <summary>文字列</summary>
        public string Str;
        /// <summary>文字列配置基点Ｘ座標</summary>
        public double TextX;
        /// <summary>文字列配置基点Ｙ座標</summary>
        public double TextY;
        /// <summary>文字範囲高</summary>
        public double Height;
        /// <summary>文字範囲幅</summary>
        public double Width;
        /// <summary>文字間隔</summary>
        public double Spc;
        /// <summary>文字列回転角</summary>
        public double Angle;
        /// <summary>スラント角度</summary>
        public double Slant;
        /// <summary>
        /// 文字配置基点
        ///   １：左下、２：中下、３：右下、
        ///   ４：左中、５：中中、６：右中、
        ///   ７：左上、８：中上、９：右上 
        /// </summary>
        public int BPnt;
        /// <summary>文字書出し方向(１：横書き、２：縦書き)</summary>
        public int Direct;

        public SxfDiameterDimShape(
            int layer, int color, int lineType, int lineWidth, 
            double sunX1, double sunY1, double sunX2, double sunY2, 
            int arr1Code1, int arr1Code2, double arr1X, double arr1Y, double arr1R, 
            int arr2Code1, int arr2Code2, double arr2X, double arr2Y, double arr2R, int flag, 
            int font, string str, double textX, double textY, double height, double width, 
            double spc, double angle, double slant, int bPnt, int direct
        ) : base(layer)
        {
            Color = color;
            LineType = lineType;
            LineWidth = lineWidth;
            SunX1 = sunX1;
            SunY1 = sunY1;
            SunX2 = sunX2;
            SunY2 = sunY2;
            Arr1Code1 = arr1Code1;
            Arr1Code2 = arr1Code2;
            Arr1X = arr1X;
            Arr1Y = arr1Y;
            Arr1R = arr1R;
            Arr2Code1 = arr2Code1;
            Arr2Code2 = arr2Code2;
            Arr2X = arr2X;
            Arr2Y = arr2Y;
            Arr2R = arr2R;
            Flag = flag;
            Font = font;
            Str = str;
            TextX = textX;
            TextY = textY;
            Height = height;
            Width = width;
            Spc = spc;
            Angle = angle;
            Slant = slant;
            BPnt = bPnt;
            Direct = direct;
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString(
                "diameter_dim_feature", Layer, Color, LineType, LineWidth, SunX1, SunY1, SunX2, SunY2, 
                Arr1Code1, Arr1Code2, Arr1X, Arr1Y, Arr1R, Arr2Code1, Arr2Code2, Arr2X, Arr2Y, Arr2R, Flag, 
                Font, Str, TextX, TextY, Height, Width, Spc, Angle, Slant, BPnt, Direct);
        }
    }


    //------------------------------------------------
    //  引出し線
    //------------------------------------------------
    public class SxfLabelShape : SxfShape
    {
        /// <summary>色コード </summary>
        public int Color;
        /// <summary>線種コード</summary>
        public int LineType;
        /// <summary>線幅コード</summary>
        public int LineWidth;

        /// <summary> 頂点 X </summary>
        public readonly List<double> VertexX = new();
        /// <summary> 頂点 Y </summary>
        public readonly List<double> VertexY = new();

        /// <summary>矢印コード</summary>
        public int ArrCode;
        /// <summary>矢印１配置倍率</summary>
        public double ArrR;

        /// <summary>寸法値の有無フラグ(０：無、１：有)</summary>
        public int Flag;
        /// <summary>文字フォントコード</summary>
        public int Font;
        /// <summary>文字列</summary>
        public string Str;
        /// <summary>文字列配置基点Ｘ座標</summary>
        public double TextX;
        /// <summary>文字列配置基点Ｙ座標</summary>
        public double TextY;
        /// <summary>文字範囲高</summary>
        public double Height;
        /// <summary>文字範囲幅</summary>
        public double Width;
        /// <summary>文字間隔</summary>
        public double Spc;
        /// <summary>文字列回転角</summary>
        public double Angle;
        /// <summary>スラント角度</summary>
        public double Slant;
        /// <summary>
        /// 文字配置基点
        ///   １：左下、２：中下、３：右下、
        ///   ４：左中、５：中中、６：右中、
        ///   ７：左上、８：中上、９：右上 
        /// </summary>
        public int BPnt;
        /// <summary>文字書出し方向(１：横書き、２：縦書き)</summary>
        public int Direct;

        public SxfLabelShape(
            int layer, int color, int lineType, int lineWidth,
            IReadOnlyList<double> vertexX, IReadOnlyList<double> vertexY, 
            int arrCode, double arrR, int flag, 
            int font, string str, double textX, double textY, double height, double width, 
            double spc, double angle, double slant, int bPnt, int direct
        ) : base(layer)
        {
            Color = color;
            LineType = lineType;
            LineWidth = lineWidth;
            ArrCode = arrCode;
            ArrR = arrR;
            Flag = flag;
            Font = font;
            Str = str;
            TextX = textX;
            TextY = textY;
            Height = height;
            Width = width;
            Spc = spc;
            Angle = angle;
            Slant = slant;
            BPnt = bPnt;
            Direct = direct;
            VertexX.AddRange(vertexX);
            VertexY.AddRange(vertexY);
        }
        /// <summary>
        /// 頂点を追加する。
        /// </summary>
        public void AddVertex(double x, double y)
        {
            VertexX.Add(x);
            VertexY.Add(y);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString(
                "label_feature", Layer, Color, LineType, LineWidth, 
                VertexX.Count, VertexX, VertexY,
                ArrCode, ArrR, Flag,
                Font, Str, TextX, TextY, Height, Width, Spc, Angle, Slant, BPnt, Direct);
        }
    }

//------------------------------------------------
//  バルーン
//------------------------------------------------
public class SxfBalloonShape : SxfShape
    {
        /// <summary>色コード </summary>
        public int Color;
        /// <summary>線種コード</summary>
        public int LineType;
        /// <summary>線幅コード</summary>
        public int LineWidth;

        /// <summary> 頂点 X </summary>
        public readonly List<double> VertexX = new();
        /// <summary> 頂点 Y </summary>
        public readonly List<double> VertexY = new();

        /// <summary>中心X座標</summary>
        public double CenterX;
        /// <summary>中心Y座標</summary>
        public double CenterY;
        /// <summary>半径</summary>
        public double Radius;

        /// <summary>矢印コード</summary>
        public int ArrCode;
        /// <summary>矢印１配置倍率</summary>
        public double ArrR;

        /// <summary>寸法値の有無フラグ(０：無、１：有)</summary>
        public int Flag;
        /// <summary>文字フォントコード</summary>
        public int Font;
        /// <summary>文字列</summary>
        public string Str;
        /// <summary>文字列配置基点Ｘ座標</summary>
        public double TextX;
        /// <summary>文字列配置基点Ｙ座標</summary>
        public double TextY;
        /// <summary>文字範囲高</summary>
        public double Height;
        /// <summary>文字範囲幅</summary>
        public double Width;
        /// <summary>文字間隔</summary>
        public double Spc;
        /// <summary>文字列回転角</summary>
        public double Angle;
        /// <summary>スラント角度</summary>
        public double Slant;
        /// <summary>
        /// 文字配置基点
        ///   １：左下、２：中下、３：右下、
        ///   ４：左中、５：中中、６：右中、
        ///   ７：左上、８：中上、９：右上 
        /// </summary>
        public int BPnt;
        /// <summary>文字書出し方向(１：横書き、２：縦書き)</summary>
        public int Direct;

        public SxfBalloonShape(
            int layer, int color, int lineType, int lineWidth,
            IReadOnlyList<double> vertexX, IReadOnlyList<double> vertexY, double centerX, double centerY, double radius, 
            int arrCode, double arrR, int flag, 
            int font, string str, double textX, double textY, double height, double width, 
            double spc, double angle, double slant, int bPnt, int direct
        ):base(layer)
        {
            Color = color;
            LineType = lineType;
            LineWidth = lineWidth;
            VertexX.AddRange(vertexX);
            VertexY.AddRange(vertexY);
            CenterX = centerX;
            CenterY = centerY;
            Radius = radius;
            ArrCode = arrCode;
            ArrR = arrR;
            Flag = flag;
            Font = font;
            Str = str;
            TextX = textX;
            TextY = textY;
            Height = height;
            Width = width;
            Spc = spc;
            Angle = angle;
            Slant = slant;
            BPnt = bPnt;
            Direct = direct;
        }
        /// <summary>
        /// 頂点を追加する。
        /// </summary>
        public void AddVertex(double x, double y)
        {
            VertexX.Add(x);
            VertexY.Add(y);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString(
                "balloon_feature", Layer, Color, LineType, LineWidth,
                VertexX.Count, VertexX, VertexY, CenterX, CenterY, Radius,
                ArrCode, ArrR, Flag,
                Font, Str, TextX, TextY, Height, Width, Spc, Angle, Slant, BPnt, Direct);
        }
    }

    //------------------------------------------------
    //  ハッチング(既定義(外部定義)
    //------------------------------------------------
    class SxfExternallyDefinedHatchShape : SxfShape
    {
        /// <summary>ハッチング名</summary>
        public string Name;
        /// <summary>外形の複合曲線のフィーチャコード</summary>
        public int OutId;
        //        int number;                     /* 中抜きの閉領域数 */
        /// <summary>中抜きの複合曲線のフィーチャコード</summary>
        public readonly List<int> InId = new();

        public SxfExternallyDefinedHatchShape(int layer, string name, int outId, IReadOnlyList<int> inId):base(layer)
        {
            Name = name;
            OutId = outId;
            InId.AddRange(inId);
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString(
                "externally_defined_hatch_feature", Layer, OutId, InId.Count, InId);
        }
    }

    //------------------------------------------------
    //  ハッチング(塗り)
    //------------------------------------------------
    class SxfFillAreaStyleColorShape : SxfShape
    {
        /// <summary>色コード </summary>
        public int Color;
        /// <summary>外形の複合曲線のフィーチャコード</summary>
        public int OutId;
        //        int number;                     /* 中抜きの閉領域数 */
        /// <summary>中抜きの複合曲線のフィーチャコード</summary>
        public readonly List<int> InId = new();

        public SxfFillAreaStyleColorShape(int layer, int color, int outId, IReadOnlyList<int> inId) : base(layer)
        {
            Color = color;
            OutId = outId;
            InId.AddRange(inId);
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString(
                "fill_area_style_colour_feature", Layer, Color, OutId, InId.Count, InId);
        }
    }

    class HatchingPattern
    {
        /// <summary>ハッチング線の色コード</summary>
        public int Color;
        /// <summary>ハッチング線の線種コード</summary>
        public int LineType;
        /// <summary>ハッチング線の線幅コード</summary>
        public int LineWidth;
        /// <summary>ハッチング線のパターン開始点X座標</summary>
        public double StartX;
        /// <summary>ハッチング線のパターン開始点Y座標</summary>
        public double StartY;
        /// <summary>ハッチング間隔</summary>
        public double Spacing;
        /// <summary>ハッチング線の角度</summary>
        public double Angle;

        public HatchingPattern(int color, int lineType, int lineWidth, double startX, double startY, double spacing, double angle)
        {
            Color = color;
            LineType = lineType;
            LineWidth = lineWidth;
            StartX = startX;
            StartY = startY;
            Spacing = spacing;
            Angle = angle;
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"({Color},{LineType}, {LineWidth}, {StartX}, {StartY}, {Spacing}, {Angle})";
        }
    }

    //------------------------------------------------
    //  ハッチング(ユーザ定義)
    //------------------------------------------------
    class SxfFillAreaStyleHatchingShape : SxfShape
    {
        /// <summary>ハッチング線のパターン数</summary>
        public readonly List<HatchingPattern> HatchingPatternList = new();
        /// <summary>外形の複合曲線のフィーチャコード</summary>
        public int OutId;
        public readonly List<int> InId = new();

        public SxfFillAreaStyleHatchingShape(
            int layer, IReadOnlyList<HatchingPattern> hatchingPatternList, int outId, List<int> inId):base(layer)
        {
            if (hatchingPatternList.Count > SxfConst.MaxHatchNumber) {
                throw new Exception($"SxfFillAreaStyleHatchingShape::paatern size > {SxfConst.MaxHatchNumber}");
            }
            HatchingPatternList.AddRange(hatchingPatternList);
            OutId = outId;
            InId = inId;
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            var b = HatchingPatternList.Select(x => $"'{x}'");

            var s = string.Join(',', b);
            var a = string.Join(",", InId);

            return $"fill_area_style_hatching_feature('{Layer}','{HatchingPatternList.Count}',{s},'{OutId}','({a})')";
        }
    }

    //------------------------------------------------
    //  ハッチング(パターン)
    //------------------------------------------------
    class SxfFillAreaStyleTilesShape : SxfShape
    {
        int layer;                              /* レイヤコード */
        /// <summary>既定義シンボル名</summary>
        public string Name;
        /// <summary>ハッチパターンの色コード</summary>
        public int Color;                        /*  */
        /// <summary>ハッチパターン配置位置X座標</summary>
        public double PatternX;
        /// <summary>ハッチパターン配置位置Y座標</summary>
        public double PatternY;
        /// <summary>ハッチパターンの繰り返しベクトル１の大きさ</summary>
        public double PatternVector1;           /* ハッチパターンの繰り返しベクトル１の大きさ */
        /// <summary>ハッチパターンの繰り返しベクトル１の角度</summary>
        public double PatternVector1Angle;
        /// <summary>ハッチパターンの繰り返しベクトル２の大きさ</summary>
        public double PatternVector2;
        /// <summary>ハッチパターンの繰り返しベクトル２の角度</summary>
        public double PatternVector2Angle;
        /// <summary>ハッチパターンのX尺度</summary>
        public double PatternScaleX;
        /// <summary>ハッチパターンのY尺度</summary>
        public double PatternScaleY;
        /// <summary>ハッチパターンの向きの角度</summary>
        public double PatternAngle;
        /// <summary>外形の複合曲線のフィーチャコード</summary>
        public int OutId;
        /// <summary>中抜きの複合曲線のフィーチャコード</summary>
        public readonly List<int> InId = new();

        public SxfFillAreaStyleTilesShape(
            int layer, string name, int color, double patternX, double patternY, 
            double patternVector1, double patternVector1Angle, double patternVector2, double patternVector2Angle, 
            double patternScaleX, double patternScaleY, double patternAngle, int outId, List<int> inId
        ) : base(layer)
        {
            Name = name;
            Color = color;
            PatternX = patternX;
            PatternY = patternY;
            PatternVector1 = patternVector1;
            PatternVector1Angle = patternVector1Angle;
            PatternVector2 = patternVector2;
            PatternVector2Angle = patternVector2Angle;
            PatternScaleX = patternScaleX;
            PatternScaleY = patternScaleY;
            PatternAngle = patternAngle;
            OutId = outId;
            InId = inId;
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString(
                "fill_area_style_tiles_feature", Layer, Name, Color, PatternX, PatternY, 
                PatternVector1, PatternVector1Angle, PatternVector2, PatternVector2Angle,
                PatternScaleX, PatternScaleY, PatternAngle,
                OutId, InId.Count, InId);
        }
    }

    //------------------------------------------------
    //  複合曲線定義
    //------------------------------------------------
    public class SxfCcurveOrg
    {
        /// <summary>色コード </summary>
        public int Color;
        /// <summary>線種コード</summary>
        public int LineType;
        /// <summary>線幅コード</summary>
        public int LineWidth;
        /// <summary>表示/非表示フラグ</summary>
        public int Flag;

        public SxfCcurveOrg(int color, int lineType, int lineWidth, int flag)
        {
            Color = color;
            LineType = lineType;
            LineWidth = lineWidth;
            Flag = flag;
        }
    }

    //------------------------------------------------
    //  寸法線用Terminator Symbol
    //------------------------------------------------
    public class SxfTermSymbol
    {
        public int target_ID;          /* 矢印インスタンスＩＤ */
        public int flag;               /* 矢印フラグ */
        public int color;              /* 色コード */
        public int code;               /* 矢印コード */
        public double direction_x;     /* Ｘ方向ベクトル */
        public double direction_y;     /* Ｙ方向ベクトル */
        public double x;               /* 矢印配置点Ｘ座標 */
        public double y;               /* 矢印配置点Ｙ座標 */
        public double scale;           /* 矢印配置倍率 */
    }

    //------------------------------------------------
    //  寸法線用Projection Line
    //------------------------------------------------
    public class SxfProjLine
    {
        public int target_ID;  /* 補助線インスタンスＩＤ */
        public double ho_x0;   /* 補助線基点Ｘ座標 */
        public double ho_y0;   /* 補助線基点Ｙ座標 */
        public double ho_x1;   /* 補助線始点Ｘ座標 */
        public double ho_y1;   /* 補助線始点Ｙ座標 */
        public double ho_x2;   /* 補助線終点Ｘ座標 */
        public double ho_y2;   /* 補助線終点Ｙ座標 */
    }
}

