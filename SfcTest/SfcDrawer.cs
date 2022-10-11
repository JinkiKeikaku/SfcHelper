using SfcHelper;
using System.Diagnostics;

namespace SfcTest
{
    class SfcDrawer
    {
        SxfDocument mDoc;
        /// <summary>
        /// 図形の縮尺。SXFは縦横異縮にも対応しているが考えない。線幅、線種、矢印で使う。
        /// </summary>
        double mShapeScale = 1.0;
        public SfcDrawer(SxfDocument doc)
        {
            mDoc = doc;
        }

        public void OnDraw(Graphics g, DrawContext d)
        {
            var save = g.Save();
            g.TranslateTransform(
                0.0f + (float)d.PaperWidth / 2, 
                (float)d.PaperHeight + (float)d.PaperHeight / 2);
            OnDrawPaper(g, d);
            foreach (var sxfShape in mDoc.Shapes)
            {
                OnDrawShape(g, d, sxfShape);
            }
            g.Restore(save);
        }
        void OnDrawPaper(Graphics g, DrawContext d)
        {
            var p0 = d.DocToCanvas(0, 0);
            var p1 = d.DocToCanvas(d.PaperWidth, d.PaperHeight);
            g.FillRectangle(Brushes.White, p0.X, p1.Y, (int)d.PaperWidth, (int)d.PaperHeight);
        }
        void OnDrawShape(Graphics g, DrawContext d, SxfShape shape)
        {
            switch (shape)
            {
                case SxfLineShape s: OnDrawLine(g, d, s); break;
                case SxfPolylineShape s: OnDrawPolyline(g, d, s); break;
                case SxfCircleShape s: OnDrawCircle(g, d, s); break;
                case SxfArcShape s: OnDrawArc(g, d, s); break;
                case SxfEllipseShape s: OnDrawEllipse(g, d, s); break;
                case SxfEllipseArcShape s: OnDrawEllipseArc(g, d, s); break;
                case SxfTextShape s: OnDrawText(g, d, s); break;
                case SxfSplineShape s: OnDrawSpline(g, d, s); break;
                case SxfLinearDimShape s: OnDrawLinearDim(g, d, s); break;
                case SxfRadiusDimShape s: OnDrawRadiusDim(g, d, s); break;
                case SxfDiameterDimShape s: OnDrawDiameterDim(g, d, s); break;
                case SxfAngularDimShape s: OnDrawAngularDim(g, d, s); break;
                case SxfLabelShape s: OnDrawLabel(g, d, s); break;
                case SxfBalloonShape s: OnDrawBalloon(g, d, s); break;
                case SxfFillAreaStyleColorShape s: OnDrawFillAreaStyleColor(g, d, s); break;
                case SxfFillAreaStyleHatchingShape s: OnDrawFillAreaStyleHatching(g, d, s); break;
                case SxfFillAreaStyleTilesShape s: OnDrawFillAreaStyleTiles(g, d, s); break;
                case SxfSfiglocShape s: OnDrawSfigloc(g, d, s); break;
            }
        }

        void OnDrawLine(Graphics g, DrawContext d, SxfLineShape shape)
        {
            ApplyLineStyle(d.Pen, shape.Color, shape.LineWidth, shape.LineType);
            var p0 = d.DocToCanvas(ConvertPoint(shape.StartX, shape.StartY));
            var p1 = d.DocToCanvas(ConvertPoint(shape.EndX, shape.EndY));
            g.DrawLine(d.Pen, p0, p1);
        }

        void OnDrawPolyline(Graphics g, DrawContext d, SxfPolylineShape shape)
        {
            DrawPolyline(g, d, shape, shape.Color, shape.LineWidth, shape.LineType);
        }

        void DrawPolyline(Graphics g, DrawContext d, SxfPolylineShape shape, int color, int lineWidth, int lineType)
        {
            ApplyLineStyle(d.Pen, color, lineWidth, lineType);
            var pts = new PointF[shape.VertexX.Count];
            for (var i = 0; i < shape.VertexX.Count; i++)
            {
                pts[i] = d.DocToCanvas(ConvertPoint(shape.VertexX[i], shape.VertexY[i]));
            }
            g.DrawLines(d.Pen, pts);
        }

