
using System.Reflection.PortableExecutable;
using System.Text;

namespace SfcHelper
{
    public class SfcHeader
    {
        //ディレクトリ名を除いたファイル名等
        public string FileName = "";

        //変換開始時間
        public string TimeStamp = "2000-01-01T00:00:00";

        /// <summary>ファイル作成者</summary>
        public string Author = "";

        /// <summary>ファイル作成者所属</summary>
        public string Organization = "";

        /// <summary>
        /// 変換システムのバージョンとSXFファイルのバージョン(バージョンは$$でつなげる？)
        /// 共通ライブラリでは'SCADEC_API_Ver3.30'ですが独自実装なので'SfcHelper'としました。
        /// </summary>
        public string PreprocessorVersion = "SfcHelper";

        /// <summary>トランスレータ名</summary>
        public string TranslatorName = "My application";

        /// <summary> SXFレベル </summary>
        public int Level = 2;

        /// <summary>
        /// SFC保存時のヘッダー初期化に使用
        /// </summary>
        /// <param name="fileName">ディレクトリ名を除いたファイル名等</param>
        /// <param name="author">ファイル作成者</param>
        /// <param name="organization">ファイル作成者所属</param>
        /// <param name="translatorName">トランスレータ名</param>
        /// <param name="dt">日時</param>
        public void SetParameters(string fileName, string author, string organization, string translatorName, DateTime dt)
        {
            FileName = fileName;
            Author = author;
            Organization = organization;
            TranslatorName = translatorName;
            SetTimeStamp(dt);
        }

        /// <summary>
        /// タイムスタンプ文字列を設定します。
        /// </summary>
        /// <param name="dt">設定する日時</param>
        public void SetTimeStamp(DateTime dt)
        {
            TimeStamp = dt.ToString("yyyy-MM-ddTHH:mm:ss");
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("HEADER;");
            sb.AppendLine("FILE_DESCRIPTION(('SCADEC level2 feature_mode'),");
            sb.AppendLine("'2;1');");
            sb.AppendLine($"FILE_NAME('{FileName}',");
            sb.AppendLine($"'{TimeStamp}',");
            sb.AppendLine($"('{Author}'),");
            sb.AppendLine($"('{Organization}'),");
            sb.AppendLine($"'{PreprocessorVersion}',");
            sb.AppendLine($"'{TranslatorName}',");
            sb.AppendLine($"'');");
            sb.AppendLine($"FILE_SCHEMA(('ASSOCIATIVE_DRAUGHTING'));");
            sb.AppendLine($"ENDSEC;");
            return sb.ToString();
        }
    }
}
