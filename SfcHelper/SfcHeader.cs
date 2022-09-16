using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SfcHelper
{
    public class SfcHeader
    {
        //ディレクトリ名を除いたファイル名
        public string FileName;
        //変換開始時間
        public string TimeStamp;
        //ファイル作成者
        public string Author;
        //ファイル作成者所属
        public string Organization;
        //変換システムのバージョンとSXFファイルのバージョン(バージョンは$$でつなげる？)
        public string PreprocessorVersion = "SfcHelper";
        //トランスレータ名
        public string TranslatorName;

        public int Level = 2;

        public void SetTimeStamp(DateTime dt)
        {
            TimeStamp = dt.ToString("yyyy-MM-dd") + "T" + dt.ToString("HH:mm:ss");
        }
    }
}