        void OnDrawCircle(Graphics g, DrawContext d, SxfCircleShape shape)
        {
            ApplyLineStyle(d.Pen, shape.Color, shape.LineWidth, shape.LineType);
            var p0 = d.DocToCanvas(ConvertPoint(shape.CenterX, shape.CenterY));
            var rx = d.DocToCanvas(ConvertLength(shape.Radius));
            var ry = rx;
            var saved = g.Save();
            g.TranslateTransform(p0.X, p0.Y);
            g.DrawEllipse(d.Pen, -rx, -ry, rx * 2, ry * 2);
            g.Restore(saved);
        }

        void OnDrawArc(Graphics g, DrawContext d, SxfArcShape shape)
        {
            DrawArc(g, d, shape, shape.Color, shape.LineWidth, shape.LineType);
        }

        void DrawArc(Graphics g, DrawContext d, SxfArcShape shape, int color, int lineWidth, int lineType)
        {
            ApplyLineStyle(d.Pen, color, lineWidth, lineType);
            var p0 = d.DocToCanvas(ConvertPoint(shape.CenterX, shape.CenterY));
            var rx = d.DocToCanvas(ConvertLength(shape.Radius));
            var ry = rx;
            var sa = shape.StartAngle;
            var ea = shape.EndAngle;
            var (start, sweep) = shape.Direction == 0 ? (sa, ea - sa) : (ea, ea - sa);
            if (sweep < 0) sweep += 360;
            var saved = g.Save();
            g.TranslateTransform(p0.X, p0.Y);
            g.DrawArc(d.Pen, -rx, -ry, rx * 2, ry * 2, d.DocToCanvasAngle(start), d.DocToCanvasAngle(sweep));
            g.Restore(saved);
        }

        void OnDrawEllipse(Graphics g, DrawContext d, SxfEllipseShape shape)
        {
            ApplyLineStyle(d.Pen, shape.Color, shape.LineWidth, shape.LineType);
            var p0 = d.DocToCanvas(ConvertPoint(shape.CenterX, shape.CenterY));
            var rx = d.DocToCanvas(ConvertLength(shape.RadiusX));
            var ry = d.DocToCanvas(ConvertLength(shape.RadiusY));
            var saved = g.Save();
            g.TranslateTransform(p0.X, p0.Y);
            g.RotateTransform(d.DocToCanvasAngle(shape.RotateAngle));
            g.DrawEllipse(d.Pen, -rx, -ry, rx * 2, ry * 2);
            g.Restore(saved);
        }

        void OnDrawEllipseArc(Graphics g, DrawContext d, SxfEllipseArcShape shape)
        {
            DrawEllipseArc(g, d, shape, shape.Color, shape.LineWidth, shape.LineType);
        }

        void DrawEllipseArc(Graphics g, DrawContext d, SxfEllipseArcShape shape, int color, int lineWidth, int lineType)
        {
            ApplyLineStyle(d.Pen, color, lineWidth, lineType);
            var p0 = d.DocToCanvas(ConvertPoint(shape.CenterX, shape.CenterY));
            var rx = d.DocToCanvas(ConvertLength(shape.RadiusX));
            var ry = d.DocToCanvas(ConvertLength(shape.RadiusY));
            var sa = shape.StartAngle;
            var ea = shape.EndAngle;
            var (start, sweep) = shape.Direction == 0 ? (sa, ea - sa) : (ea, ea - sa);
            if (sweep < 0) sweep += 360;
            var saved = g.Save();
            g.TranslateTransform(p0.X, p0.Y);
            g.RotateTransform(d.DocToCanvasAngle(shape.RotateAngle));
            g.DrawArc(d.Pen, -rx, -ry, rx * 2, ry * 2, d.DocToCanvasAngle(start), d.DocToCanvasAngle(sweep));
            g.Restore(saved);
        }

        void OnDrawSpline(Graphics g, DrawContext d, SxfSplineShape shape)
        {
            DrawSpline(g, d, shape, shape.Color, shape.LineWidth, shape.LineType);
        }

