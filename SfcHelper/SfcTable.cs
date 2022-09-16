using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SfcHelper
{
    public class SfcTable
    {
        Dictionary<int, SxfLayer> LayerMap { get; } = new();
        int mNextLayerId = 1;

        Dictionary<int, SxfLineWidth> LineWidthMap { get; } = new();
        int mNextUserLineWidthId = 11;
        double[] mPreDefinedLineWidth = new double[]
        {
            0.13, 0.18, 0.25, 0.35, 0.5, 0.7, 1.0, 1.4, 2.0
        };

        Dictionary<int, SxfColor> mColorMap { get; } = new();
        int mNextUserColorId = 17;
        SxfPreDefinedColor[] mPreDefColors = new SxfPreDefinedColor[]
        {
            new("black", 0,0,0),new("red", 255,0,0),new("green", 0,255,0),new("blue", 0,0,255),
            new("yellow", 255,255,0),new("magenta", 255,0,255),new("cyan", 0,255,255),new("white", 255,255,255),
            new("deeppink", 192,0,128),new("brown", 192,128,64),new("orange", 255,128,0),new("lightgreen", 128,192,128),
            new("lightblue", 0,128,255),new("lavender", 128,64,255),new("lightgray", 192,192,192),new("darkgray", 128,128,128),
        };

        Dictionary<int, SxfLineType> mLineTypeMap { get; } = new();
        int mNextUserLineTypeId = 17;
        SxfPreDefinedLineType[] mPreDefLineTypes = new SxfPreDefinedLineType[]
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
        Dictionary<int, SxfTextFont> mTextFontMap { get; } = new();
        int mNextTextFontId = 1;


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
            var i = Array.FindIndex(mPreDefLineTypes, x => x.Name == name);
            if (i < 0) throw new Exception($"PreDefinedLineType parameter error.({name})");
            mLineTypeMap.Add(i + 1, mPreDefLineTypes[i]);
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
            mLineTypeMap.Add(mNextUserLineTypeId++, new SxfLineType(name, a2.ToArray()));
        }

        internal void ParsePreDefinedColourFeature(List<object> ps)
        {
            if (ps.Count < 1) throw new Exception($"PreDefinedColour parameter size error.({ps.Count}< 1)");
            var name = SfcReader.ParseString(ps[0], "PreDefinedColour.name");
            var i = Array.FindIndex(mPreDefColors, x => x.Name == name);
            if (i < 0) throw new Exception($"PreDefinedColour parameter error.({name})");
            mColorMap.Add(i + 1, mPreDefColors[i]);
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
            mColorMap.Add(mNextUserColorId++, new SxfColor(r, g, b));
        }

        internal void ParseWidthFeature(List<object> ps)
        {
            if (ps.Count < 1) throw new Exception($"Width parameter size error.({ps.Count}< 1)");
            var w = SfcReader.ParseDouble(ps[0], "Width");
            if (w <= 0.0) throw new Exception($"Width parameter <= 0.0.({w})");
            var i = Array.FindIndex(mPreDefinedLineWidth, x => Helper.FloatEQ(x, w));
            if (i >= 0)
            {
                LineWidthMap.Add(i + 1, new SxfLineWidth(mPreDefinedLineWidth[i]));
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

            mTextFontMap.Add(mNextTextFontId++, new SxfTextFont(name));
        }



    }
}
