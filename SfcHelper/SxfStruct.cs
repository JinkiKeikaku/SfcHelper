using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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

        public override string ToString()
        {
            return @$"drawing_sheet_feature(\'{Name}\','{PaperType}','{Orient}','{Width}','{Height}')";
        }
    }

    //------------------------------------------------
    //	複合図形定義
    //------------------------------------------------
    public class SxfSfigorg
    {
        /* 複合図形名 */
        public string name;
        /* 複合図形種別フラグ */
        public int flag;
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
        public override string ToString()
        {
            return @$"layer_feature(\'{Name}\','{Flag}')";
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

        public override string ToString()
        {
            var a = string.Join(",", Pitch);
            return @$"user_defined_font_feature(\'{Name}\','{Pitch.Length}','({a})')";
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
        /// <summary>
        /// レイヤコード 
        /// </summary>
        public int layer;

        public SxfShape(int layer)
        {
            this.layer = layer;
        }
    }

    //------------------------------------------------
    //	複合図形配置
    //------------------------------------------------
    public class SxfSfiglocShape : SxfShape
    {
        /* 複合図形名 */
        public string name;
        public double x;                   /* 配置位置X座標 */
        public double y;                   /* 配置位置Y座標 */
        public double angle;               /* 回転角 */
        public double ratio_x;             /* X方向尺度 */
        public double ratio_y;             /* Y方向尺度 */

        public SxfSfiglocShape(
            int layer, string name, double x, double y, double angle, double ratio_x, double ratio_y
        ) : base(layer)
        {
            this.name = name;
            this.x = x;
            this.y = y;
            this.angle = angle;
            this.ratio_x = ratio_x;
            this.ratio_y = ratio_y;
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
            return Helper.MakeFeatureString("point_marker_feature", layer, Color, StartX, StartY, MarkerCode, RotateAngle, Scale);
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
            return Helper.MakeFeatureString("line_feature", layer, Color, LineType, LineWidth, StartX, StartY, EndX, EndY);
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

        public void AddVertex(double x, double y)
        {
            VertexX.Add(x);
            VertexY.Add(y);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString("polyline_feature", layer, Color, LineType, LineWidth, VertexX.Count, VertexX, VertexY);
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
            return Helper.MakeFeatureString("circle_feature", layer, Color, LineType, LineWidth, CenterX, CenterY, Radius);
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
                "arc_feature", layer, Color, LineType, LineWidth, CenterX, CenterY, Radius, Direction, StartAngle, EndAngle
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
                "ellipse_feature", layer, Color, LineType, LineWidth, CenterX, CenterY, RadiusX, RadiusY, RotateAngle
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
                "ellipse_arc_feature", layer, Color, LineType, LineWidth, 
                CenterX, CenterY, RadiusX, RadiusY, Direction, RotateAngle, StartAngle, EndAngle
            );
        }
    }

    //------------------------------------------------
    //	文字要素
    //------------------------------------------------
    class SxfTextShape
    {
        /* レイヤコード */
        public int laye;
        /* 色コード */
        public int color;
        /* 文字フォントコード */
        public int font;
        /* 文字列 */
        public string str;
        /* 文字配置基点Ｘ座標 */
        public double text_x;
        /* 文字配置基点Ｙ座標 */
        public double text_y;
        /* 文字範囲高 */
        public double height;
        /* 文字範囲幅 */
        public double width;
        /* 文字間隔 */
        public double spc;
        /* 文字回転角 */
        public double angle;
        /* スラント角 */
        public double slant;
        /* 文字配置基点 */
        public int b_pnt;
        /* 文字書き出し方向 */
        public int direct;

    }

    //------------------------------------------------
    //	クロソイド曲線
    //------------------------------------------------
    public class SxfClothoidShape
    {
        /* レイヤコード */
        public int layer;
        /* 色コード */
        public int color;
        /* 線種コード */
        public int type;
        /* 線幅コード */
        public int line_width;
        /* 配置基点Ｘ座標 */
        public double base_x;
        /* 配置基点Ｙ座標 */
        public double base_y;
        /* クロソイドパラメータ */
        public double parameter;
        /* 向きフラグ */
        public int direction;
        /* 回転角 */
        public double angle;
        /* 開始曲線長 */
        public double start_length;
        /* 終了曲線長 */
        public double end_length;
    }

    //------------------------------------------------
    //	既定義シンボル
    //------------------------------------------------
    public class SxfExternallyDefinedSymbolShape
    {
        /* レイヤコード */
        public int layer;
        /* 色コードフラグ */
        public int color_flag;
        /* 色コード */
        public int color;
        /* シンボル名 */
        string name;
        /* 配置位置X座標 */
        public double start_x;
        /* 配置位置Y座標 */
        public double start_y;
        /* 回転角 */
        public double rotate_angle;
        /* 倍率 */
        public double scale;
    }

    //------------------------------------------------
    //	直線寸法
    //------------------------------------------------
    public class SxfLinearDimShape
    {
        /* レイヤコード */
        public int layer;
        /* 色コード */
        public int color;
        /* 線種コード */
        public int type;
        /* 線幅コード */
        public int line_width;
        /* 寸法線始点Ｘ座標 */
        public double sun_x1;
        /* 寸法線始点Ｙ座標 */
        public double sun_y1;
        /* 寸法線終点Ｘ座標 */
        public double sun_x2;
        /* 寸法線終点Ｙ座標 */
        public double sun_y2;
        /* 補助線１の有無フラグ(０：無、１：有) */
        public int flg2;
        /* 補助線１基点Ｘ座標 */
        public double ho1_x0;
        /* 補助線１基点Ｙ座標 */
        public double ho1_y0;
        /* 補助線１基点Ｘ座標 */
        public double ho1_x1;
        /* 補助線１基点Ｙ座標 */
        public double ho1_y1;
        /* 補助線１基点Ｘ座標 */
        public double ho1_x2;
        /* 補助線１基点Ｙ座標 */
        public double ho1_y2;
        /* 補助線２の有無フラグ(０：無、１：有) */
        public int flg3;
        /* 補助線２基点Ｘ座標 */
        public double ho2_x0;
        /* 補助線２基点Ｙ座標 */
        public double ho2_y0;
        /* 補助線２基点Ｘ座標 */
        public double ho2_x1;
        /* 補助線２基点Ｙ座標 */
        public double ho2_y1;
        /* 補助線２基点Ｘ座標 */
        public double ho2_x2;
        /* 補助線２基点Ｙ座標 */
        public double ho2_y2;
        /* 矢印１コード */
        public int arr1_code1;
        /* 矢印１内外コード(0:なし 1:外向き 2:内向き) */
        public int arr1_code2;
        /* 矢印１配置始点Ｘ座標 */
        public double arr1_x;
        /* 矢印１配置始点Ｙ座標 */
        public double arr1_y;
        /* 矢印１配置倍率 */
        public double arr1_r;
        /* 矢印２コード */
        public int arr2_code1;
        /* 矢印２内外コード(0:なし 1:外向き 2:内向き) */
        public int arr2_code2;
        /* 矢印２配置始点Ｘ座標 */
        public double arr2_x;
        /* 矢印２配置始点Ｙ座標 */
        public double arr2_y;
        /* 矢印２配置倍率 */
        public double arr2_r;
        /* 寸法値の有無フラグ(０：無、１：有) */
        public int flg4;
        /* 文字フォントコード */
        public int font;
        /* 文字列 */
        public string str;
        /* 文字列配置基点Ｘ座標 */
        public double text_x;
        /* 文字列配置基点Ｙ座標 */
        public double text_y;
        /* 文字範囲高 */
        public double height;
        /* 文字範囲幅 */
        public double width;
        /* 文字間隔 */
        public double spc;
        /* 文字列回転角 */
        public double angle;
        /* スラント角度 */
        public double slant;
        /* 文字配置基点 */
        /*  １：左下、２：中下、３：右下、*/
        /*  ４：左中、５：中中、６：右中、*/
        /*  ７：左上、８：中上、９：右上 */
        public int b_pnt;
        /* 文字書出し方向(１：横書き、２：縦書き) */
        public int direct;

    }

    //------------------------------------------------
    //  弧長寸法
    //------------------------------------------------
    public class SxfCurveDimShape
    {
        public int layer;              /* レイヤコード */
        public int color;              /* 色コード */
        public int type;               /* 線種コード */
        public int line_width;         /* 線幅コード */
        public double sun_x;           /* 寸法線原点Ｘ座標 */
        public double sun_y;           /* 寸法線原点Ｙ座標 */
        public double sun_radius;      /* 寸法線半径 */
        public double sun_angle0;      /* 寸法線始角 */
        public double sun_angle1;      /* 寸法線終角 */
        public int flg2;               /* 補助線１の有無フラグ(0:無 1:有) */
        public double ho1_x0;          /* 補助線１基点Ｘ座標 */
        public double ho1_y0;          /* 補助線１基点Ｙ座標 */
        public double ho1_x1;          /* 補助線１始点Ｘ座標 */
        public double ho1_y1;          /* 補助線１始点Ｙ座標 */
        public double ho1_x2;          /* 補助線１終点Ｘ座標 */
        public double ho1_y2;          /* 補助線１終点Ｙ座標 */
        public int flg3;               /* 補助線２の有無フラグ(0:無 1:有) */
        public double ho2_x0;          /* 補助線２基点Ｘ座標 */
        public double ho2_y0;          /* 補助線２基点Ｙ座標 */
        public double ho2_x1;          /* 補助線２始点Ｘ座標 */
        public double ho2_y1;          /* 補助線２始点Ｙ座標 */
        public double ho2_x2;          /* 補助線２終点Ｘ座標 */
        public double ho2_y2;          /* 補助線２終点Ｙ座標 */
        public int arr1_code1;         /* 矢印１コード */
        public int arr1_code2;         /* 矢印１内外コード */
        public double arr1_x;          /* 矢印１配置点Ｘ座標 */
        public double arr1_y;          /* 矢印１配置点Ｙ座標 */
        public double arr1_r;          /* 矢印１配置倍率 */
        public int arr2_code1;         /* 矢印２コード */
        public int arr2_code2;         /* 矢印２内外コード */
        public double arr2_x;          /* 矢印２配置点Ｘ座標 */
        public double arr2_y;          /* 矢印２配置点Ｙ座標 */
        public double arr2_r;          /* 矢印２配置倍率 */
        public int flg4;               /* 寸法値の有無フラグ */
        public int font;               /* 文字フォントコード */
        public string str;
        public double text_x;          /* 文字列配置基点Ｘ座標 */
        public double text_y;          /* 文字列配置基点Ｙ座標 */
        public double height;          /* 文字範囲高 */
        public double width;           /* 文字範囲幅 */
        public double spc;             /* 文字間隔 */
        public double angle;           /* 文字列回転角 */
        public double slant;           /* スラント角度 */
        public int b_pnt;              /* 文字配置基点 */
        public int direct;             /* 文字書出し方向 */
    }

    //------------------------------------------------
    //  角度寸法
    //------------------------------------------------
    public class SxfAngularDimShape
    {
        public int layer;              /* レイヤコード */
        public int color;              /* 色コード */
        public int type;               /* 線種コード */
        public int line_width;         /* 線幅コード */
        public double sun_x;           /* 寸法線原点Ｘ座標 */
        public double sun_y;           /* 寸法線原点Ｙ座標 */
        public double sun_radius;      /* 寸法線半径 */
        public double sun_angle0;      /* 寸法線始角 */
        public double sun_angle1;      /* 寸法線終角 */
        public int flg2;               /* 補助線１の有無フラグ(0:無 1:有) */
        public double ho1_x0;          /* 補助線１基点Ｘ座標 */
        public double ho1_y0;          /* 補助線１基点Ｙ座標 */
        public double ho1_x1;          /* 補助線１始点Ｘ座標 */
        public double ho1_y1;          /* 補助線１始点Ｙ座標 */
        public double ho1_x2;          /* 補助線１終点Ｘ座標 */
        public double ho1_y2;          /* 補助線１終点Ｙ座標 */
        public int flg3;               /* 補助線２の有無フラグ(0:無 1:有) */
        public double ho2_x0;          /* 補助線２基点Ｘ座標 */
        public double ho2_y0;          /* 補助線２基点Ｙ座標 */
        public double ho2_x1;          /* 補助線２始点Ｘ座標 */
        public double ho2_y1;          /* 補助線２始点Ｙ座標 */
        public double ho2_x2;          /* 補助線２終点Ｘ座標 */
        public double ho2_y2;          /* 補助線２終点Ｙ座標 */
        public int arr1_code1;         /* 矢印１コード */
        public int arr1_code2;         /* 矢印１内外コード */
        public double arr1_x;          /* 矢印１配置点Ｘ座標 */
        public double arr1_y;          /* 矢印１配置点Ｙ座標 */
        public double arr1_r;          /* 矢印１配置倍率 */
        public int arr2_code1;         /* 矢印２コード */
        public int arr2_code2;         /* 矢印２内外コード */
        public double arr2_x;          /* 矢印２配置点Ｘ座標 */
        public double arr2_y;          /* 矢印２配置点Ｙ座標 */
        public double arr2_r;          /* 矢印２配置倍率 */
        public int flg4;               /* 寸法値の有無フラグ */
        public int font;               /* 文字フォントコード */
        public string str;
        public double text_x;          /* 文字列配置基点Ｘ座標 */
        public double text_y;          /* 文字列配置基点Ｙ座標 */
        public double height;          /* 文字範囲高 */
        public double width;           /* 文字範囲幅 */
        public double spc;             /* 文字間隔 */
        public double angle;           /* 文字列回転角 */
        public double slant;           /* スラント角度 */
        public int b_pnt;              /* 文字配置基点 */
        public int direct;             /* 文字書出し方向 */
    }

    //------------------------------------------------
    //  半径寸法
    //------------------------------------------------
    public class SxfRadiusDimShape
    {
        public int layer;              /* レイヤコード */
        public int color;              /* 色コード */
        public int type;               /* 線種コード */
        public int line_width;         /* 線幅コード */
        public double sun_x1;          /* 寸法線始点Ｘ座標 */
        public double sun_y1;          /* 寸法線始点Ｙ座標 */
        public double sun_x2;          /* 寸法線終点Ｘ座標 */
        public double sun_y2;          /* 寸法線終点Ｙ座標 */
        public int arr_code1;          /* 矢印コード */
        public int arr_code2;          /* 矢印内外コード */
        public double arr_x;           /* 矢印配置点Ｘ座標 */
        public double arr_y;           /* 矢印配置点Ｙ座標 */
        public double arr_r;           /* 矢印配置倍率 */
        public int flg;                /* 寸法値の有無フラグ */
        public int font;               /* 文字フォントコード */
        public string str;
        public double text_x;          /* 文字列配置基点Ｘ座標 */
        public double text_y;          /* 文字列配置基点Ｙ座標 */
        public double height;          /* 文字範囲高 */
        public double width;           /* 文字範囲幅 */
        public double spc;             /* 文字間隔 */
        public double angle;           /* 文字列回転角 */
        public double slant;           /* スラント角 */
        public int b_pnt;              /* 文字配置基点 */
        public int direct;             /* 文字書出し方向 */
    }

    //------------------------------------------------
    //  直径寸法
    //------------------------------------------------
    public class SxfDiameterDimShape
    {
        public int layer;              /* レイヤコード */
        public int color;              /* 色コード */
        public int type;               /* 線種コード */
        public int line_width;         /* 線幅コード */
        public double sun_x1;          /* 寸法線始点Ｘ座標 */
        public double sun_y1;          /* 寸法線始点Ｙ座標 */
        public double sun_x2;          /* 寸法線終点Ｘ座標 */
        public double sun_y2;          /* 寸法線終点Ｙ座標 */
        public int arr1_code1;         /* 矢印１コード */
        public int arr1_code2;         /* 矢印１内外コード */
        public double arr1_x;          /* 矢印１配置点Ｘ座標 */
        public double arr1_y;          /* 矢印１配置点Ｙ座標 */
        public double arr1_r;          /* 矢印１配置倍率 */
        public int arr2_code1;         /* 矢印２コード */
        public int arr2_code2;         /* 矢印２内外コード */
        public double arr2_x;          /* 矢印２配置点Ｘ座標 */
        public double arr2_y;          /* 矢印２配置点Ｙ座標 */
        public double arr2_r;          /* 矢印２配置倍率 */
        public int flg;                /* 寸法値有無フラグ */
        public int font;               /* 文字フォントコード */
        public string str;
        public double text_x;          /* 文字列配置基点Ｘ座標 */
        public double text_y;          /* 文字列配置基点Ｙ座標 */
        public double height;          /* 文字範囲高 */
        public double width;           /* 文字範囲幅 */
        public double spc;             /* 文字間隔 */
        public double angle;           /* 文字列回転角 */
        public double slant;           /* スラント角度 */
        public int b_pnt;              /* 文字配置基点 */
        public int direct;             /* 文字書出し方向 */
    }


    //------------------------------------------------
    //	スプライン曲線
    //------------------------------------------------
    public class SxfSplineShape
    {
        /* レイヤコード */
        int layer;
        /* 色コード */
        int color;
        /* 線種コード */
        int type;
        /* 線幅コード */
        int line_width;
        /* 開閉区分 */
        int open_close;
        /* 頂点数 */
        int number;
        /* X座標 */
        List<double> x = new();
        /* Y座標 */
        List<double> y = new();
    };


    //------------------------------------------------
    //  引出し線
    //------------------------------------------------
    public class SxfLabelShape
    {
        int layer;                          /* レイヤコード */
        int color;                          /* 色コード */
        int type;                           /* 線種コード */
        int line_width;                     /* 線幅コード */
        int vertex_number;                  /* 頂点数 */
        /* X座標 */
        List<double> vertex_x = new();
        /* Y座標 */
        List<double> vertex_y = new();
        int arr_code;                       /* 矢印コード */
        double arr_r;                       /* 矢印配置倍率 */
        int flg;                            /* 寸法値の有無フラグ */
        int font;                           /* 文字フォントコード */
        string str;
        double text_x;                      /* 文字列配置基点Ｘ座標 */
        double text_y;                      /* 文字列配置基点Ｙ座標 */
        double height;                      /* 文字範囲高 */
        double width;                       /* 文字範囲幅 */
        double spc;                         /* 文字間隔 */
        double angle;                       /* 文字列回転角 */
        double slant;                       /* スラント角度 */
        int b_pnt;                          /* 文字配置基点 */
        int direct;                         /* 文字書出し方向 */
    };

    //------------------------------------------------
    //  バルーン
    //------------------------------------------------
    public class SxfBalloonShape
    {
        int layer;                          /* レイヤコード */
        int color;                          /* 色コード */
        int type;                           /* 線種コード */
        int line_width;                     /* 線幅コード */
        int vertex_number;                  /* 頂点数 */
        /* X座標 */
        List<double> vertex_x = new();
        /* Y座標 */
        List<double> vertex_y = new();
        double center_x;                    /* 中心Ｘ座標 */
        double center_y;                    /* 中心Ｙ座標 */
        double radius;                      /* 半径 */
        int arr_code;                       /* 矢印コード */
        double arr_r;                       /* 矢印配置倍率 */
        int flg;                            /* 寸法値の有無フラグ */
        int font;                           /* 文字フォントコード */
        string str;
        double text_x;                      /* 文字列配置基点Ｘ座標 */
        double text_y;                      /* 文字列配置基点Ｙ座標 */
        double height;                      /* 文字範囲高 */
        double width;                       /* 文字範囲幅 */
        double spc;                         /* 文字間隔 */
        double angle;                       /* 文字列回転角 */
        double slant;                       /* スラント角度 */
        int b_pnt;                          /* 文字配置基点 */
        int direct;                         /* 文字書出し方向 */
    };


    //------------------------------------------------
    //  複合曲線定義
    //------------------------------------------------
    public class SxfCcurveOrg
    {
        public int color;              /* 色コード */
        public int type;               /* 線種コード */
        public int line_width;         /* 線幅コード */
        public int flag;               /* 表示/非表示フラグ */
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