        void DrawSpline(Graphics g, DrawContext d, SxfSplineShape shape, int color, int lineWidth, int lineType)
        {
            ApplyLineStyle(d.Pen, color, lineWidth, lineType);
            var pts = new PointF[shape.VertexX.Count];
            for (var i = 0; i < shape.VertexX.Count; i++)
            {
                pts[i] = d.DocToCanvas(ConvertPoint(shape.VertexX[i], shape.VertexY[i]));
            }
            g.DrawBeziers(d.Pen, pts);
        }

        /// <summary>
        /// 文字を書きます。文字の描画位置は正確ではありません。文字幅は文字間隔も考慮していません。
        /// </summary>
        void OnDrawText(Graphics g, DrawContext d, SxfTextShape shape)
        {
            DrawText(
                g, d, shape.TextX, shape.TextY, shape.Width, shape.Height, shape.Angle,
                shape.Str, shape.Font, shape.Direct, shape.BPnt, shape.Color);
        }

        void OnDrawLinearDim(Graphics g, DrawContext d, SxfLinearDimShape shape)
        {
            ApplyLineStyle(d.Pen, shape.Color, shape.LineWidth, shape.LineType);
            var ho11 = d.DocToCanvas(ConvertPoint(shape.Ho1X1, shape.Ho1Y1));
            var ho12 = d.DocToCanvas(ConvertPoint(shape.Ho1X2, shape.Ho1Y2));
            var sun1 = d.DocToCanvas(ConvertPoint(shape.SunX1, shape.SunY1));
            var ho21 = d.DocToCanvas(ConvertPoint(shape.Ho2X1, shape.Ho2Y1));
            var ho22 = d.DocToCanvas(ConvertPoint(shape.Ho2X2, shape.Ho2Y2));
            var sun2 = d.DocToCanvas(ConvertPoint(shape.SunX2, shape.SunY2));

            if (shape.Flag2 == 1) g.DrawLine(d.Pen, ho11, ho12);
            if (shape.Flag3 == 1) g.DrawLine(d.Pen, ho21, ho22);
            g.DrawLine(d.Pen, sun1, sun2);
            DrawArrow(g, d, shape.Arr1X, shape.Arr1Y, shape.Arr1R, shape.Arr1Code2);
            DrawArrow(g, d, shape.Arr2X, shape.Arr2Y, shape.Arr2R, shape.Arr2Code2);
            if (shape.Flag4 == 1)
            {
                DrawText(g, d, shape.TextX, shape.TextY, shape.Width, shape.Height, shape.Angle,
                    shape.Str, shape.Font, shape.Direct, shape.BPnt, shape.Color);
            }
        }

        void OnDrawAngularDim(Graphics g, DrawContext d, SxfAngularDimShape shape)
        {
            ApplyLineStyle(d.Pen, shape.Color, shape.LineWidth, shape.LineType);
            var ho11 = d.DocToCanvas(ConvertPoint(shape.Ho1X1, shape.Ho1Y1));
            var ho12 = d.DocToCanvas(ConvertPoint(shape.Ho1X2, shape.Ho1Y2));
            var sun = d.DocToCanvas(ConvertPoint(shape.SunX, shape.SunY));
            var ho21 = d.DocToCanvas(ConvertPoint(shape.Ho2X1, shape.Ho2Y1));
            var ho22 = d.DocToCanvas(ConvertPoint(shape.Ho2X2, shape.Ho2Y2));
            if (shape.Flag2 == 1) g.DrawLine(d.Pen, ho11, ho12);
            if (shape.Flag3 == 1) g.DrawLine(d.Pen, ho21, ho22);
            var r = d.DocToCanvas(ConvertLength(shape.SunRadius));
            var sa = shape.SunAngle0;
            var ea = shape.SunAngle1;
            var (start, sweep) = (sa, ea - sa);
            if (sweep < 0) sweep += 360;
            var saved = g.Save();
            g.TranslateTransform(sun.X, sun.Y);
            g.DrawArc(d.Pen, -r, -r, r * 2, r * 2, d.DocToCanvasAngle(start), d.DocToCanvasAngle(sweep));
            g.Restore(saved);
            DrawArrow(g, d, shape.Arr1X, shape.Arr1Y, shape.Arr1R, shape.Arr1Code2);
            DrawArrow(g, d, shape.Arr2X, shape.Arr2Y, shape.Arr2R, shape.Arr2Code2);
            if (shape.Flag4 == 1)
            {
                DrawText(g, d, shape.TextX, shape.TextY, shape.Width, shape.Height, shape.Angle,
                    shape.Str, shape.Font, shape.Direct, shape.BPnt, shape.Color);
            }
        }

