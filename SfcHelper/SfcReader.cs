using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SfcHelper
{
    public class SfcReader
    {

        public SfcHeader Header { get; set; } = new();
        public SxfSheet Sheet { get; set; } = new("A3 Portrait", 3, 1, 420, 297);
        public SfcTable Table { get; private set; } = new();
        List<SxfShape> mShapeBuffer = new();



        public SfcReader()
        {
        }

        public void Read(string path)
        {
            Table = new();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using var r = new StreamReader(path, Encoding.GetEncoding("shift_jis"));
            if (!ReadHaeder(r)) throw new Exception("Header error");
            ReadFeature(r);
        }

        bool CheckStartOfDataSection(TextReader r)
        {
            var tokenizer = new Tokenizer(r);
            while (true)
            {
                var tok = tokenizer.GetNextToken();
                if (tok.Kind != Tokenizer.TokenKind.Identifier) return false;
                if (tok.Value == "DATA")
                {
                    tok = tokenizer.GetNextToken();
                    return tok.Kind == Tokenizer.TokenKind.Semicolon;
                }
                if (tok.IsEof) return false;
            }
        }
        public void ReadFeature(TextReader r)
        {
            if (!CheckStartOfDataSection(r)) throw new Exception("Could not find DATA section");
            var sb = new StringBuilder();
            while (true)
            {
                var s = r.ReadLine();
                if (s == null) return;
                s.Trim();
                if (s == "/*SXF" || s == "/*SXF3" || s == "/*SXF3.1")
                {
                    sb.Clear();
                    while (true)
                    {
                        s = r.ReadLine();
                        if (s == null) throw new Exception("Unexpected eol in comment feature.");
                        s.Trim();
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
                if (s.StartsWith("ENDSEC") && s.EndsWith(";")) return;
            }
        }

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
                case "layer_feature":
                    Table.ParseLayerFeature(ps);
                    break;
                case "pre_defined_font_feature":
                    Table.ParsePreDefinedLineTypeFeature(ps);
                    break;
                case "user_defined_font_feature":
                    Table.ParseUserDefinedLineTypeFeature(ps);
                    break;
                case "pre_defined_colour_feature":
                    Table.ParsePreDefinedColourFeature(ps);
                    break;
                case "user_defined_colour_feature":
                    Table.ParseUserDefinedColourFeature(ps);
                    break;
                case "width_feature":
                    Table.ParseWidthFeature(ps);
                    break;
                case "text_font_feature":
                    Table.ParseTextFontFeature(ps);
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
            Sheet = new(name, t, orient, width, height);
            Sheet.Shapes.AddRange(mShapeBuffer);
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
            var s = new SxfPolylineShape(layer, color, lineType, lineWidth, vx,vy);
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
            var s = new SxfEllipseShape(layer, color, lineType, lineWidth, x0, y0, rx,ry,angle);
            mShapeBuffer.Add(s);
        }

        void ParseEllipseArcFeature(List<object> ps)
        {
            CheckParameterSize(ps, 12, "Ellipse");
            var layer = ParseInt(ps[0], "Ellipse.layer");
            var color = ParseInt(ps[1], "Ellipse.color");
            var lineType = ParseInt(ps[2], "Ellipse.type");
            var lineWidth = ParseInt(ps[3], "Ellipse.lineWidth");
            var x0 = ParseDouble(ps[4], "Ellipse.center_x");
            var y0 = ParseDouble(ps[5], "Ellipse.center_y");
            var rx = ParseDouble(ps[6], "Ellipse.radius_x");
            var ry = ParseDouble(ps[7], "Ellipse.radius_y");
            var direction = ParseInt(ps[8], "Arc.direction");
            var angle = ParseDouble(ps[9], "Ellipse.angle");
            var sa = ParseDouble(ps[10], "Arc.start_angle");
            var ea = ParseDouble(ps[11], "Arc.end_angle");
            var s = new SxfEllipseArcShape(
                layer, color, lineType, lineWidth, x0, y0, rx, ry, direction, angle, sa, ea);
            mShapeBuffer.Add(s);
        }

        public bool ReadHaeder(TextReader r)
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
                            Header.Level = level;
                            //only support sfc, "feature_mode"
                            if (sp[2] != "feature_mode") return false;
                        }
                        break;
                    case "FILE_NAME":
                        {
                            if (ps.Count < 7) return false;
                            if (ps[0] is not string s0) return false;
                            Header.FileName = s0;
                            if (ps[1] is not string s1) return false;
                            Header.TimeStamp = s1;
                            var s2 = GetSingleStringFromList(ps[2]);
                            if (s2 == null) return false;
                            Header.Author = s2;
                            var s3 = GetSingleStringFromList(ps[3]);
                            if (s3 == null) return false;
                            Header.Organization = s3;
                            if (ps[4] is not string s4) return false;
                            Header.PreprocessorVersion = s4;
                            if (ps[5] is not string s5) return false;
                            Header.TranslatorName = s5;
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
            foreach(var a in sa)
            {
                if(!double.TryParse(a, out double d)) throw new Exception($"{name} list item is not double.({a})");
                ret.Add(d);
            }
            return ret;
        }
    }
}
