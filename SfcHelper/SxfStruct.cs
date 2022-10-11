using System.Xml.Linq;

namespace SfcHelper
{
    /// <summary>
    /// 用紙
    /// </summary>
    public class SxfSheet
    {
        internal SxfSheet(string name, int paperType, int orient, int width, int height)
        {
            Name = name;
            PaperType = paperType;
            Orient = orient;
            Width = width;
            Height = height;
        }

        //public void SetParameters(string name, int paperType, int orient, int width, int height)
        //{
        //    Name = name;
        //    PaperType = paperType;
        //    Orient = orient;
        //    Width = width;
        //    Height = height;
        //}


        /// <summary>
        /// 図面名
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// 用紙サイズ種別(0:A0, 1:A1, 2:A2, 3:A3, 4:A4, 9:FREE)
        /// </summary>
        public int PaperType { get; private set; }
        /// <summary>
        /// 縦／横区分　0:縦 1:横
        /// </summary>
        public int Orient { get; private set; }
        /// <summary>
        /// 自由用紙横長
        /// </summary>
        public int Width { get; private set; }
        /// <summary>
        /// 自由用紙縦長
        /// </summary>
        public int Height { get; private set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString("drawing_sheet_feature", Name, PaperType, Orient, Width, Height);
        }
    }

    /// <summary>
    /// 既定義シンボル
    /// </summary>
    public class SxfExternallyDefinedSymbol
    {
        /// <summary>
        /// 既定義シンボルを返す。シンボル名は、
        /// "sxf_hatch_style_7_symbol"及び"sxf_hatch_style_8_symbol"
        /// それ以外は空の配列を返す。
        /// </summary>
        /// <param name="sxfHatchPatternName">既定義シンボル名</param>
        /// <param name="ratioX">倍率X</param>
        /// <param name="ratioY">倍率Y</param>
        /// <param name="angleDeg">回転角（度）</param>
        /// <returns>線分の座標の配列。各線分は(X1,Y1)-(X2,Y2)で構成される。つまり配列サイズは4の倍数となります。</returns>
        public static double[] GetExternallyDefinedSymbol(
            string sxfHatchPatternName, double ratioX, double ratioY, double angleDeg
        )
        {
            double[] ret = Array.Empty<double>();
            switch (sxfHatchPatternName)
            {
                case "sxf_hatch_style_7_symbol":
                    //DX=200 DY=200
                    {
                        ret = new double[]
                        {
                            0.0, 0.0, 200.0, 0.0,
                            200.0, 0.0, 200.0, 100.0,
                            200.0, 100.0, 0.0, 100.0,
                            100.0, 100.0, 100.0, 200.0,
                        };
                    }
                    break;
                case "sxf_hatch_style_8_symbol":
                    //DX=sqrt(2)*200 DY=sqrt(2)*200
                    {
                        ret = new double[]
                        {
                        0.0, 0.0, 100.0, 0.0,
                        100.0, -100.0, 100.0, 100.0,
                        100.0, 100.0, 300.0, 100,
                        300.0, 100, 300.0, 0.0,
                        300.0, 0.0, 400, 0.0,
                        200.0, 100.0, 200, -200.0,
                        200.0, -100.0, 300, -100.0,
                        };
                        var a45 = Helper.DegToRad(45.0);
                        for (var i = 0; i < ret.Length; i += 2)
                        {
                            (ret[i], ret[i + 1]) = Helper.RotatePoint(ret[i], ret[i + 1], a45);
                        }
                    }
                    break;
            }
            var a = Helper.DegToRad(angleDeg);
            for (var i = 0; i < ret.Length; i += 2)
            {
                (ret[i], ret[i + 1]) = Helper.ScalePoint(ret[i], ret[i + 1], ratioX, ratioY);
                (ret[i], ret[i + 1]) = Helper.RotatePoint(ret[i], ret[i + 1], a);
            }
            return ret;
        }
    }


    /// <summary>
    /// 複合図形定義
    /// </summary>
    public class SxfSfigOrg
    {
        /// <summary>複合図形名</summary>
        public string Name;