        void OnDrawRadiusDim(Graphics g, DrawContext d, SxfRadiusDimShape shape)
        {
            ApplyLineStyle(d.Pen, shape.Color, shape.LineWidth, shape.LineType);
            var sun1 = d.DocToCanvas(ConvertPoint(shape.SunX1, shape.SunY1));
            var sun2 = d.DocToCanvas(ConvertPoint(shape.SunX2, shape.SunY2));

            g.DrawLine(d.Pen, sun1, sun2);
            DrawArrow(g, d, shape.ArrX, shape.ArrY, shape.ArrR, shape.ArrCode2);
            if (shape.Flag == 1)
            {
                DrawText(g, d, shape.TextX, shape.TextY, shape.Width, shape.Height, shape.Angle,
                    shape.Str, shape.Font, shape.Direct, shape.BPnt, shape.Color);
            }
        }

        void OnDrawDiameterDim(Graphics g, DrawContext d, SxfDiameterDimShape shape)
        {
            ApplyLineStyle(d.Pen, shape.Color, shape.LineWidth, shape.LineType);
            var sun1 = d.DocToCanvas(ConvertPoint(shape.SunX1, shape.SunY1));
            var sun2 = d.DocToCanvas(ConvertPoint(shape.SunX2, shape.SunY2));

            g.DrawLine(d.Pen, sun1, sun2);
            DrawArrow(g, d, shape.Arr1X, shape.Arr1Y, shape.Arr1R, shape.Arr1Code2);
            DrawArrow(g, d, shape.Arr2X, shape.Arr2Y, shape.Arr2R, shape.Arr2Code2);
            if (shape.Flag == 1)
            {
                DrawText(g, d, shape.TextX, shape.TextY, shape.Width, shape.Height, shape.Angle,
                    shape.Str, shape.Font, shape.Direct, shape.BPnt, shape.Color);
            }
        }

        void OnDrawLabel(Graphics g, DrawContext d, SxfLabelShape shape)
        {
            ApplyLineStyle(d.Pen, shape.Color, shape.LineWidth, shape.LineType);
            var pts = new PointF[shape.VertexX.Count];
            for (var i = 0; i < shape.VertexX.Count; i++)
            {
                pts[i] = d.DocToCanvas(ConvertPoint(shape.VertexX[i], shape.VertexY[i]));
            }
            g.DrawLines(d.Pen, pts);
            DrawArrow(g, d, shape.VertexX[0], shape.VertexY[0], shape.ArrR, 1);
            if (shape.Flag == 1)
            {
                DrawText(g, d, shape.TextX, shape.TextY, shape.Width, shape.Height, shape.Angle,
                    shape.Str, shape.Font, shape.Direct, shape.BPnt, shape.Color);
            }
        }

        void OnDrawBalloon(Graphics g, DrawContext d, SxfBalloonShape shape)
        {
            ApplyLineStyle(d.Pen, shape.Color, shape.LineWidth, shape.LineType);
            var pts = new PointF[shape.VertexX.Count];
            for (var i = 0; i < shape.VertexX.Count; i++)
            {
                pts[i] = d.DocToCanvas(ConvertPoint(shape.VertexX[i], shape.VertexY[i]));
            }
            g.DrawLines(d.Pen, pts);
            var p0 = d.DocToCanvas(ConvertPoint(shape.CenterX, shape.CenterY));
            var r = d.DocToCanvas(ConvertLength(shape.Radius));
            g.DrawEllipse(d.Pen, p0.X-r, p0.Y-r, r * 2, r * 2);
            DrawArrow(g, d, shape.VertexX[0], shape.VertexY[0], shape.ArrR, 1);
            if (shape.Flag == 1)
            {
                DrawText(g, d, shape.TextX, shape.TextY, shape.Width, shape.Height, shape.Angle,
                    shape.Str, shape.Font, shape.Direct, shape.BPnt, shape.Color);
            }
        }

