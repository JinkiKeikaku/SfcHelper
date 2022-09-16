using SfcHelper;

namespace SfcTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var d = new OpenFileDialog();
            d.Filter = "Sxf files|*.sfc;|All files|*.*";
            if (d.ShowDialog() != DialogResult.OK) return;
            OpenFile(d.FileName);
        }


        void OpenFile(string fname)
        {
            var r = new SfcHelper.SfcReader();
            r.Read(fname);
        }
    }
}