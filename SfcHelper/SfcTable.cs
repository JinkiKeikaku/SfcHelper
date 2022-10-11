using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SfcHelper
{
    /// <summary>
    /// テーブル
    /// </summary>
    public class SfcTable
    {
        private int mNextLayerId = 1;
        private int mNextUserLineWidthId = 11;
        private int mNextUserColorId = 17;
        private int mNextUserLineTypeId = 17;
        private int mNextTextFontId = 1;

        /// <summary>
        /// レイヤテーブル
        /// </summary>
        public Dictionary<int, SxfLayer> LayerMap { get; } = new();
        /// <summary>
        /// 線幅テーブル
        /// </summary>
        public Dictionary<int, SxfLineWidth> LineWidthMap { get; } = new();
        /// <summary>
        /// 色テーブル
        /// </summary>
        public Dictionary<int, SxfColor> ColorMap { get; } = new();
        /// <summary>
        /// 線種テーブル
        /// </summary>
        public Dictionary<int, SxfLineType> LineTypeMap { get; } = new();
        /// <summary>
        /// フォントテーブル
        /// </summary>
        public Dictionary<int, SxfTextFont> TextFontMap { get; } = new();
        /// <summary>
        /// 既定義線幅テーブル
        /// </summary>
        public double[] PreDefinedLineWidth = new double[]
        {
            0.13, 0.18, 0.25, 0.35, 0.5, 0.7, 1.0, 1.4, 2.0
        };

        /// <summary>
        /// SXFの色コードから実際の色を返します。
        /// 色コードから色が取得できない場合、nullが返ります。
        /// </summary>
        /// <param name="colorId">色コード</param>
        /// <returns>RGBの色</returns>
        public SxfColor? GetColorFromId(int colorId)
        {
            var c = new SxfColor(0,0,0);
            if (ColorMap.TryGetValue(colorId, out var sxfColor))
            {
                return sxfColor;
            }
            return null;
        }

        /// <summary>
        /// 既定義線色を設定します。保存処理の時に使います。
        /// </summary>
        public void SetAllPreDefinedColors()
        {
            for (var i = 0; i < SxfPreDefinedColor.PreDefColors.Length; i++)
            {
                ColorMap[i + 1] = SxfPreDefinedColor.PreDefColors[i];
            }
        }

        /// <summary>
        /// 既定義線幅を設定します。保存処理の時に使います。
        /// </summary>
        public void SetAllPreDefinedLineWidths()
        {
            for (var i = 0; i < SxfLineWidth.PreDefinedLineWidth.Length; i++)
            {
                LineWidthMap[i + 1] = new SxfLineWidth(SxfLineWidth.PreDefinedLineWidth[i]);
            }
        }

        /// <summary>
        /// 既定義線種を設定します。保存処理の時に使います。
        /// </summary>
        public void SetAllPreDefinedLineTypes()
        {
            for (var i = 0; i < SxfPreDefinedLineType.PreDefLineTypes.Length; i++)
            {
                LineTypeMap[i + 1] = SxfPreDefinedLineType.PreDefLineTypes[i];
            }
        }


        /// <summary>
        /// 線幅コードから線幅を返します。
        /// 線幅コードから線幅が取得できない場合、nullが返ります。
        /// </summary>
        /// <param name="widthId">線幅コード</param>
        /// <returns>線幅</returns>
        public SxfLineWidth? GetLineWidthFromId(int widthId)
        {
            if (LineWidthMap.TryGetValue(widthId, out var sxfLineWidth))
            {
                return sxfLineWidth;
            }
            return null;
        }

        /// <summary>
        /// 線種コードから線種を返します。
        /// みつからない場合、nullを返します。
        /// </summary>
        /// <param name="typeId">線種コード</param>
        /// <returns>線種</returns>
        public SxfLineType? GetLineTypeFromId(int typeId)
        {
            if (LineTypeMap.TryGetValue(typeId, out var sxfLineType))
            {
                return sxfLineType;
            }
            return null;
        }

        /// <summary>
        /// テーブルにフォントを追加しコードを返します。
        /// フォントが定義できる数を超えた場合は例外が発生します。
        /// </summary>
        /// <param name="name">フォント名</param>
        /// <returns>フォントコード</returns>
        /// <exception cref="Exception">フォントが定義できる数を超えた場合に発生します。</exception>
        public int AddTextFont(string name)
        {
            var i = mNextTextFontId;
            if (mNextTextFontId > SxfConst.MaxTextFont) throw new Exception($"Number of fonts >{SxfConst.MaxTextFont} ");
            TextFontMap[mNextTextFontId++] = new SxfTextFont(name);
            return i;
        }

        /// <summary>
        /// フォントコードからフォントを取得します。
        /// みつからない場合、nullを返します。
        /// </summary>
        /// <param name="id">フォントコード</param>
        /// <returns>フォント</returns>
        public SxfTextFont? GetTextFontFromId(int id)
        {
            if (TextFontMap.TryGetValue(id, out var sxfTextFont))
            {
                return sxfTextFont;
            }
            return null;
        }

        /// <summary>
        /// テーブルにレイヤを追加しレイヤコードを返します。
        /// レイヤが最大数に達して追加できない場合は例が発生します。
        /// </summary>
        /// <param name="layer">レイヤ</param>
        /// <returns>レイヤコード</returns>
        /// <exception cref="Exception">レイヤが定義できる数を超えた場合に発生します。</exception>
        public int AddLayer(SxfLayer layer)
        {
            var i = mNextLayerId;
            if (mNextLayerId > SxfConst.MaxLayer) throw new Exception($"Number of layers >{SxfConst.MaxLayer} ");
            LayerMap[mNextLayerId++] = layer;
            return i;
        }

        /// <summary>
        /// レイヤコードからレイヤを返します。
        /// みつからなければnullを返します。
        /// </summary>
        /// <param name="id">レイヤコード</param>
        /// <returns>レイヤ</returns>
        public SxfLayer? GetLayerFromId(int id)
        {
            if (LayerMap.TryGetValue(id, out var SxfLayer))
            {
                return SxfLayer;
            }
            return null;
        }


        internal void ParseLayerFeature(List<object> ps)
        {
            if (ps.Count < 2) throw new Exception($"Layer parameter size error.({ps.Count}< 2)");
            var name = SfcReader.ParseString(ps[0], "Layer.name");
            var flag = SfcReader.ParseInt(ps[1], "Layer.flag");
            var lay = new SxfLayer(name, flag);
            if (mNextLayerId > SxfConst.MaxLayer)
            {
                throw new Exception($"Layer exceeds the maximum number of definitions.");
            }
            LayerMap.Add(mNextLayerId++, lay);
        }

        internal void ParsePreDefinedLineTypeFeature(List<object> ps)
        {
            if (ps.Count < 1) throw new Exception($"PreDefinedLineType parameter size error.({ps.Count}< 1)");
            var name = SfcReader.ParseString(ps[0], "PreDefinedLineType.name");
            var i = Array.FindIndex(SxfPreDefinedLineType.PreDefLineTypes, x => x.Name == name);
            if (i < 0) throw new Exception($"PreDefinedLineType parameter error.({name})");
            LineTypeMap.Add(i + 1, SxfPreDefinedLineType.PreDefLineTypes[i]);
        }

        internal void ParseUserDefinedLineTypeFeature(List<object> ps)
        {
            if (ps.Count < 3) throw new Exception($"UserDefinedLineType parameter size error.({ps.Count}< 3)");
            var name = SfcReader.ParseString(ps[0], "UserDefinedLineType.name");
            var n = SfcReader.ParseInt(ps[1], "UserDefinedLineType.segment");
            var a2 = SfcReader.ParseDoubleList(ps[2], "UserDefinedLineType.pitch");
            if (a2.Count != n || (n & 1) != 0) throw new Exception($"UserDefinedLineType segment size is invalid. ({a2.Count} != {n})");
            if (mNextUserLineTypeId > SxfConst.MaxLineType)
            {
                throw new Exception($"Line type exceeds the maximum number of definitions.");
            }

            LineTypeMap.Add(mNextUserLineTypeId++, new SxfLineType(name, a2.ToArray()));
        }

        internal void ParsePreDefinedColourFeature(List<object> ps)
        {
            if (ps.Count < 1) throw new Exception($"PreDefinedColour parameter size error.({ps.Count}< 1)");
            var name = SfcReader.ParseString(ps[0], "PreDefinedColour.name");
            var i = Array.FindIndex(SxfPreDefinedColor.PreDefColors, x => x.Name == name);
            if (i < 0) throw new Exception($"PreDefinedColour parameter error.({name})");
            ColorMap.Add(i + 1, SxfPreDefinedColor.PreDefColors[i]);
        }

        internal void ParseUserDefinedColourFeature(List<object> ps)
        {
            if (ps.Count < 3) throw new Exception($"UserDefinedColour parameter size error.({ps.Count}< 3)");
            var r = SfcReader.ParseInt(ps[0], "UserDefinedColour.r");
            var g = SfcReader.ParseInt(ps[1], "UserDefinedColour.g");
            var b = SfcReader.ParseInt(ps[2], "UserDefinedColour.b");
            if (mNextUserColorId > SxfConst.MaxColor)
            {
                throw new Exception($"UserDefinedColour exceeds the maximum number of definitions.");
            }
            ColorMap.Add(mNextUserColorId++, new SxfColor(r, g, b));
        }

        internal void ParseWidthFeature(List<object> ps)
        {
            if (ps.Count < 1) throw new Exception($"Width parameter size error.({ps.Count}< 1)");
            var w = SfcReader.ParseDouble(ps[0], "Width");
            if (w <= 0.0) throw new Exception($"Width parameter <= 0.0.({w})");
            var i = Array.FindIndex(PreDefinedLineWidth, x => Helper.FloatEQ(x, w));
            if (i >= 0)
            {
                LineWidthMap.Add(i + 1, new SxfLineWidth(PreDefinedLineWidth[i]));
            }
            else
            {
                if (mNextUserLineWidthId > SxfConst.MaxLineWidth)
                {
                    throw new Exception($"Line width exceeds the maximum number of definitions.");
                }
                LineWidthMap.Add(mNextUserLineWidthId++, new SxfLineWidth(w));
            }
        }

        internal void ParseTextFontFeature(List<object> ps)
        {
            if (ps.Count < 1) throw new Exception($"TextFont parameter size error.({ps.Count}< 3)");
            var name = SfcReader.ParseString(ps[0], "TextFont.name");
            if (mNextTextFontId > SxfConst.MaxTextFont)
            {
                throw new Exception($"TextFont exceeds the maximum number of definitions.");
            }

            TextFontMap.Add(mNextTextFontId++, new SxfTextFont(name));
        }
    }
}