        /// <summary>
        /// ハッチング（塗り）。手抜きで外形線のみで塗は無し。
        /// </summary>
        /// <param name="g"></param>
        /// <param name="d"></param>
        /// <param name="shape"></param>
        void OnDrawFillAreaStyleColor(Graphics g, DrawContext d, SxfFillAreaStyleColorShape shape)
        {
            DrawFillAreaStyle(g, d, shape.OutId, shape.InId);
        }

        void OnDrawFillAreaStyleHatching(Graphics g, DrawContext d, SxfFillAreaStyleHatchingShape shape)
        {
            DrawFillAreaStyle(g, d, shape.OutId, shape.InId);
        }
        void OnDrawFillAreaStyleTiles(Graphics g, DrawContext d, SxfFillAreaStyleTilesShape shape)
        {
            DrawFillAreaStyle(g, d, shape.OutId, shape.InId);
        }

        void DrawFillAreaStyle(Graphics g, DrawContext d, int outId, IReadOnlyList<int> inIds)
        {
            var sxfOutCurve = mDoc.GetCompositCurve(outId);
            if (sxfOutCurve == null)
            {
                Debug.WriteLine($"Could not find FillArea.outId = '{outId}'");
                return;
            }
            DrawCurveOrg(g, d, sxfOutCurve);
            foreach (var inId in inIds)
            {
                var sxfCurve = mDoc.GetCompositCurve(inId);
                if (sxfCurve == null)
                {
                    Debug.WriteLine($"Could not find FillArea.inId = '{inId}'");
                    return;
                }
                DrawCurveOrg(g, d, sxfOutCurve);
            }
        }


        void DrawCurveOrg(Graphics g, DrawContext d, SxfCcurveOrg curve)
        {
            var color = curve.Color;
            var lineWidth = curve.LineWidth;
            var lineType = curve.LineType;
            foreach (var s in curve.Shapes)
            {
                switch (s)
                {
                    case SxfPolylineShape ss:
                        DrawPolyline(g, d, ss, color, lineWidth, lineType);
                        break;
                    case SxfSplineShape ss:
                        DrawSpline(g, d, ss, color, lineWidth, lineType);
                        break;
                    case SxfArcShape ss:
                        DrawArc(g, d, ss, color, lineWidth, lineType);
                        break;
                    case SxfEllipseArcShape ss:
                        DrawEllipseArc(g, d, ss, color, lineWidth, lineType);
                        break;
                }
            }
        }

        void OnDrawSfigloc(Graphics g, DrawContext d, SxfSfiglocShape sfigLoc)
        {
            var sfigOrg = mDoc.GetSfigOrg(sfigLoc.Name);
            if (sfigOrg == null) throw new Exception($"Could not find SfigOrg.name = '{sfigLoc.Name}'");
            //sfigOrg.Flagは複合図形種別フラグ(1:部分図(数学座標系)、2: 部分図(測地座標系)、3:作図グループ、4:作図部品)です。
            //ここでは描画するだけなのでどれも同じように処理しますがCADなどでは処理を分けることになります。
            //描画するためにGraphicsのTranslate系の機能を使います。
            //線幅が縮尺で変わってしまうためmShapeScaleを使います。手を抜いて縦横異縮は考えず横方向のみ使っています。
            var oldDrawingScale = mShapeScale;
            mShapeScale *= sfigLoc.RatioX;
            var save = g.Save();
            var p0 = d.DocToCanvas(ConvertPoint(sfigLoc.X, sfigLoc.Y));
            g.TranslateTransform(p0.X, p0.Y);
            g.ScaleTransform((float)sfigLoc.RatioX, (float)sfigLoc.RatioY);
            g.RotateTransform((float)d.DocToCanvasAngle(sfigLoc.Angle));
            foreach (var sxfShape in sfigOrg.Shapes)
            {
                OnDrawShape(g, d, sxfShape);
            }
            g.Restore(save);
            mShapeScale = oldDrawingScale;
        }