        /// <summary>
        /// 複合図形種別フラグ
        /// (1:部分図（数学座標系）, 2:部分図（測地座標系）,3:作図グループ, 4:作図部品)
        /// </summary>
        public int Flag;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="name">複合図形名</param>
        /// <param name="flag">
        /// 複合図形種別フラグ(1:部分図（数学座標系）, 2:部分図（測地座標系）,3:作図グループ, 4:作図部品)
        /// </param>
        public SxfSfigOrg(string name, int flag)
        {
            Name = name;
            Flag = flag;
        }
        /// <summary>
        /// 図形のリスト
        /// </summary>
        public List<SxfShape> Shapes { get; } = new();

        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString("sfig_org_feature", Name, Flag);
        }
    }

    /// <summary>
    /// レイヤ
    /// </summary>
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
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="name">レイヤ名</param>
        /// <param name="flag">表示/非表示フラグ 0:非表示　1:表示</param>
        public SxfLayer(string name, int flag)
        {
            Name = name;
            Flag = flag;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString("layer_feature", Name, Flag);
            //            return @$"layer_feature(\'{Name}\','{Flag}')";
        }
    }

    /// <summary>
    /// 線種（ユーザ定義）
    /// </summary>
    public class SxfLineType
    {
        /// <summary>
        /// 線種名
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// ピッチ線分の長さ＋空白長さの繰り返し
        /// </summary>
        public readonly double[] Pitch;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="name">線種名</param>
        /// <param name="pitch">ピッチ線分の長さ＋空白長さの繰り返し</param>
        public SxfLineType(string name, double[] pitch)
        {
            Name = name;
            Pitch = pitch;
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString("user_defined_font_feature", Name, Pitch.Length, Pitch);
            //var a = string.Join(",", Pitch);
            //return @$"user_defined_font_feature(\'{Name}\','{Pitch.Length}','({a})')";
        }
    }

    /// <summary>
    /// 既定義線種
    /// </summary>
    public class SxfPreDefinedLineType : SxfLineType
    {
        /// <summary>
        /// 既定義線種の配列
        /// </summary>
        public static SxfPreDefinedLineType[] PreDefLineTypes = new SxfPreDefinedLineType[]
        {
            new SxfPreDefinedLineType("continuous", new double[]{ }),//1
            new SxfPreDefinedLineType("dashed", new double[]{6, 1.5}),//2
            new SxfPreDefinedLineType("dashed spaced", new double[]{6, 6}),//3
            new SxfPreDefinedLineType("long dashed dotted", new double[]{12, 1.5, 0.25, 1.5}),//4
            new SxfPreDefinedLineType("long dashed double-dotted", new double[]{12,1.5,0.25,1.5,0.25,1.5}),//5
            new SxfPreDefinedLineType("long dashed triplicate-dotted", new double[]{12,3,0.25,1.5,0.25,1.5,0.25,1.5}),//6
            new SxfPreDefinedLineType("dotted", new double[]{0.25, 1.5}),//7
            new SxfPreDefinedLineType("chain", new double[]{12,1.5,3.5,1.5}),//8
            new SxfPreDefinedLineType("chain double dash", new double[]{12,1.5,3.5,1.5,3.5,1.5}),//9
            new SxfPreDefinedLineType("dashed dotted", new double[]{6,1.5,0.25,1.5}),//10
            new SxfPreDefinedLineType("double-dashed dotted", new double[]{6,1.5,6,1.5,0.25,1.5}),//11
            new SxfPreDefinedLineType("dashed double-dotted", new double[]{6,1.5,0.25,1.5,0.25,1.5}),//12
            new SxfPreDefinedLineType("double-dashed double-dotted", new double[]{6,1.5,6,1.5,0.25,1.5,0.25,1.5}),//13
            new SxfPreDefinedLineType("dashed triplicate-dotted", new double[]{6,1.5,0.25,1.5,0.25,1.5,0.25,1.5}),//14
            new SxfPreDefinedLineType("double-dashed triplicate-dotted", new double[]{6,1.5,6,1.5,0.25,1.5,0.25,1.5,0.25,1.5}),//15
        };

        private SxfPreDefinedLineType(string name, double[] pitch) : base(name, pitch)
        {
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $@"pre_defined_font_feature(\'{Name}\')";
        }
    }

    /// <summary>
    /// 色
    /// </summary>
    public class SxfColor
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="r">赤</param>
        /// <param name="g">緑</param>
        /// <param name="b">青</param>
        public SxfColor(int r, int g, int b)
        {
            Red = r;
            Green = g;
            Blue = b;
        }
        /// <summary>赤</summary>
        public int Red { get; }
        /// <summary>緑</summary>
        public int Green { get; }
        /// <summary>青</summary>
        public int Blue { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $@"user_defined_colour_feature('{Red}','{Green}','{Blue}')";
        }
    }

    /// <summary>
    /// 既定義色
    /// </summary>
    public class SxfPreDefinedColor : SxfColor
    {
        /// <summary>色名</summary>
        public string Name;

        /// <summary>
        /// 既定義色の配列
        /// </summary>
        public static SxfPreDefinedColor[] PreDefColors = new SxfPreDefinedColor[]
        {
            new("black", 0,0,0),new("red", 255,0,0),new("green", 0,255,0),new("blue", 0,0,255),
            new("yellow", 255,255,0),new("magenta", 255,0,255),new("cyan", 0,255,255),new("white", 255,255,255),
            new("deeppink", 192,0,128),new("brown", 192,128,64),new("orange", 255,128,0),new("lightgreen", 128,192,128),
            new("lightblue", 0,128,255),new("lavender", 128,64,255),new("lightgray", 192,192,192),new("darkgray", 128,128,128),
        };


        private SxfPreDefinedColor(string name, int r, int g, int b) : base(r, g, b)
        {
            Name = name;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $@"pre_defined_colour_feature(\'{Name}\')";
        }
    }

    /// <summary>
    /// 線幅
    /// </summary>
    public class SxfLineWidth
    {
        /// <summary>線幅</summary>
        public double Width { get; }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="width">線幅</param>
        public SxfLineWidth(double width)
        {
            Width = width;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $@"width_feature('{Width}')";
        }

        /// <summary>
        /// 既定義線幅
        /// </summary>
        public static double[] PreDefinedLineWidth = new double[]
        {
            0.13,0.18,0.25,0.35,0.5,0.7,1.0,1.4,2.0
        };
    }

    /// <summary>
    /// 文字フォント
    /// </summary>
    public class SxfTextFont
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="name">フォント名</param>
        public SxfTextFont(string name)
        {
            Name = name;
        }

        /// <summary>
        /// 文字フォント名
        /// </summary>
        public string Name { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $@"text_font_feature(\'{Name}\')";
        }

    }

    /// <summary>
    /// 図面表題欄
    /// </summary>
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


    /// <summary>
    /// 図形の基本クラス
    /// </summary>
    public class SxfShape
    {
        /// <summary> レイヤコード </summary>
        public int Layer;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="layer">レイヤコード</param>
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

    /// <summary>
    /// 線分
    /// </summary>
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
        /// <summary>始点Ｙ座標</summary>
        public double StartY;
        /// <summary>終点Ｘ座標</summary>
        public double EndX;
        /// <summary>終点Ｙ座標</summary>
        public double EndY;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="layer">レイヤコード</param>
        /// <param name="color">色コード</param>
        /// <param name="lineType">線種コード</param>
        /// <param name="lineWidth">線幅コード</param>
        /// <param name="start_x">始点Ｘ座標</param>
        /// <param name="start_y">始点Ｙ座標</param>
        /// <param name="end_x">終点Ｘ座標</param>
        /// <param name="end_y">終点Ｙ座標</param>
        public SxfLineShape(
            int layer, int color, int lineType, int lineWidth,
            double start_x, double start_y, double end_x, double end_y
        ) : base(layer)
        {
            this.Color = color;
            this.LineType = lineType;
            this.LineWidth = lineWidth;
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

    /// <summary>
    /// 折線
    /// </summary>
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

    /// <summary>
    /// 円
    /// </summary>
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
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="layer">レイヤコード</param>
        /// <param name="color">色コード</param>
        /// <param name="lineType">線種コード</param>
        /// <param name="lineWidth">線幅コード</param>
        /// <param name="centerX">中心X座標</param>
        /// <param name="centerY">中心Y座標</param>
        /// <param name="radius">半径</param>
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

    /// <summary>
    /// 円弧
    /// </summary>
    public class SxfArcShape : SxfShape
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
        /// <summary>向きフラグ（0:反時計廻り,1: 時計廻り）</summary>
        public int Direction;
        /// <summary>始角</summary>
        public double StartAngle;
        /// <summary>終角</summary>
        public double EndAngle;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="layer">レイヤコード</param>
        /// <param name="color">色コード</param>
        /// <param name="lineType">線種コード</param>
        /// <param name="lineWidth">線幅コード</param>
        /// <param name="centerX">中心X座標</param>
        /// <param name="centerY">中心Y座標</param>
        /// <param name="radius">半径</param>
        /// <param name="direction">向きフラグ（0:反時計廻り,1: 時計廻り）</param>
        /// <param name="startAngle">始角</param>
        /// <param name="endAngle">終角</param>
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

    /// <summary>
    /// 楕円
    /// </summary>
    public class SxfEllipseShape : SxfShape
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

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="layer">レイヤコード</param>
        /// <param name="color">色コード</param>
        /// <param name="lineType">線種コード</param>
        /// <param name="lineWidth">線幅コード</param>
        /// <param name="centerX">中心X座標</param>
        /// <param name="centerY">中心Y座標</param>
        /// <param name="radiusX">X方向半径</param>
        /// <param name="radiusY">Y方向半径</param>
        /// <param name="rotateAngle">回転角</param>
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

    /// <summary>
    /// 楕円弧
    /// </summary>
    public class SxfEllipseArcShape : SxfShape
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
        /// <summary>向きフラグ（0:反時計廻り,1: 時計廻り）</summary>
        public int Direction;
        /// <summary>回転角</summary>
        public double RotateAngle;
        /// <summary>始角</summary>
        public double StartAngle;
        /// <summary>終角</summary>
        public double EndAngle;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="layer">レイヤコード</param>
        /// <param name="color">色コード</param>
        /// <param name="lineType">線種コード</param>
        /// <param name="lineWidth">線幅コード</param>
        /// <param name="centerX">中心X座標</param>
        /// <param name="centerY">中心Y座標</param>
        /// <param name="radiusX">X方向半径</param>
        /// <param name="radiusY">Y方向半径</param>
        /// <param name="direction">向きフラグ（0:反時計廻り,1: 時計廻り）</param>
        /// <param name="rotateAngle">回転角</param>
        /// <param name="startAngle">始角</param>
        /// <param name="endAngle">終角</param>
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

    /// <summary>
    /// 文字要素
    /// </summary>
    public class SxfTextShape : SxfShape
    {
        /// <summary>
        /// 色コード
        /// </summary>
        public int Color;
        /// <summary>
        /// 文字フォントコード
        /// </summary>
        public int Font;
        /// <summary>
        /// 文字列
        /// </summary>
        public string Str;
        /// <summary>
        /// 文字配置基点Ｘ座標
        /// </summary>
        public double TextX;
        /// <summary>
        /// 文字配置基点Ｙ座標
        /// </summary>
        public double TextY;
        /// <summary>
        /// 文字範囲高
        /// </summary>
        public double Height;
        /// <summary>
        /// 文字範囲幅
        /// </summary>
        public double Width;
        /// <summary>
        /// 文字間隔
        /// </summary>
        public double Spc;
        /// <summary>
        /// 文字列回転角（0≦度＜３６０）
        /// </summary>
        public double Angle;
        /// <summary>
        /// スラント角度（－８５≦度≦８５）
        /// </summary>
        public double Slant;
        /// <summary>
        /// 文字配置基点（1:左下，2:中下，3:右下，4:左中，5:中中，6:右中，7:左上，8:中上，9:右上）
        /// </summary>
        public int BPnt;
        /// <summary>
        /// 文字書出し方向（1:横書き, 2:縦書き）
        /// </summary>
        public int Direct;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="layer">レイヤコード</param>
        /// <param name="color">色コード</param>
        /// <param name="font">文字フォントコード</param>
        /// <param name="str">文字列</param>
        /// <param name="textX">文字配置基点Ｘ座標</param>
        /// <param name="textY">文字配置基点Ｙ座標</param>
        /// <param name="height">文字範囲高</param>
        /// <param name="width">文字範囲幅</param>
        /// <param name="spc">文字間隔</param>
        /// <param name="angle">文字列回転角（0≦度＜３６０）</param>
        /// <param name="slant">スラント角度（－８５≦度≦８５）</param>
        /// <param name="bPnt">文字配置基点（1:左下，2:中下，3:右下，4:左中，5:中中，6:右中，7:左上，8:中上，9:右上）</param>
        /// <param name="direct">文字書出し方向（1:横書き, 2:縦書き）</param>
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


    /// <summary>
    /// クロソイド曲線
    /// </summary>
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

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="layer">レイヤコード</param>
        /// <param name="color">色コード</param>
        /// <param name="lineType">線種コード</param>
        /// <param name="lineWidth">線幅コード</param>
        /// <param name="baseX">配置基点Ｘ座標</param>
        /// <param name="baseY">配置基点Ｙ座標</param>
        /// <param name="parameter">クロソイドパラメータ</param>
        /// <param name="direction">向きフラグ</param>
        /// <param name="angle">回転角</param>
        /// <param name="startLength">開始曲線長</param>
        /// <param name="endLength">終了曲線長</param>
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

    /// <summary>
    /// 複合図形配置
    /// </summary>
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

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="layer">レイヤコード</param>
        /// <param name="name">複合図形名</param>
        /// <param name="x">配置位置X座標</param>
        /// <param name="y">配置位置Y座標</param>
        /// <param name="angle">回転角</param>
        /// <param name="ratio_x">X方向尺度</param>
        /// <param name="ratio_y">Y方向尺度</param>
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

    /// <summary>
    /// 既定義シンボル
    /// </summary>
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

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="layer">レイヤコード</param>
        /// <param name="colorFlag">色コードフラグ</param>
        /// <param name="color">色コード</param>
        /// <param name="name">シンボル名</param>
        /// <param name="x">配置位置X座標</param>
        /// <param name="y">配置位置Y座標</param>
        /// <param name="angle">回転角</param>
        /// <param name="scale">倍率</param>
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

    /// <summary>
    /// 直線寸法
    /// </summary>
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

    /// <summary>
    /// 弧長寸法
    /// </summary>
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

    /// <summary>
    /// 角度寸法
    /// </summary>
    public class SxfAngularDimShape : SxfShape
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

        public SxfAngularDimShape(
            int layer, int color, int lineType, int lineWidth,
            double sunX, double sunY, double sunRadius, double sunAngle0, double sunAngle1,
            int flag2, double ho1X0, double ho1Y0, double ho1X1, double ho1Y1, double ho1X2, double ho1Y2,
            int flag3, double ho2X0, double ho2Y0, double ho2X1, double ho2Y1, double ho2X2, double ho2Y2,
            int arr1Code1, int arr1Code2, double arr1X, double arr1Y, double arr1R,
            int arr2Code1, int arr2Code2, double arr2X, double arr2Y, double arr2R, 
            int flag4, int font, string str, double textX, double textY, double height, double width,
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

    /// <summary>
    /// 半径寸法
    /// </summary>
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

    /// <summary>
    /// 直径寸法
    /// </summary>
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

    /// <summary>
    /// 引出し線
    /// </summary>
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

    /// <summary>
    /// バルーン
    /// </summary>
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

    /// <summary>
    /// ハッチング(既定義(外部定義)
    /// </summary>
    public class SxfExternallyDefinedHatchShape : SxfShape
    {
        /// <summary>ハッチング名</summary>
        public string Name;
        /// <summary>外形の複合曲線のフィーチャコード</summary>
        public int OutId;
        /// <summary>中抜きの複合曲線のフィーチャコード</summary>
        public readonly List<int> InId = new();
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="layer">レイヤコード</param>
        /// <param name="name">ハッチング名</param>
        /// <param name="outId">外形の複合曲線のフィーチャコード</param>
        /// <param name="inId">中抜きの複合曲線のフィーチャコード</param>
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


    /// <summary>
    /// ハッチング(塗り)
    /// </summary>
    public class SxfFillAreaStyleColorShape : SxfShape
    {
        /// <summary>色コード </summary>
        public int Color;
        /// <summary>外形の複合曲線のフィーチャコード</summary>
        public int OutId;
        /// <summary>中抜きの複合曲線のフィーチャコード</summary>
        public readonly List<int> InId = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="layer">レイヤコード</param>
        /// <param name="color">色コード</param>
        /// <param name="outId">外形の複合曲線のフィーチャコード</param>
        /// <param name="inId">中抜きの複合曲線のフィーチャコード</param>
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

    public class HatchingPattern
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

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="color">ハッチング線の色コード</param>
        /// <param name="lineType">ハッチング線の線種コード</param>
        /// <param name="lineWidth">ハッチング線の線幅コード</param>
        /// <param name="startX">ハッチング線のパターン開始点X座標</param>
        /// <param name="startY">ハッチング線のパターン開始点Y座標</param>
        /// <param name="spacing">ハッチング間隔</param>
        /// <param name="angle">ハッチング線の角度</param>
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

    /// <summary>
    /// ハッチング(ユーザ定義)
    /// </summary>
    public class SxfFillAreaStyleHatchingShape : SxfShape
    {
        /// <summary>ハッチング線のパターンのリスト</summary>
        public readonly List<HatchingPattern> HatchingPatternList = new();
        /// <summary>外形の複合曲線のフィーチャコード</summary>
        public int OutId;
        /// <summary>中抜きの複合曲線のフィーチャコード</summary>
        public readonly List<int> InId = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="layer">レイヤコード</param>
        /// <param name="hatchingPatternList">ハッチング線のパターンのリスト</param>
        /// <param name="outId">外形の複合曲線のフィーチャコード</param>
        /// <param name="inId">中抜きの複合曲線のフィーチャコード</param>
        /// <exception cref="Exception"></exception>
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

            return $"fill_area_style_hatching_feature('{Layer}','{HatchingPatternList.Count}',{s},'{OutId}','{InId.Count}','({a})')";
        }
    }

    /// <summary>
    /// ハッチング(パターン)
    /// </summary>
    public class SxfFillAreaStyleTilesShape : SxfShape
    {
        /// <summary>既定義シンボル名</summary>
        public string Name;
        /// <summary>ハッチパターンの色コード</summary>
        public int Color;                        /*  */
        /// <summary>ハッチパターン配置位置X座標</summary>
        public double PatternX;
        /// <summary>ハッチパターン配置位置Y座標</summary>
        public double PatternY;
        /// <summary>ハッチパターンの繰り返しベクトル１の大きさ</summary>
        public double PatternVector1;
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

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="layer">レイヤコード</param>
        /// <param name="name">既定義シンボル名</param>
        /// <param name="color">ハッチパターンの色コード</param>
        /// <param name="patternX">ハッチパターン配置位置X座標</param>
        /// <param name="patternY">ハッチパターン配置位置Y座標</param>
        /// <param name="patternVector1">ハッチパターンの繰り返しベクトル１の大きさ</param>
        /// <param name="patternVector1Angle">ハッチパターンの繰り返しベクトル１の角度</param>
        /// <param name="patternVector2">ハッチパターンの繰り返しベクトル２の大きさ</param>
        /// <param name="patternVector2Angle">ハッチパターンの繰り返しベクトル２の角度</param>
        /// <param name="patternScaleX">ハッチパターンのX尺度</param>
        /// <param name="patternScaleY">ハッチパターンのY尺度</param>
        /// <param name="patternAngle">ハッチパターンの向きの角度</param>
        /// <param name="outId">外形の複合曲線のフィーチャコード</param>
        /// <param name="inId">中抜きの複合曲線のフィーチャコード</param>
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

    /// <summary>
    /// 複合曲線定義
    /// </summary>
    public class SxfCcurveOrg
    {
        /// <summary>色コード </summary>
        public int Color;
        /// <summary>線種コード</summary>
        public int LineType;
        /// <summary>線幅コード</summary>
        public int LineWidth;
        /// <summary>表示/非表示フラグ(0:非表示, 1:表示)</summary>
        public int Flag;

        /// <summary>
        /// 図形のリスト
        /// </summary>
        public List<SxfShape> Shapes { get; } = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="color">色コード</param>
        /// <param name="lineType">線種コード</param>
        /// <param name="lineWidth">線幅コード</param>
        /// <param name="flag">表示/非表示フラグ(0:非表示, 1:表示)</param>
        public SxfCcurveOrg(int color, int lineType, int lineWidth, int flag)
        {
            Color = color;
            LineType = lineType;
            LineWidth = lineWidth;
            Flag = flag;
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return Helper.MakeFeatureString(
                "composite_curve_org_feature", Color, LineType, LineWidth, Flag);
        }
    }
}

