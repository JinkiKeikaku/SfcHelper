using SfcHelper;
using System.Diagnostics;

namespace SfcTest
{
    public partial class Form1 : Form
    {
        DrawContext? mDrawContext = null;
        SxfDocument mDoc = new SxfDocument();

        public Form1()
        {
            InitializeComponent();
            this.panel1.OnMouseWheelZoom += panel1_MouseWheel;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var d = new OpenFileDialog();
            d.Filter = "Sxf files|*.sfc;|All files|*.*";
            if (d.ShowDialog() != DialogResult.OK) return;
            OpenFile(d.FileName);
        }

        /// <summary>
        /// 最初に表示する図形。図形登録方法をここに記す。
        /// </summary>
        private void InitDrawing()
        {
            //ドキュメントを作って用紙を設定。
            mDoc = new SxfDocument();
            mDoc.SetSheetParameter("A3 Portrait", 3, 1, 420, 297);
            //既定義色、既定義線幅、既定義線種を全て登録します。本来は使わない色は登録しなくてもよいのですが、
            //使うものをひとつづつ定義するよりもこのほうが楽と思います。
            //既定義色をテーブルに設定します。
            mDoc.Table.SetAllPreDefinedColors();
            //既定義線幅をテーブルに設定します。
            mDoc.Table.SetAllPreDefinedLineWidths();
            //既定義線種をテーブルに設定します。
            mDoc.Table.SetAllPreDefinedLineTypes();
            //フォントを定義
            var fontId = mDoc.Table.AddTextFont("ＭＳ ゴシック");
            //レイヤを定義。返ってきたレイヤコードを図形登録で使います。
            var　layerId = mDoc.Table.AddLayer(new SxfLayer("Layer1", 1));

            //図形の色、線幅、線種は既定義線色を使ってます。コードは仕様書を見てください。
            //layer1, color:black, type:continuous, width:0.13,(0,0)-(420,297)
            mDoc.Shapes.Add(new SxfLineShape(layerId, 1, 1, 1, 0, 0, 420, 297));
            //layer1, color:red, type:continuous, width:1.0,(0,297)-(420,0)
            mDoc.Shapes.Add(new SxfLineShape(layerId, 2, 1, 7, 0, 297, 420, 0));
            mDoc.Shapes.Add(new SxfTextShape(layerId, 3, fontId, "これはテストです", 210, 148.5, 10, 80, 0, 0, 0,7,1));


            //表示に使うDrawContextを設定。
            var (w, h) = GetPaperSize(mDoc);
            mDrawContext = new DrawContext(w, h);
            //画面いっぱいに用紙を表示
            FitPaper();
        }

        private void OpenFile(string fname)
        {
            mDoc = new SxfDocument();
            var r = new SfcHelper.SfcReader(mDoc);
            r.Read(fname);
            var (w,h) = GetPaperSize(mDoc);
            mDrawContext = new DrawContext(w, h);
            //CalcSize();
            FitPaper();
            panel1.Invalidate();
            textBox1.Text= mDoc.Header.ToString();
        }

        (double width, double height) GetPaperSize(SfcHelper.SxfDocument doc)
        {
            var (w, h) = doc.Sheet.PaperType switch
            {
                0 => (1189, 841),
                1 => (841, 594),
                2 => (594, 420),
                3 => (420, 297),
                4 => (297, 210),
                9 => (doc.Sheet.Width, doc.Sheet.Height),
                _ => (420, 297)
            };
            return doc.Sheet.Orient == 1 ? (w, h) : (h, w);
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.LightGray);
            if (mDrawContext == null) return;
            var saved = g.Save();
            g.TranslateTransform(
                (float)(panel1.AutoScrollPosition.X),
                (float)(panel1.AutoScrollPosition.Y)
            );
            g.ScaleTransform(mDrawContext.TranslateScale, mDrawContext.TranslateScale);
            var drawer = new SfcDrawer(mDoc);
            drawer.OnDraw(g, mDrawContext);
            g.Restore(saved);

        }

        private void panel1_MouseWheel(object? sender, MouseEventArgs e)
        {

            if (mDrawContext == null) return;
            var x0 = (-panel1.AutoScrollPosition.X + e.X) / mDrawContext.TranslateScale;// + e.X * mDrawContext.TranslateScale;
            var y0 = mDrawContext.PaperHeight - (-panel1.AutoScrollPosition.Y + e.Y) / mDrawContext.TranslateScale;// + e.Y * mDrawContext.TranslateScale;

            /// マウスホイールでは拡大縮小のみ行う。
            if (e.Delta < 0)
            {
                mDrawContext.TranslateScale *= 0.8f;
            }
            else
            {
                mDrawContext.TranslateScale *= 1.25f;
            }
            CalcSize();
            var x1 = (-panel1.AutoScrollPosition.X + e.X) / mDrawContext.TranslateScale;// + e.X * mDrawContext.TranslateScale;
            var y1 = mDrawContext.PaperHeight - (-panel1.AutoScrollPosition.Y + e.Y) / mDrawContext.TranslateScale;// + e.Y * mDrawContext.TranslateScale;
            var dx = -(int)(x1 - x0) * mDrawContext.TranslateScale;
            var dy = (int)(y1 - y0) * mDrawContext.TranslateScale;
            var x = -panel1.AutoScrollPosition.X + (int)(dx);
            var y = -panel1.AutoScrollPosition.Y + (int)(dy);
            Debug.WriteLine($"(x0:{x0}, y0:{y0}) (x1:{x1}, y1:{y1}) (x:{x}, y:{y})  (dx:{dx}  dy:{dy})");
            panel1.AutoScrollPosition = new Point(x, y);
            panel1.Invalidate();
        }
        private void panel1_ClientSizeChanged(object sender, EventArgs e)
        {
            CalcSize();
        }

        private void FitPaper()
        {
            if (mDrawContext == null) return;
            var s = Math.Min(
                panel1.ClientSize.Width / mDrawContext.PaperWidth,
                panel1.ClientSize.Height / mDrawContext.PaperHeight);

            mDrawContext.TranslateScale = (float)s;
            CalcSize();
            var ps = new Size((int)(mDrawContext.PaperWidth * s),(int)(mDrawContext.PaperHeight * s));
            panel1.AutoScrollPosition = new Point(
                (int)(ps.Width / 2),
                (int)(ps.Height / 2));
            panel1.Invalidate();

        }

        /// <summary>
        /// スクロールの設定
        /// </summary>
        private void CalcSize()
        {
            if (mDrawContext == null) return;

            var ps = new Size(
                (int)(mDrawContext.PaperWidth * 2 * mDrawContext.TranslateScale), 
                (int)(mDrawContext.PaperHeight * 2 * mDrawContext.TranslateScale));
            panel1.AutoScrollMinSize = new Size((int)ps.Width, (int)ps.Height);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitDrawing();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var d = new SaveFileDialog();
            d.Filter = "Sxf files|*.sfc;|All files|*.*";
            if (d.ShowDialog() != DialogResult.OK) return;
            WriteFile(d.FileName);
        }
        private void WriteFile(string fname)
        {
            var r = new SfcHelper.SfcWriter(mDoc);
            r.Write(fname);
        }

    }
}