        void DrawText(
            Graphics g, DrawContext d,
            double x, double y, double width, double height, double angle,
            string text, int fontId, int direct, int basis, int colorId)
        {
            var fontName = ConvertTextFont(fontId);
            var w = width;
            var h = height;
            var sf = new StringFormat();
            if (direct == 2)
            {
                //縦書き
                (w, h) = (h, w);
                //                angle -= 90;
                sf.FormatFlags = StringFormatFlags.DirectionVertical;
            }
            using var font = new Font(fontName, (float)h);
            using var brush = new SolidBrush(ConvertColor(colorId));
            var (dx, dy) = basis switch
            {
                1 => (0, h),//BottomLeft
                2 => (-w / 2, h),//BottomCenter
                3 => (-w, h),//BottomRight
                4 => (0, h / 2),//CenterLeft
                5 => (-w / 2, h / 2),//Center
                6 => (-w, h / 2),//CenterRight
                7 => (0, 0),//TopLeft
                8 => (-w / 2, 0),//TopCenter
                9 => (-w / 2, 0),//TopRight
                _ => (0, h),
            };
            var p = d.DocToCanvas(ConvertPoint(x, y));//回転、拡縮のの基準
            var p0 = d.DocToCanvas(ConvertPoint(x + dx, y + dy));//配置位置
            var size = g.MeasureString(text, font, p0, sf);
            var save = g.Save();

            g.TranslateTransform(p.X, p.Y);
            g.ScaleTransform((float)width / size.Width, (float)height / size.Height);
            g.RotateTransform(d.DocToCanvasAngle(angle));
            g.DrawString(text, font, brush, p0.X - p.X, p0.Y-p.Y, sf);
            g.Restore(save);

        }

        void DrawArrow(Graphics g, DrawContext d, double x, double y, double radius, int code2)
        {
            if (code2 != 0)
            {
                var p = d.DocToCanvas(ConvertPoint(x, y));
                var size = d.DocToCanvas((float)(2.5 * radius / mShapeScale));
                g.DrawEllipse(d.Pen, p.X - size / 2, p.Y - size / 2, size, size);
            }
        }


        void ApplyLineStyle(Pen pen, int lineColorId, int lineWidthId, int lineTypeId)
        {
            var c = ConvertColor(lineColorId);
            pen.Color = c;
            pen.Width = ConvertLineWidth(lineWidthId);
            var t = ConvertLineType(lineTypeId);
            if (t.Length == 0)
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
            }
            else
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                pen.DashPattern = t;
            }
        }


        Color ConvertColor(int colorId)
        {
            var c = mDoc.Table.GetColorFromId(colorId);
            if (c == null) c = new SxfColor(0, 0, 0);
            if (c.Red == 255 && c.Green == 255 && c.Blue == 255) return Color.Black;
            return Color.FromArgb(c.Red, c.Green, c.Blue);
        }

        float ConvertLineWidth(int widthId)
        {
            var w = mDoc.Table.GetLineWidthFromId(widthId);
            if (w == null) return 0.0f;
            return (float)(w.Width / mShapeScale);
        }

        float[] ConvertLineType(int typeId)
        {
            var t = mDoc.Table.GetLineTypeFromId(typeId);
            if (t == null) return Array.Empty<float>();
            return Array.Empty<float>();
            //これではうまくいかない
//            return t.Pitch.Select(x => (float)(x / mShapeScale)).ToArray();
        }


        string ConvertTextFont(int fontId)
        {
            var f = mDoc.Table.GetTextFontFromId(fontId);
            return f?.Name ?? "";
        }

        //CadPoint ConvertPoint(SxfPoint p)
        //{
        //    return new CadPoint(p.X, p.Y);
        //}

        CadPoint ConvertPoint(double x, double y)
        {
            return new CadPoint(x, y);
        }
        double ConvertLength(double a)
        {
            return a;
        }

    }

}
