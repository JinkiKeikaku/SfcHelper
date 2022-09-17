using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SfcHelper
{
    static class Helper
    {
        public static bool FloatEQ(double x, double y, double delta = 0.00001)
        {
            return Math.Abs(x - y) <= delta;
        }

        public static string MakeFeatureString(string featureName, params object[] list)
        {
            var sa = new List<string>();
            foreach (var item in list)
            {
                switch (item)
                {
                    case IReadOnlyList<int> s:
                        {
                            var a = string.Join(",", s);
                            sa.Add($"'({a})'");
                        }
                        break;
                    case IReadOnlyList<double> s:
                        {
                            var a = string.Join(",", s);
                            sa.Add($"'({a})'");
                        }
                        break;
                    case string s:
                        {
                            sa.Add(@$"\'{s}\'");
                        }
                        break;
                    default:
                        sa.Add($"'{item}'");
                        break;
                }
            }
            return $"{featureName}({String.Join(',', sa)})";
        }
    }
}
