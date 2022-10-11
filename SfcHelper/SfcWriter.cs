using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SfcHelper
{
    /// <summary>
    /// SFCファイル書き込みクラス
    /// </summary>
    public class SfcWriter
    {
        SxfDocument mDoc = null!;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="doc">ドキュメント</param>
        public SfcWriter(SxfDocument doc)
        {
            mDoc = doc;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        /// <summary>
        /// ドキュメント書き込み
        /// </summary>
        /// <param name="path">ファイルのパス</param>
        public void Write(string path)
        {
            using var w = new StreamWriter(path, false, Encoding.GetEncoding("shift_jis"));
            w.WriteLine("ISO-10303-21;");
            w.WriteLine(mDoc.Header.ToString());
            w.WriteLine("DATA;");
            WriteTable(w);
            WriteCompositeCurve(w);
            WriteSfig(w);
            WriteSheet(w);
            w.WriteLine("ENDSEC;");
            w.WriteLine("END-ISO-10303-21;");
        }

        private int mNumber = 10;

        private void WriteFeature(TextWriter w, object obj)
        {
            w.WriteLine();
            w.WriteLine("/*SXF");
            w.Write($"#{mNumber} = ");
            w.WriteLine(obj.ToString());
            w.WriteLine("SXF*/");
            mNumber += 10;
        }

        private void WriteTable(TextWriter w)
        {
            foreach(var a in mDoc.Table.ColorMap.OrderBy(c=>c.Key))
            {
                WriteFeature(w, a.Value);
            }
            foreach (var a in mDoc.Table.LineTypeMap.OrderBy(c => c.Key))
            {
                WriteFeature(w, a.Value);
            }
            foreach (var a in mDoc.Table.LineWidthMap.OrderBy(c => c.Key))
            {
                WriteFeature(w, a.Value);
            }
            foreach (var a in mDoc.Table.TextFontMap.OrderBy(c => c.Key))
            {
                WriteFeature(w, a.Value);
            }
            foreach (var a in mDoc.Table.LayerMap.OrderBy(c => c.Key))
            {
                WriteFeature(w, a.Value);
            }
        }

        private void WriteCompositeCurve(TextWriter w)
        {
            foreach(var a in mDoc.CompositCurveMap.OrderBy(c => c.Key))
            {
                foreach(var s in a.Value.Shapes)
                {
                    WriteFeature(w, s);
                }
                WriteFeature(w, a.Value);
            }
        }

        private void WriteSfig(TextWriter w)
        {
            foreach (var a in mDoc.SfigOrgList)
            {
                foreach (var s in a.Shapes)
                {
                    WriteFeature(w, s);
                }
                WriteFeature(w, a);
            }
        }

        private void WriteSheet(TextWriter w)
        {
            foreach(var s in mDoc.Shapes)
            {
                WriteFeature(w, s);
            }
            WriteFeature(w, mDoc.Sheet.ToString());
        }
    }
}
