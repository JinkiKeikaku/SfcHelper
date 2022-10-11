namespace SfcHelper
{
    /// <summary>
    /// ドキュメント
    /// </summary>
    public class SxfDocument
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SxfDocument()
        {
        }
        public List<SxfSfigOrg> SfigOrgList { get; } = new();
        public Dictionary<int, SxfCcurveOrg> CompositCurveMap { get; } = new();

        /// <summary>
        /// ドキュメントの初期化。
        /// </summary>
        public void Clear()
        {
            SetSheetParameter();
            Header = new();
            Table = new();
            Shapes.Clear();
            SfigOrgList.Clear();
            mNextCompositCurveId = 1;
            CompositCurveMap.Clear();
        }

        /// <summary>ヘッダ</summary>
        public SfcHeader Header { get; private set; } = new();

        /// <summary>シート（用紙）</summary>
        public SxfSheet Sheet { get; private set; } = new("A3 Portrait", 3, 1, 420, 297);

        /// <summary>テーブル</summary>
        public SfcTable Table { get; private set; } = new();

        /// <summary>図形のリスト</summary>
        public List<SxfShape> Shapes { get; } = new();

        /// <summary>
        /// シート（用紙）設定
        /// </summary>
        /// <param name="name">図面名</param>
        /// <param name="paperType">用紙サイズ種別(0:A0, 1:A1, 2:A2, 3:A3, 4:A4, 9:FREE)</param>
        /// <param name="orient">縦／横区分(0:縦,1:横)</param>
        /// <param name="width">自由用紙横長（単位ｍｍ）</param>
        /// <param name="height">自由用紙縦長（単位ｍｍ）</param>
        public void SetSheetParameter(
            string name = "A3 Portrait", int paperType = 3, int orient = 1,
            int width = 420, int height = 297
        )
        {
            Sheet = new(name, paperType, orient, width, height);
        }

        /// <summary>
        /// idの複合曲線を返します。存在しなければnullを返します。
        /// </summary>
        /// <param name="id">複合曲線ID</param>
        /// <returns>複合曲線オブジェクト。存在しなければnull。</returns>
        public SxfCcurveOrg? GetCompositCurve(int id)
        {
            if (!CompositCurveMap.ContainsKey(id)) return null;
            return CompositCurveMap[id];
        }

        /// <summary>
        /// 複合曲線を登録し、そのIDを返します。
        /// 保存用のデータ作成時に使います。
        /// </summary>
        /// <param name="curve">複合曲線</param>
        /// <returns>ID</returns>
        public int AddCompositCurve(SxfCcurveOrg curve)
        {
            CompositCurveMap.Add(mNextCompositCurveId, curve);
            return mNextCompositCurveId++;
        }

        /// <summary>
        /// 名前から複合図形オブジェクトを返します。
        /// </summary>
        /// <param name="name">名前</param>
        /// <returns>複合図形オブジェクト。存在しなければnull。</returns>
        public SxfSfigOrg? GetSfigOrg(string name)
        {
            return SfigOrgList.FirstOrDefault(s => s.Name == name);
        }

        /// <summary>
        /// 複合図形を登録します。
        /// </summary>
        /// <param name="sfig">複合図形</param>
        public void AddSfigOrg(SxfSfigOrg sfig)
        {
            SfigOrgList.Add(sfig);
        }

        private int mNextCompositCurveId = 1;

    }
}
