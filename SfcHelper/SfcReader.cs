using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace SfcHelper
{
    /// <summary>
    /// SFCファイル読み取りクラス
    /// </summary>
    public class SfcReader
    {
        SxfDocument mDoc = null!;
        List<SxfShape> mShapeBuffer = new();
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="doc">ドキュメント</param>
        public SfcReader(SxfDocument doc)
        {
            mDoc = doc;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        /// <summary>
        /// SFCファイルかどうかのチェック。ファイルのヘッダから判別します。
        /// </summary>
        /// <param name="path">ファイルのパス</param>
        /// <returns>SFCファイルであればtrue。</returns>
        public bool IsSfcFile(string path)
        {
            if (!File.Exists(path)) return false;
            using var r = new StreamReader(path, Encoding.GetEncoding("shift_jis"));
            return ReadHaeder(r);
        }

        /// <summary>
        /// ドキュメント読み込み
        /// </summary>
        /// <param name="path">ファイルのパス</param>
        public void Read(string path)
        {
            mDoc.Clear();
            using var r = new StreamReader(path, Encoding.GetEncoding("shift_jis"));
            if (!ReadHaeder(r)) throw new Exception("Header error");
            ReadDataSection(r);
        }

        /// <summary>
        /// データセクション読み込み
        /// </summary>
        void ReadDataSection(TextReader r)
        {
            //セクション開始チェック
            if (!CheckStartOfDataSection(r)) throw new Exception("Could not find DATA section");
            //初期化
            mShapeBuffer.Clear();
            //mNextCompositCurveId = 1;
            //読み込み開始
            var sb = new StringBuilder();
            while (true)
            {
                var s = r.ReadLine();
                if (s == null) return;
                s = s.Trim();
                if (s == "/*SXF" || s == "/*SXF3" || s == "/*SXF3.1")
                {
                    sb.Clear();
                    while (true)
                    {
                        s = r.ReadLine();
                        if (s == null) throw new Exception("Unexpected eol in comment feature.");
                        s = s.Trim();
                        if (s == "") continue;
                        if (s == "SXF*/" || s == "SXF3*/" || s == "SXF3.1*/")
                        {
                            var ts = new StringReader(sb.ToString());
                            ReadCommentFeature(ts);
                            break;
                        }
                        sb.Append(s);
                    }
                }
                //セクション終了チェック。
                if (Regex.Replace(s, @"[\s]+", "") == "ENDSEC;") return;
            }
        }

        /// <summary>
        /// HEADER セクションに続いてDATA;があるかのチェック。
        /// </summary>
        bool CheckStartOfDataSection(TextReader r)
        {
            while (true)
            {
                var s = r.ReadLine();
                if (s == null) return false;
                if (Regex.Replace(s, @"[\s]+", "") == "DATA;")  return true;
            }
        }


        /// <summary>
        /// Feature読み込み。書式は#100=feature_name(...)
        /// </summary>
        void ReadCommentFeature(TextReader r)
        {
            var tokenizer = new Tokenizer(r);
            var tok = tokenizer.GetNextToken();
            if (tok.Kind != Tokenizer.TokenKind.Hash) throw new Exception("Comment feature line did not start with #.");
            tok = tokenizer.GetNextToken();
            if (tok.Kind != Tokenizer.TokenKind.Number || !int.TryParse(tok.Value, out var num))
            {
                throw new Exception("Comment feature line did not have the instance ID.");
            }
            tok = tokenizer.GetNextToken();
            if (tok.Kind != Tokenizer.TokenKind.Equal) throw new Exception("Comment feature line did not have =.");
            tok = tokenizer.GetNextToken();
            if (tok.Kind != Tokenizer.TokenKind.Identifier) throw new Exception("Comment feature did not have the identifier.");
            var tag = tok.Value;
            var ps = GetParams(tokenizer);
            switch (tag)
            {
                case "drawing_sheet_feature":
                    ParseDrawingSheetFeature(ps);
                    break;
                case "sfig_org_feature":
                    ParseSfigOrgFeature(ps);
                    break;
                case "layer_feature":
                    mDoc.Table.ParseLayerFeature(ps);
                    break;
                case "pre_defined_font_feature":
                    mDoc.Table.ParsePreDefinedLineTypeFeature(ps);
                    break;
                case "user_defined_font_feature":
                    mDoc.Table.ParseUserDefinedLineTypeFeature(ps);
                    break;
                case "pre_defined_colour_feature":
                    mDoc.Table.ParsePreDefinedColourFeature(ps);
                    break;
                case "user_defined_colour_feature":
                    mDoc.Table.ParseUserDefinedColourFeature(ps);
                    break;
                case "width_feature":
                    mDoc.Table.ParseWidthFeature(ps);
                    break;
                case "text_font_feature":
                    mDoc.Table.ParseTextFontFeature(ps);
                    break;
                case "point_marker_feature":
                    ParsePointMarkerFeature(ps);
                    break;
                case "line_feature":
                    ParseLineFeature(ps);
                    break;
                case "polyline_feature":
                    ParsePolylineFeature(ps);
                    break;
                case "circle_feature":
                    ParseCircleFeature(ps);
                    break;
                case "arc_feature":
                    ParseArcFeature(ps);
                    break;
                case "ellipse_feature":
                    ParseEllipseFeature(ps);
                    break;
                case "ellipse_arc_feature":
                    ParseEllipseArcFeature(ps);
                    break;
                case "text_string_feature":
                    ParseTextStringFeature(ps);
                    break;
                case "spline_feature":
                    ParseSplineFeature(ps);
                    break;
                case "clothoid_feature":
                    ParseClothoidFeature(ps);
                    break;
                case "sfig_locate_feature":
                    ParseSfigLocateFeature(ps);
                    break;
                case "externally_defined_symbol_feature":
                    ParseExternallyDefinedSymbolFeature(ps);
                    break;
                case "linear_dim_feature":
                    ParseLinearDimFeature(ps);
                    break;
                case "curve_dim_feature":
                    ParseCurveDimFeature(ps);
                    break;
                case "angular_dim_feature":
                    ParseAngularDimFeature(ps);
                    break;
                case "radius_dim_feature":
                    ParseRadiusDimFeature(ps);
                    break;
                case "diameter_dim_feature":
                    ParseDiameterDimFeature(ps);
                    break;
                case "label_feature":
                    ParseLabelFeature(ps);
                    break;
                case "balloon_feature":
                    ParseBalloonFeature(ps);
                    break;
                case "externally_defined_hatch_feature":
                    ParseExternallyDefinedHatchFeature(ps);
                    break;
                case "fill_area_style_colour_feature":
                    ParseFillAreaStyleColorFeature(ps);
                    break;
                case "fill_area_style_hatching_feature":
                    ParseFillAreaStyleHatchingFeature(ps);
                    break;
                case "fill_area_style_tiles_feature":
                    ParseFillAreaStyleTilesFeature(ps);
                    break;
                case "composite_curve_org_feature":
                    ParseCompositeCurveOrgFeature(ps);
                    break;
                default:
                    Debug.WriteLine($"ReadCommentFeature:: unknown feature{tag}");
                    break;
            }
        }

        void ParseDrawingSheetFeature(List<object> ps)
        {
            CheckParameterSize(ps, 5, "Drawing sheet");
            var name = ParseString(ps[0], "DrawingSheet.name");
            var t = ParseInt(ps[1], "DrawingSheet.type");
            var orient = ParseInt(ps[2], "DrawingSheet.orient");
            var width = ParseInt(ps[3], "DrawingSheet.width");
            var height = ParseInt(ps[4], "DrawingSheet.height");
            mDoc.SetSheetParameter(name, t, orient, width, height);
            mDoc.Shapes.AddRange(mShapeBuffer);
            mShapeBuffer.Clear();
        }

        void ParseSfigOrgFeature(List<object> ps)
        {
            CheckParameterSize(ps, 2, "SfigOrg");
            var name = ParseString(ps[0], "SfigOrg.name");
            var flag = ParseInt(ps[1], "SfigOrg.flag");
            var sfig = new SxfSfigOrg(name, flag);
            sfig.Shapes.AddRange(mShapeBuffer);
            mDoc.AddSfigOrg(sfig);
            mShapeBuffer.Clear();
        }

        void ParseCompositeCurveOrgFeature(List<object> ps)
        {
            CheckParameterSize(ps, 4, "CompositeCurveOrg");
            var color = ParseInt(ps[0], "CompositeCurveOrg.color");
            var lineType = ParseInt(ps[1], "CompositeCurveOrg.lineType");
            var lineWidth = ParseInt(ps[2], "CompositeCurveOrg.lineWidth");
            var invisibility = ParseInt(ps[3], "CompositeCurveOrg.invisibility");
            var s = new SxfCcurveOrg(color, lineType, lineWidth, invisibility);
            s.Shapes.AddRange(mShapeBuffer);
            mDoc.AddCompositCurve(s);
            mShapeBuffer.Clear();
        }


        void ParsePointMarkerFeature(List<object> ps)
        {
            CheckParameterSize(ps, 7, "PointMarker");
            var layer = ParseInt(ps[0], "PointMarker.layer");
            var color = ParseInt(ps[1], "PointMarker.color");
            var x0 = ParseDouble(ps[2], "PointMarker.start_x");
            var y0 = ParseDouble(ps[3], "PointMarker.start_y");
            var code = ParseInt(ps[4], "PointMarker.marker_code");
            var angle = ParseDouble(ps[5], "PointMarker.angle");
            var scale = ParseDouble(ps[6], "PointMarker.scale");
            var s = new SxfPointMarker(layer, color, x0, y0, code, angle, scale);
            mShapeBuffer.Add(s);
        }

        void ParseLineFeature(List<object> ps)
        {
            CheckParameterSize(ps, 8, "Line");
            var layer = ParseInt(ps[0], "Line.layer");
            var color = ParseInt(ps[1], "Line.color");
            var lineType = ParseInt(ps[2], "Line.type");
            var lineWidth = ParseInt(ps[3], "Line.lineWidth");
            var x0 = ParseDouble(ps[4], "Line.start_x");
            var y0 = ParseDouble(ps[5], "Line.start_y");
            var x1 = ParseDouble(ps[6], "Line.end_x");
            var y1 = ParseDouble(ps[7], "Line.end_y");
            var s = new SxfLineShape(layer, color, lineType, lineWidth, x0, y0, x1, y1);
            mShapeBuffer.Add(s);
        }
        void ParsePolylineFeature(List<object> ps)
        {
            CheckParameterSize(ps, 7, "Polyline");
            var layer = ParseInt(ps[0], "Polyline.layer");
            var color = ParseInt(ps[1], "Polyline.color");
            var lineType = ParseInt(ps[2], "Polyline.type");
            var lineWidth = ParseInt(ps[3], "Polyline.lineWidth");
            var n = ParseInt(ps[4], "Polyline.number");
            var vx = SfcReader.ParseDoubleList(ps[5], "Polyline.vertex_x");
            var vy = SfcReader.ParseDoubleList(ps[6], "Polyline.vertex_y");
            CheckVertexSize(vx, vy, n, "Polyline");
            var s = new SxfPolylineShape(layer, color, lineType, lineWidth, vx, vy);
            mShapeBuffer.Add(s);
        }
        void ParseCircleFeature(List<object> ps)
        {
            CheckParameterSize(ps, 7, "Circle");
            var layer = ParseInt(ps[0], "Circle.layer");
            var color = ParseInt(ps[1], "Circle.color");
            var lineType = ParseInt(ps[2], "Circle.type");
            var lineWidth = ParseInt(ps[3], "Circle.lineWidth");
            var x0 = ParseDouble(ps[4], "Circle.center_x");
            var y0 = ParseDouble(ps[5], "Circle.center_y");
            var r = ParseDouble(ps[6], "Circle.radius");
            var s = new SxfCircleShape(layer, color, lineType, lineWidth, x0, y0, r);
            mShapeBuffer.Add(s);
        }

        void ParseArcFeature(List<object> ps)
        {
            CheckParameterSize(ps, 10, "Arc");
            var layer = ParseInt(ps[0], "Arc.layer");
            var color = ParseInt(ps[1], "Arc.color");
            var lineType = ParseInt(ps[2], "Arc.type");
            var lineWidth = ParseInt(ps[3], "Arc.lineWidth");
            var x0 = ParseDouble(ps[4], "Arc.center_x");
            var y0 = ParseDouble(ps[5], "Arc.center_y");
            var r = ParseDouble(ps[6], "Arc.radius");
            var direction = ParseInt(ps[7], "Arc.direction");
            var sa = ParseDouble(ps[8], "Arc.start_angle");
            var ea = ParseDouble(ps[9], "Arc.end_angle");
            var s = new SxfArcShape(layer, color, lineType, lineWidth, x0, y0, r, direction, sa, ea);
            mShapeBuffer.Add(s);
        }

        void ParseEllipseFeature(List<object> ps)
        {
            CheckParameterSize(ps, 9, "Ellipse");
            var layer = ParseInt(ps[0], "Ellipse.layer");
            var color = ParseInt(ps[1], "Ellipse.color");
            var lineType = ParseInt(ps[2], "Ellipse.type");
            var lineWidth = ParseInt(ps[3], "Ellipse.lineWidth");
            var x0 = ParseDouble(ps[4], "Ellipse.center_x");
            var y0 = ParseDouble(ps[5], "Ellipse.center_y");
            var rx = ParseDouble(ps[6], "Ellipse.radius_x");
            var ry = ParseDouble(ps[7], "Ellipse.radius_y");
            var angle = ParseDouble(ps[8], "Ellipse.angle");
            var s = new SxfEllipseShape(layer, color, lineType, lineWidth, x0, y0, rx, ry, angle);
            mShapeBuffer.Add(s);
        }

        void ParseEllipseArcFeature(List<object> ps)
        {
            CheckParameterSize(ps, 12, "EllipseArc");
            var layer = ParseInt(ps[0], "EllipseArc.layer");
            var color = ParseInt(ps[1], "EllipseArc.color");
            var lineType = ParseInt(ps[2], "EllipseArc.type");
            var lineWidth = ParseInt(ps[3], "EllipseArc.lineWidth");
            var x0 = ParseDouble(ps[4], "EllipseArc.center_x");
            var y0 = ParseDouble(ps[5], "EllipseArc.center_y");
            var rx = ParseDouble(ps[6], "EllipseArc.radius_x");
            var ry = ParseDouble(ps[7], "EllipseArc.radius_y");
            var direction = ParseInt(ps[8], "EllipseArc.direction");
            var angle = ParseDouble(ps[9], "EllipseArc.angle");
            var sa = ParseDouble(ps[10], "EllipseArc.start_angle");
            var ea = ParseDouble(ps[11], "EllipseArc.end_angle");
            var s = new SxfEllipseArcShape(
                layer, color, lineType, lineWidth, x0, y0, rx, ry, direction, angle, sa, ea);
            mShapeBuffer.Add(s);
        }

        void ParseTextStringFeature(List<object> ps)
        {
            CheckParameterSize(ps, 13, "TextString");
            var layer = ParseInt(ps[0], "TextString.layer");
            var color = ParseInt(ps[1], "TextString.color");
            var font = ParseInt(ps[2], "TextString.font");
            var str = ParseString(ps[3], "TextString.str");
            var x0 = ParseDouble(ps[4], "TextString.x");
            var y0 = ParseDouble(ps[5], "TextString.y");
            var h = ParseDouble(ps[6], "TextString.height");
            var w = ParseDouble(ps[7], "TextString.width");
            var spc = ParseDouble(ps[8], "TextString.spc");
            var angle = ParseDouble(ps[9], "TextString.angle");
            var slant = ParseDouble(ps[10], "TextString.slant");
            var bpnt = ParseInt(ps[11], "TextString.b_pnt");
            var direct = ParseInt(ps[12], "TextString.direct");
            var s = new SxfTextShape(
                layer, color, font, str, x0, y0, h, w, spc, angle, slant, bpnt, direct);
            var a = s.ToString();
            mShapeBuffer.Add(s);
        }
        void ParseSplineFeature(List<object> ps)
        {
            CheckParameterSize(ps, 8, "Spline");
            var layer = ParseInt(ps[0], "Spline.layer");
            var color = ParseInt(ps[1], "Spline.color");
            var lineType = ParseInt(ps[2], "Spline.type");
            var lineWidth = ParseInt(ps[3], "Spline.lineWidth");
            var openClose = ParseInt(ps[4], "Spline.number");
            var n = ParseInt(ps[5], "Spline.number");
            var vx = SfcReader.ParseDoubleList(ps[6], "Spline.vertex_x");
            var vy = SfcReader.ParseDoubleList(ps[7], "Spline.vertex_y");
            CheckVertexSize(vx, vy, n, "Spline");
            var s = new SxfSplineShape(layer, color, lineType, lineWidth, vx, vy);
            mShapeBuffer.Add(s);
        }

        void ParseClothoidFeature(List<object> ps)
        {
            CheckParameterSize(ps, 11, "Clothoid");
            var layer = ParseInt(ps[0], "Clothoid.layer");
            var color = ParseInt(ps[1], "Clothoid.color");
            var lineType = ParseInt(ps[2], "Clothoid.type");
            var lineWidth = ParseInt(ps[3], "Clothoid.lineWidth");
            var x = ParseDouble(ps[4], "Clothoid.x");
            var y = ParseDouble(ps[5], "Clothoid.y");
            var param = ParseDouble(ps[6], "Clothoid.parameter");
            var direct = ParseInt(ps[7], "Clothoid.direct");
            var angle = ParseDouble(ps[8], "Clothoid.angle");
            var start = ParseDouble(ps[9], "Clothoid.start_length");
            var end = ParseDouble(ps[10], "Clothoid.end_length");
            var s = new SxfClothoidShape(layer, color, lineType, lineWidth, x, y, param, direct, angle, start, end);
            mShapeBuffer.Add(s);
        }

        void ParseSfigLocateFeature(List<object> ps)
        {
            CheckParameterSize(ps, 7, "SfigLocate");
            var layer = ParseInt(ps[0], "SfigLocate.layer");
            var name = ParseString(ps[1], "SfigLocate.name");
            var x = ParseDouble(ps[2], "SfigLocate.x");
            var y = ParseDouble(ps[3], "SfigLocate.y");
            var angle = ParseDouble(ps[4], "SfigLocate.angle");
            var ratioX = ParseDouble(ps[5], "SfigLocate.ratio_x");
            var ratioY = ParseDouble(ps[6], "SfigLocate.ratio_y");
            var s = new SxfSfiglocShape(layer, name, x, y, angle, ratioX, ratioY);
            mShapeBuffer.Add(s);
        }

        void ParseExternallyDefinedSymbolFeature(List<object> ps)
        {
            CheckParameterSize(ps, 8, "ExternallyDefinedSymbol");
            var layer = ParseInt(ps[0], "ExternallyDefinedSymbol.layer");
            var colorFlag = ParseInt(ps[1], "ExternallyDefinedSymbol.colorFlag");
            var color = ParseInt(ps[2], "ExternallyDefinedSymbol.color");
            var name = ParseString(ps[3], "ExternallyDefinedSymbol.name");
            var x = ParseDouble(ps[4], "ExternallyDefinedSymbol.x");
            var y = ParseDouble(ps[5], "ExternallyDefinedSymbolthoid.y");
            var angle = ParseDouble(ps[6], "ExternallyDefinedSymbol.angle");
            var scale = ParseDouble(ps[7], "ExternallyDefinedSymbol.scale");
            var s = new SxfExternallyDefinedSymbolShape(layer, colorFlag, color, name, x, y, angle, scale);
            mShapeBuffer.Add(s);
        }

        void ParseLinearDimFeature(List<object> ps)
        {
            CheckParameterSize(ps, 44, "LinearDim");
            var layer = ParseInt(ps[0], "LinearDim.layer");
            var color = ParseInt(ps[1], "LinearDim.color");
            var lineType = ParseInt(ps[2], "LinearDim.type");
            var lineWidth = ParseInt(ps[3], "LinearDim.lineWidth");
            var sunX1 = ParseDouble(ps[4], "LinearDim.sunX1");
            var sunY1 = ParseDouble(ps[5], "LinearDim.sunY1");
            var sunX2 = ParseDouble(ps[6], "LinearDim.sunX2");
            var sunY2 = ParseDouble(ps[7], "LinearDim.sunY2");
            var flag2 = ParseInt(ps[8], "LinearDim.flag2");
            var ho1X0 = ParseDouble(ps[9], "LinearDim.ho1X0");
            var ho1Y0 = ParseDouble(ps[10], "LinearDim.ho1Y0");
            var ho1X1 = ParseDouble(ps[11], "LinearDim.ho1X1");
            var ho1Y1 = ParseDouble(ps[12], "LinearDim.ho1Y1");
            var ho1X2 = ParseDouble(ps[13], "LinearDim.ho1X2");
            var ho1Y2 = ParseDouble(ps[14], "LinearDim.ho1Y2");
            var flag3 = ParseInt(ps[15], "LinearDim.flag3");
            var ho2X0 = ParseDouble(ps[16], "LinearDim.ho2X0");
            var ho2Y0 = ParseDouble(ps[17], "LinearDim.ho2Y0");
            var ho2X1 = ParseDouble(ps[18], "LinearDim.ho2X1");
            var ho2Y1 = ParseDouble(ps[19], "LinearDim.ho2Y1");
            var ho2X2 = ParseDouble(ps[20], "LinearDim.ho2X2");
            var ho2Y2 = ParseDouble(ps[21], "LinearDim.ho2Y2");
            var arr1Code1 = ParseInt(ps[22], "LinearDim.arr1Code1");
            var arr1Code2 = ParseInt(ps[23], "LinearDim.arr1Code2");
            var arr1X = ParseDouble(ps[24], "LinearDim.arr1X");
            var arr1Y = ParseDouble(ps[25], "LinearDim.arr1Y");
            var arr1R = ParseDouble(ps[26], "LinearDim.arr1R");
            var arr2Code1 = ParseInt(ps[27], "LinearDim.arr2Code1");
            var arr2Code2 = ParseInt(ps[28], "LinearDim.arr2Code2");
            var arr2X = ParseDouble(ps[29], "LinearDim.arr2X");
            var arr2Y = ParseDouble(ps[30], "LinearDim.arr2Y");
            var arr2R = ParseDouble(ps[31], "LinearDim.arr2R");
            var flag4 = ParseInt(ps[32], "LinearDim.flag4");
            var font = ParseInt(ps[33], "LinearDim.font");
            var str = ParseString(ps[34], "LinearDim.str");
            var textX = ParseDouble(ps[35], "LinearDim.textX");
            var textY = ParseDouble(ps[36], "LinearDim.textY");
            var height = ParseDouble(ps[37], "LinearDim.height");
            var width = ParseDouble(ps[38], "LinearDim.width");
            var spc = ParseDouble(ps[39], "LinearDim.spc");
            var angle = ParseDouble(ps[40], "LinearDim.angle");
            var slant = ParseDouble(ps[41], "LinearDim.slant");
            var bpnt = ParseInt(ps[42], "LinearDim.brnt");
            var direct = ParseInt(ps[43], "LinearDim.direct");
            var s = new SxfLinearDimShape(
                layer, color, lineType, lineWidth,
                sunX1, sunY1, sunX2, sunY2, flag2,
                ho1X0, ho1Y0, ho1X1, ho1Y1, ho1X2, ho1Y2, flag3,
                ho2X0, ho2Y0, ho2X1, ho2Y1, ho2X2, ho2Y2,
                arr1Code1, arr1Code2, arr1X, arr1Y, arr1R,
                arr2Code1, arr2Code2, arr2X, arr2Y, arr2R, flag4,
                font, str, textX, textY, height, width,
                spc, angle, slant, bpnt, direct);
            mShapeBuffer.Add(s);
        }

        void ParseCurveDimFeature(List<object> ps)
        {
            CheckParameterSize(ps, 45, "CurveDim");
            var layer = ParseInt(ps[0], "CurveDim.layer");
            var color = ParseInt(ps[1], "CurveDim.color");
            var lineType = ParseInt(ps[2], "CurveDim.type");
            var lineWidth = ParseInt(ps[3], "CurveDim.lineWidth");
            var sunX = ParseDouble(ps[4], "CurveDim.sunX");
            var sunY = ParseDouble(ps[5], "CurveDim.sunY");
            var radius = ParseDouble(ps[6], "CurveDim.radius");
            var angle0 = ParseDouble(ps[7], "CurveDim.angle0");
            var angle1 = ParseDouble(ps[8], "CurveDim.angle1");
            var flag2 = ParseInt(ps[9], "CurveDim.flag2");
            var ho1X0 = ParseDouble(ps[10], "CurveDim.ho1X0");
            var ho1Y0 = ParseDouble(ps[11], "CurveDim.ho1Y0");
            var ho1X1 = ParseDouble(ps[12], "CurveDim.ho1X1");
            var ho1Y1 = ParseDouble(ps[13], "CurveDim.ho1Y1");
            var ho1X2 = ParseDouble(ps[14], "CurveDim.ho1X2");
            var ho1Y2 = ParseDouble(ps[15], "CurveDim.ho1Y2");
            var flag3 = ParseInt(ps[16], "CurveDim.flag3");
            var ho2X0 = ParseDouble(ps[17], "CurveDim.ho2X0");
            var ho2Y0 = ParseDouble(ps[18], "CurveDim.ho2Y0");
            var ho2X1 = ParseDouble(ps[19], "CurveDim.ho2X1");
            var ho2Y1 = ParseDouble(ps[20], "CurveDim.ho2Y1");
            var ho2X2 = ParseDouble(ps[21], "CurveDim.ho2X2");
            var ho2Y2 = ParseDouble(ps[22], "CurveDim.ho2Y2");
            var arr1Code1 = ParseInt(ps[23], "CurveDim.arr1Code1");
            var arr1Code2 = ParseInt(ps[24], "CurveDim.arr1Code2");
            var arr1X = ParseDouble(ps[25], "CurveDim.arr1X");
            var arr1Y = ParseDouble(ps[26], "CurveDim.arr1Y");
            var arr1R = ParseDouble(ps[27], "CurveDim.arr1R");
            var arr2Code1 = ParseInt(ps[28], "CurveDim.arr2Code1");
            var arr2Code2 = ParseInt(ps[29], "CurveDim.arr2Code2");
            var arr2X = ParseDouble(ps[30], "CurveDim.arr2X");
            var arr2Y = ParseDouble(ps[31], "CurveDim.arr2Y");
            var arr2R = ParseDouble(ps[32], "CurveDim.arr2R");
            var flag4 = ParseInt(ps[33], "CurveDim.flag4");
            var font = ParseInt(ps[34], "CurveDim.font");
            var str = ParseString(ps[35], "CurveDim.str");
            var textX = ParseDouble(ps[36], "CurveDim.textX");
            var textY = ParseDouble(ps[37], "CurveDim.textY");
            var height = ParseDouble(ps[38], "CurveDim.height");
            var width = ParseDouble(ps[39], "CurveDim.width");
            var spc = ParseDouble(ps[40], "CurveDim.spc");
            var angle = ParseDouble(ps[41], "CurveDim.angle");
            var slant = ParseDouble(ps[42], "CurveDim.slant");
            var bpnt = ParseInt(ps[43], "CurveDim.brnt");
            var direct = ParseInt(ps[44], "CurveDim.direct");
            var s = new SxfCurveDimShape(
                layer, color, lineType, lineWidth, sunX, sunY, radius, angle0,
                angle1, flag2, ho1X0, ho1Y0, ho1X1, ho1Y1,
                ho1X2, ho1Y2, flag3, ho2X0, ho2Y0, ho2X1, ho2Y1,
                ho2X2, ho2Y2, arr1Code1, arr1Code2, arr1X, arr1Y, arr1R, arr2Code1, arr2Code2,
                arr2X, arr2Y, arr2R, flag4, font, str, textX, textY,
                height, width, spc, angle, slant, bpnt, direct);
            mShapeBuffer.Add(s);
        }

        void ParseAngularDimFeature(List<object> ps)
        {
            CheckParameterSize(ps, 45, "AngularDim");
            var layer = ParseInt(ps[0], "AngularDim.layer");
            var color = ParseInt(ps[1], "AngularDim.color");
            var lineType = ParseInt(ps[2], "AngularDim.type");
            var lineWidth = ParseInt(ps[3], "AngularDim.lineWidth");
            var sunX = ParseDouble(ps[4], "AngularDim.sunX");
            var sunY = ParseDouble(ps[5], "AngularDim.sunY");
            var radius = ParseDouble(ps[6], "AngularDim.radius");
            var angle0 = ParseDouble(ps[7], "AngularDim.angle0");
            var angle1 = ParseDouble(ps[8], "AngularDim.angle1");
            var flag2 = ParseInt(ps[9], "AngularDim.flag2");
            var ho1X0 = ParseDouble(ps[10], "AngularDim.ho1X0");
            var ho1Y0 = ParseDouble(ps[11], "AngularDim.ho1Y0");
            var ho1X1 = ParseDouble(ps[12], "AngularDim.ho1X1");
            var ho1Y1 = ParseDouble(ps[13], "AngularDim.ho1Y1");
            var ho1X2 = ParseDouble(ps[14], "AngularDim.ho1X2");
            var ho1Y2 = ParseDouble(ps[15], "AngularDim.ho1Y2");
            var flag3 = ParseInt(ps[16], "AngularDim.flag3");
            var ho2X0 = ParseDouble(ps[17], "AngularDim.ho2X0");
            var ho2Y0 = ParseDouble(ps[18], "AngularDim.ho2Y0");
            var ho2X1 = ParseDouble(ps[19], "AngularDim.ho2X1");
            var ho2Y1 = ParseDouble(ps[20], "AngularDim.ho2Y1");
            var ho2X2 = ParseDouble(ps[21], "AngularDim.ho2X2");
            var ho2Y2 = ParseDouble(ps[22], "AngularDim.ho2Y2");
            var arr1Code1 = ParseInt(ps[23], "AngularDim.arr1Code1");
            var arr1Code2 = ParseInt(ps[24], "AngularDim.arr1Code2");
            var arr1X = ParseDouble(ps[25], "AngularDim.arr1X");
            var arr1Y = ParseDouble(ps[26], "AngularDim.arr1Y");
            var arr1R = ParseDouble(ps[27], "AngularDim.arr1R");
            var arr2Code1 = ParseInt(ps[28], "AngularDim.arr2Code1");
            var arr2Code2 = ParseInt(ps[29], "AngularDim.arr2Code2");
            var arr2X = ParseDouble(ps[30], "AngularDim.arr2X");
            var arr2Y = ParseDouble(ps[31], "AngularDim.arr2Y");
            var arr2R = ParseDouble(ps[32], "AngularDim.arr2R");
            var flag4 = ParseInt(ps[33], "AngularDim.flag4");
            var font = ParseInt(ps[34], "AngularDim.font");
            var str = ParseString(ps[35], "AngularDim.str");
            var textX = ParseDouble(ps[36], "AngularDim.textX");
            var textY = ParseDouble(ps[37], "AngularDim.textY");
            var height = ParseDouble(ps[38], "AngularDim.height");
            var width = ParseDouble(ps[39], "AngularDim.width");
            var spc = ParseDouble(ps[40], "AngularDim.spc");
            var angle = ParseDouble(ps[41], "AngularDim.angle");
            var slant = ParseDouble(ps[42], "AngularDim.slant");
            var bpnt = ParseInt(ps[43], "AngularDim.brnt");
            var direct = ParseInt(ps[44], "AngularDim.direct");
            var s = new SxfAngularDimShape(
                layer, color, lineType, lineWidth, sunX, sunY, radius, angle0,
                angle1, flag2, ho1X0, ho1Y0, ho1X1, ho1Y1,
                ho1X2, ho1Y2, flag3, ho2X0, ho2Y0, ho2X1, ho2Y1,
                ho2X2, ho2Y2, arr1Code1, arr1Code2, arr1X, arr1Y, arr1R, arr2Code1, arr2Code2,
                arr2X, arr2Y, arr2R, flag4, font, str, textX, textY,
                height, width, spc, angle, slant, bpnt, direct);
            mShapeBuffer.Add(s);
        }
        void ParseRadiusDimFeature(List<object> ps)
        {
            CheckParameterSize(ps, 25, "RadiusDim");
            var layer = ParseInt(ps[0], "RadiusDim.layer");
            var color = ParseInt(ps[1], "RadiusDim.color");
            var lineType = ParseInt(ps[2], "RadiusDim.type");
            var lineWidth = ParseInt(ps[3], "RadiusDim.lineWidth");
            var sunX1 = ParseDouble(ps[4], "RadiusDim.sunX1");
            var sunY1 = ParseDouble(ps[5], "RadiusDim.sunY1");
            var sunX2 = ParseDouble(ps[6], "RadiusDim.sunX2");
            var sunY2 = ParseDouble(ps[7], "RadiusDim.sunY2");
            var arrCode1 = ParseInt(ps[8], "RadiusDim.arrCode1");
            var arrCode2 = ParseInt(ps[9], "RadiusDim.arrCode2");
            var arrX = ParseDouble(ps[10], "RadiusDim.arrX");
            var arrY = ParseDouble(ps[11], "RadiusDim.arrY");
            var arrR = ParseDouble(ps[12], "RadiusDim.arrR");
            var flag = ParseInt(ps[13], "RadiusDim.flag");
            var font = ParseInt(ps[14], "RadiusDim.font");
            var str = ParseString(ps[15], "RadiusDim.str");
            var textX = ParseDouble(ps[16], "RadiusDim.textX");
            var textY = ParseDouble(ps[17], "RadiusDim.textY");
            var height = ParseDouble(ps[18], "RadiusDim.height");
            var width = ParseDouble(ps[19], "RadiusDim.width");
            var spc = ParseDouble(ps[20], "RadiusDim.spc");
            var angle = ParseDouble(ps[21], "RadiusDim.angle");
            var slant = ParseDouble(ps[22], "RadiusDim.slant");
            var bpnt = ParseInt(ps[23], "RadiusDim.brnt");
            var direct = ParseInt(ps[24], "RadiusDim.direct");
            var s = new SxfRadiusDimShape(
                layer, color, lineType, lineWidth, sunX1, sunY1, sunX2, sunY2,
                arrCode1, arrCode2, arrX, arrY, arrR, flag, font, str, textX, textY,
                height, width, spc, angle, slant, bpnt, direct);
            mShapeBuffer.Add(s);
        }

        void ParseDiameterDimFeature(List<object> ps)
        {
            CheckParameterSize(ps, 30, "DiameterDim");
            var layer = ParseInt(ps[0], "DiameterDim.layer");
            var color = ParseInt(ps[1], "DiameterDim.color");
            var lineType = ParseInt(ps[2], "DiameterDim.type");
            var lineWidth = ParseInt(ps[3], "DiameterDim.lineWidth");
            var sunX1 = ParseDouble(ps[4], "DiameterDim.sunX1");
            var sunY1 = ParseDouble(ps[5], "DiameterDim.sunY1");
            var sunX2 = ParseDouble(ps[6], "DiameterDim.sunX2");
            var sunY2 = ParseDouble(ps[7], "DiameterDim.sunY2");
            var arr1Code1 = ParseInt(ps[8], "DiameterDim.arr1Code1");
            var arr1Code2 = ParseInt(ps[9], "DiameterDim.arr1Code2");
            var arr1X = ParseDouble(ps[10], "DiameterDim.arr1X");
            var arr1Y = ParseDouble(ps[11], "DiameterDim.arr1Y");
            var arr1R = ParseDouble(ps[12], "DiameterDim.arr1R");
            var arr2Code1 = ParseInt(ps[13], "DiameterDim.arr2Code1");
            var arr2Code2 = ParseInt(ps[14], "DiameterDim.arr2Code2");
            var arr2X = ParseDouble(ps[15], "DiameterDim.arr2X");
            var arr2Y = ParseDouble(ps[16], "DiameterDim.arr2Y");
            var arr2R = ParseDouble(ps[17], "DiameterDim.arr2R");
            var flag = ParseInt(ps[18], "DiameterDim.flag");
            var font = ParseInt(ps[19], "DiameterDim.font");
            var str = ParseString(ps[20], "DiameterDim.str");
            var textX = ParseDouble(ps[21], "DiameterDim.textX");
            var textY = ParseDouble(ps[22], "DiameterDim.textY");
            var height = ParseDouble(ps[23], "DiameterDim.height");
            var width = ParseDouble(ps[24], "DiameterDim.width");
            var spc = ParseDouble(ps[25], "DiameterDim.spc");
            var angle = ParseDouble(ps[26], "DiameterDim.angle");
            var slant = ParseDouble(ps[27], "DiameterDim.slant");
            var bpnt = ParseInt(ps[28], "DiameterDim.brnt");
            var direct = ParseInt(ps[29], "DiameterDim.direct");
            var s = new SxfDiameterDimShape(
                layer, color, lineType, lineWidth, sunX1, sunY1, sunX2, sunY2,
                arr1Code1, arr1Code2, arr1X, arr1Y, arr1R,
                arr2Code1, arr2Code2, arr2X, arr2Y, arr2R, flag,
                font, str, textX, textY, height, width, spc, angle, slant, bpnt, direct);
            mShapeBuffer.Add(s);
        }

        void ParseLabelFeature(List<object> ps)
        {
            CheckParameterSize(ps, 21, "Label");
            var layer = ParseInt(ps[0], "Label.layer");
            var color = ParseInt(ps[1], "Label.color");
            var lineType = ParseInt(ps[2], "Label.type");
            var lineWidth = ParseInt(ps[3], "Label.lineWidth");
            var n = ParseInt(ps[4], "Label.number");
            var vx = ParseDoubleList(ps[5], "Label.vertex_x");
            var vy = ParseDoubleList(ps[6], "Label.vertex_y");
            var arrCode = ParseInt(ps[7], "Label.arrCode");
            var arrR = ParseDouble(ps[8], "Label.arrR");
            var flag = ParseInt(ps[9], "Label.flag");
            var font = ParseInt(ps[10], "Label.font");
            var str = ParseString(ps[11], "Label.str");
            var textX = ParseDouble(ps[12], "Label.textX");
            var textY = ParseDouble(ps[13], "Label.textY");
            var height = ParseDouble(ps[14], "Label.height");
            var width = ParseDouble(ps[15], "Label.width");
            var spc = ParseDouble(ps[16], "Label.spc");
            var angle = ParseDouble(ps[17], "Label.angle");
            var slant = ParseDouble(ps[18], "Label.slant");
            var bpnt = ParseInt(ps[19], "Label.brnt");
            var direct = ParseInt(ps[20], "Label.direct");
            CheckVertexSize(vx, vy, n, "Label");
            var s = new SxfLabelShape(
                layer, color, lineType, lineWidth, vx, vy, arrCode, arrR, flag, font, str, textX, textY, height, width, spc, angle, slant, bpnt, direct);
            mShapeBuffer.Add(s);
        }

        void ParseBalloonFeature(List<object> ps)
        {
            CheckParameterSize(ps, 24, "Balloon");
            var layer = ParseInt(ps[0], "Balloon.layer");
            var color = ParseInt(ps[1], "Balloon.color");
            var lineType = ParseInt(ps[2], "Balloon.type");
            var lineWidth = ParseInt(ps[3], "Balloon.lineWidth");
            var n = ParseInt(ps[4], "Balloon.number");
            var vx = ParseDoubleList(ps[5], "Balloon.vertex_x");
            var vy = ParseDoubleList(ps[6], "Balloon.vertex_y");
            var x0 = ParseDouble(ps[7], "Balloon.center_x");
            var y0 = ParseDouble(ps[8], "Balloon.center_y");
            var r = ParseDouble(ps[9], "Balloon.radius");
            var arrCode = ParseInt(ps[10], "Balloon.arrCode");
            var arrR = ParseDouble(ps[11], "Balloon.arrR");
            var flag = ParseInt(ps[12], "Balloon.flag");
            var font = ParseInt(ps[13], "Balloon.font");
            var str = ParseString(ps[14], "Balloon.str");
            var textX = ParseDouble(ps[15], "Balloon.textX");
            var textY = ParseDouble(ps[16], "Balloon.textY");
            var height = ParseDouble(ps[17], "Balloon.height");
            var width = ParseDouble(ps[18], "Balloon.width");
            var spc = ParseDouble(ps[19], "Balloon.spc");
            var angle = ParseDouble(ps[20], "Balloon.angle");
            var slant = ParseDouble(ps[21], "Balloon.slant");
            var bpnt = ParseInt(ps[22], "Balloon.brnt");
            var direct = ParseInt(ps[23], "Balloon.direct");
            CheckVertexSize(vx, vy, n, "Balloon");
            var s = new SxfBalloonShape(
                layer, color, lineType, lineWidth, vx, vy, x0, y0, r, 
                arrCode, arrR, flag, font, str, textX, textY, height, width, 
                spc, angle, slant, bpnt, direct);
            mShapeBuffer.Add(s);
        }

        void ParseExternallyDefinedHatchFeature(List<object> ps)
        {
            CheckParameterSize(ps, 5, "ExternallyDefinedHatch");
            var layer = ParseInt(ps[0], "ExternallyDefinedHatch.layer");
            var name = ParseString(ps[1], "ExternallyDefinedHatch.name");
            var outId = ParseInt(ps[2], "ExternallyDefinedHatch.outId");
            var _ = ParseInt(ps[3], "ExternallyDefinedHatch.number");
            var inId = ParseIntList(ps[4], "ExternallyDefinedHatch.inId");
            var s = new SxfExternallyDefinedHatchShape(layer, name, outId, inId);
            mShapeBuffer.Add(s);
        }

        void ParseFillAreaStyleColorFeature(List<object> ps)
        {
            CheckParameterSize(ps, 5, "FillAreaStyleColor");
            var layer = ParseInt(ps[0], "FillAreaStyleColor.layer");
            var color = ParseInt(ps[1], "FillAreaStyleColor.color");
            var outId = ParseInt(ps[2], "FillAreaStyleColor.outId");
            var _ = ParseInt(ps[3], "FillAreaStyleColor.number");
            var inId = ParseIntList(ps[4], "FillAreaStyleColor.inId");
            var s = new SxfFillAreaStyleColorShape(layer, color, outId, inId);
            mShapeBuffer.Add(s);
        }

        void ParseFillAreaStyleHatchingFeature(List<object> ps)
        {
            CheckParameterSize(ps, 6, "FillAreaStyleHatching");
            var layer = ParseInt(ps[0], "FillAreaStyleHatching.layer");
            int n = ParseInt(ps[1], "FillAreaStyleHatching.number");
            var hs = new List<HatchingPattern>();
            for(int i = 0; i < n; i++)
            {
                hs.Add(ParseHatchingPattern(ps[2 + i]));
            }
            var outId = ParseInt(ps[2+n], "FillAreaStyleHatching.outId");
            var _ = ParseInt(ps[3+n], "FillAreaStyleHatching.number");
            var inId = ParseIntList(ps[4+n], "FillAreaStyleHatching.inId");
            var s = new SxfFillAreaStyleHatchingShape(layer, hs, outId, inId);
            mShapeBuffer.Add(s);
        }

        void ParseFillAreaStyleTilesFeature(List<object> ps)
        {
            CheckParameterSize(ps, 15, "FillAreaStyleTiles");
            var layer = ParseInt(ps[0], "FillAreaStyleTiles.layer");
            var name = ParseString(ps[1], "FillAreaStyleTiles.name");
            var color = ParseInt(ps[2], "FillAreaStyleTiles.color");
            var patternX = ParseDouble(ps[3], "FillAreaStyleTiles.patternX");
            var patternY = ParseDouble(ps[4], "FillAreaStyleTiles.patternY");
            var patternVector1 = ParseDouble(ps[5], "FillAreaStyleTiles.patternVector1");
            var patternVector1Angle = ParseDouble(ps[6], "FillAreaStyleTiles.patternVector1Angle");
            var patternVector2 = ParseDouble(ps[7], "FillAreaStyleTiles.patternVector2");
            var patternVecto2Angle = ParseDouble(ps[8], "FillAreaStyleTiles.patternVector2Angle");
            var patternScaleX = ParseDouble(ps[9], "FillAreaStyleTiles.patternScaleX");
            var patternScaleY = ParseDouble(ps[10], "FillAreaStyleTiles.patternScaleY");
            var patternAngle = ParseDouble(ps[11], "FillAreaStyleTiles.patternAngle");
            var outId = ParseInt(ps[12], "FillAreaStyleTiles.outId");
            var _ = ParseInt(ps[13], "FillAreaStyleTiles.number");
            var inId = ParseIntList(ps[14], "FillAreaStyleTiles.inId");
            var s = new SxfFillAreaStyleTilesShape(layer, name, color, patternX, patternY, 
                patternVector1, patternVector1Angle, patternVector2, patternVecto2Angle, 
                patternScaleX, patternScaleY, patternAngle, outId, inId);
            mShapeBuffer.Add(s);
        }


        bool ReadHaeder(TextReader r)
        {
            //最初に"ISO-10303-21;"があればSXF
            while (true)
            {
                var t = r.ReadLine();
                if (t == null) return false;
                t = t.Trim();
                if (t == "") continue;
                if (t != "ISO-10303-21;") return false;
                break;
            }
            var tokenizer = new Tokenizer(r);
            var tok = tokenizer.GetNextToken();
            if (tok.Value != "HEADER") return false;
            tok = tokenizer.GetNextToken();
            if (tok.Kind != Tokenizer.TokenKind.Semicolon) return false;
            while (true)
            {
                tok = tokenizer.GetNextToken();
                if (tok.Kind != Tokenizer.TokenKind.Identifier) return false;
                var tag = tok.Value;
                var ps = GetParams(tokenizer);
                tok = tokenizer.GetNextToken();
                if (tok.Kind != Tokenizer.TokenKind.Semicolon) return false;
                switch (tag)
                {
                    case "FILE_SCHEMA":
                        {
                            if (ps.Count == 0) return false;
                            if (ps[0] is not List<object> sa) return false;
                            if (sa.Count == 0) return false;
                            if (sa[0] is not string s) return false;
                            if (s != "ASSOCIATIVE_DRAUGHTING") return false;
                        }
                        break;
                    case "FILE_DESCRIPTION":
                        {
                            if (ps.Count < 2) return false;
                            var s = GetSingleStringFromList(ps[0]);
                            if (s == null) return false;
                            var sp = s.Split(' ');
                            if (sp.Length < 3) return false;
                            if (sp[0] != "SCADEC") return false;
                            if (!sp[1].StartsWith("level")) return false;
                            if (!int.TryParse(sp[1].Substring(5), out int level)) return false;
                            mDoc.Header.Level = level;
                            //only support sfc, "feature_mode"
                            if (sp[2] != "feature_mode") return false;
                        }
                        break;
                    case "FILE_NAME":
                        {
                            if (ps.Count < 7) return false;
                            if (ps[0] is not string s0) return false;
                            mDoc.Header.FileName = s0;
                            if (ps[1] is not string s1) return false;
                            mDoc.Header.TimeStamp = s1;
                            var s2 = GetSingleStringFromList(ps[2]);
                            if (s2 == null) return false;
                            mDoc.Header.Author = s2;
                            var s3 = GetSingleStringFromList(ps[3]);
                            if (s3 == null) return false;
                            mDoc.Header.Organization = s3;
                            if (ps[4] is not string s4) return false;
                            mDoc.Header.PreprocessorVersion = s4;
                            if (ps[5] is not string s5) return false;
                            mDoc.Header.TranslatorName = s5;
                        }
                        break;
                    case "ENDSEC":
                        {
                            return true;
                        }
                    default:
                        {
                        }
                        break;
                }
            }
        }

        string? GetSingleStringFromList(object obj)
        {
            if (obj is not List<object> sa1) return null;
            if (sa1.Count == 0) return null;
            if (sa1[0] is not string s3) return null;
            return s3;
        }

        void Skip(Tokenizer tokenizer)
        {
            var tok = tokenizer.GetNextToken();
            if (tok.Kind == Tokenizer.TokenKind.Semicolon)
            {
                tokenizer.PushToken(tok);
            }
            if (tok.Kind != Tokenizer.TokenKind.LPar) throw new Exception("Could not find '('.");
            while (true)
            {
                tok = tokenizer.GetNextToken();
                switch (tok.Kind)
                {
                    case Tokenizer.TokenKind.LPar:
                        {
                            tokenizer.PushToken(tok);
                            Skip(tokenizer);
                        }
                        break;
                    case Tokenizer.TokenKind.Parameter:
                    case Tokenizer.TokenKind.String:
                        {
                        }
                        break;
                    default: throw new Exception("Counld not find param.");
                }
                tok = tokenizer.GetNextToken();
                if (tok.Kind == Tokenizer.TokenKind.RPar)
                {
                    break;
                }
                if (tok.Kind != Tokenizer.TokenKind.Comma) throw new Exception("Counld not find separator ','.");
            }
        }

        internal static void CheckParameterSize(IReadOnlyList<object> ps, int size, string name)
        {
            if (ps.Count < size) throw new Exception($"{name} parameter size is invalid.({ps.Count}< {size})");
        }

        internal static void CheckVertexSize(IReadOnlyList<double> vx, IReadOnlyList<double> vy, int size, string name)
        {
            if (vx.Count != size || vy.Count != size) throw new Exception($"{name} vertex size is invalid.(vx={vx.Count} vy={vy.Count} n={size})");
        }

        List<object> GetParams(Tokenizer tokenizer)
        {
            var ret = new List<object>();
            var tok = tokenizer.GetNextToken();
            if (tok.Kind == Tokenizer.TokenKind.Semicolon)
            {
                tokenizer.PushToken(tok);
                return ret;
            }
            if (tok.Kind != Tokenizer.TokenKind.LPar) throw new Exception("Could not find '('.");
            while (true)
            {
                tok = tokenizer.GetNextToken();
                switch (tok.Kind)
                {
                    case Tokenizer.TokenKind.LPar:
                        {
                            tokenizer.PushToken(tok);
                            var ps = GetParams(tokenizer);
                            ret.Add(ps);
                        }
                        break;
                    case Tokenizer.TokenKind.Parameter:
                    case Tokenizer.TokenKind.String:
                        {
                            ret.Add(tok.Value);
                        }
                        break;
                    default: throw new Exception("Counld not find param.");

                }
                tok = tokenizer.GetNextToken();
                if (tok.Kind == Tokenizer.TokenKind.RPar)
                {
                    break;
                }
                if (tok.Kind != Tokenizer.TokenKind.Comma) throw new Exception("Counld not find separator ','.");
            }
            return ret;
        }

        internal static string ParseString(object obj, string name)
        {
            if (obj is not string s) throw new Exception($"{name} parameter is invalid.({obj})");
            return s;
        }

        internal static int ParseInt(object obj, string name)
        {
            if (!TryParseInt(obj, out var n)) throw new Exception($"{name} parameter is invalid.({obj})");
            return n;
        }

        internal static bool TryParseInt(object obj, out int x)
        {
            x = 0;
            if (obj is not string s) return false;
            return int.TryParse(s, out x);
        }

        internal static double ParseDouble(object obj, string name)
        {
            if (!TryParseDouble(obj, out var a)) throw new Exception($"{name} parameter is invalid.({obj})");
            return a;
        }

        internal static bool TryParseDouble(object obj, out double x)
        {
            x = 0;
            if (obj is not string s) return false;
            return double.TryParse(s, out x);
        }

        internal static List<double> ParseDoubleList(object obj, string name)
        {
            var src = SfcReader.ParseString(obj, name);
            List<double> ret = new List<double>();
            var s = src.Trim();
            if (s[0] != '(') throw new Exception($"{name} list is not start with '('");
            if (s[^1] != ')') throw new Exception($"{name} list is not end with '('");
            s = s[1..^1];
            var sa = s.Split(",");
            foreach (var a in sa)
            {
                if (!double.TryParse(a, out double d)) throw new Exception($"{name} list item is not double.({a})");
                ret.Add(d);
            }
            return ret;
        }

        internal static List<int> ParseIntList(object obj, string name)
        {
            var src = SfcReader.ParseString(obj, name);
            List<int> ret = new ();
            var s = src.Trim();
            if (s[0] != '(') throw new Exception($"{name} list is not start with '('");
            if (s[^1] != ')') throw new Exception($"{name} list is not end with '('");
            s = s[1..^1];
            s = s.Trim();
            if(s != "")
            {
                var sa = s.Split(",");
                foreach (var a in sa)
                {
                    if (!int.TryParse(a, out var d)) throw new Exception($"{name} list item is not int.({a})");
                    ret.Add(d);
                }
            }
            return ret;
        }

        internal static HatchingPattern ParseHatchingPattern(object obj)
        {
            var src = SfcReader.ParseString(obj, "HatchingPattern");
            var s = src.Trim();
            if (s[0] != '(') throw new Exception($"HatchingPattern is not start with '('");
            if (s[^1] != ')') throw new Exception($"HatchingPattern is not end with '('");
            s = s[1..^1];
            var sa = s.Split(",");
            if(sa.Length != 7) throw new Exception($"HatchingPattern invalid size.");
            if (!int.TryParse(sa[0], out var color)) throw new Exception($"HatchingPattern item is not int.({sa[0]})");
            if (!int.TryParse(sa[1], out var lineType)) throw new Exception($"HatchingPattern item is not int.({sa[1]})");
            if (!int.TryParse(sa[2], out var lineWidth)) throw new Exception($"HatchingPattern item is not int.({sa[2]})");
            if (!double.TryParse(sa[3], out var startX)) throw new Exception($"HatchingPattern item is not double.({sa[3]})");
            if (!double.TryParse(sa[4], out var startY)) throw new Exception($"HatchingPattern item is not double.({sa[4]})");
            if (!double.TryParse(sa[5], out var spacing)) throw new Exception($"HatchingPattern item is not double.({sa[5]})");
            if (!double.TryParse(sa[6], out var angle)) throw new Exception($"HatchingPattern item is not double.({sa[6]})");
            return new HatchingPattern(color, lineType, lineWidth, startX, startY, spacing, angle);
        }
    }
}
