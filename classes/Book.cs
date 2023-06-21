using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.International.Converters.TraditionalChineseToSimplifiedConverter;
using UtfUnknown;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace TxtReader
{
    using ssDict = Dictionary<string, string>;
    using sbDict = Dictionary<string, bool>;
    using siDict = Dictionary<string, int>;

    public class Book
    {
        #region 常量
        const string PREFIX = "标题前缀", NUMBER = "标题中缀",
            SUFFIX = "标题后缀", REQUIRE = "必备字符", ILLEGAL = "非法字符",
            NOSTART = "非法开头", NOEND = "非法结尾",
            MAXLEN = "最长标题", MINLEN = "最短标题",
            FORMAL = "可选标题", CATANAME = "目录名称",
            CH_LF = "换行符号",
            STRICT = "严格相等", REGULAR = "规范标题",
            TAILMODE = "尾部模式", PAGEMODE = "分页模式",
            TAILCATA = "文末目录";

        #endregion
        public string oriText;
        public string path;
        public List<string> lines;
        public List<string> heads;              // 手动设置的标题
        public List<Section> secs;
        public List<Section> pages;             // 页码分割
        public int wCount;
        public double kwCount;
        public List<int> preSecCount;           // 某个章节之前的所有字数

        #region 目录设置相关
        public int locCatalog = -1;            // 目录开始行，-1表示不存在
        public int locMainBody = 0;            // 正文开始行

        //规范设置目录

        public ssDict cataSet = new ssDict
        {
            {PREFIX, "第 chapter \"\""},
            {NUMBER, "一二三四五六七八九十1234567890"},
            {SUFFIX, "章节部集卷回节册篇目"},
            {REQUIRE, "" },
            {ILLEGAL, "。"},
            {NOSTART, "—" },
            {NOEND, ":：;；。" },
            {MAXLEN, "30" },
            {MINLEN, "3" },
            {CH_LF, Environment.NewLine},
            {CATANAME,  "目录"},
            {FORMAL, "目录 序 前言 封面"}
        };

        public siDict headLength = new siDict()
        {
            {MAXLEN, 30 }, {MINLEN, 3}
        };

        public siDict lineSet = new siDict
        {
            {"每页段数", 15 },
            {"每行字数", 20 },
            {"每页行数", 15 }
        };

        public sbDict boolSet = new sbDict
        {
            {STRICT, true },
            {REGULAR, true },
            {TAILMODE, false },
            {PAGEMODE, false },  //分页模式还是段落模式
            {TAILCATA, false }
        };

        #endregion

        #region 初始化
        public Book(string path, Encoding enc)
        {
            this.path = path;
            string txt = File.ReadAllText(path, enc);
            init(txt);
        }
        public Book(string txt)
        {
            init(txt);
        }

        public void init(string txt)
        {
            heads = new List<string>();
            oriText = txt;
            update();
        }

        public void reSetLF(string chLF)
        {
            cataSet[CH_LF] = chLF;
            update();
        }

        public void update(bool useOriText=true)
        {
            if(useOriText)
                lines = oriText.Split(cataSet[CH_LF], StringSplitOptions.None).ToList();
            secs = setCatalog(lines.ToArray());
            splitPage();
            if (boolSet[REGULAR])
                mergeCatalog();
            setWordCount();
        }

        public void setWordCount()
        {
            List<Section> s = boolSet[PAGEMODE] ? pages : secs;
            wCount = s.Select(s => s.wCount).Sum();
            kwCount = wCount / 1e3;
            preSecCount = new List<int> { 0 };
            for (int i = 0; i < s.Count; i++)
                preSecCount.Add(preSecCount[i] + s[i].wCount);
        }

        //第i章之前的字数
        public int getWordCount(int n)
        {
            return Enumerable.Range(0, n).Select(i => secs[i].wCount).Sum();
        }

        #endregion

        #region 设置分页
        public void setSplitParas(int n, int width, int height)
        {
            boolSet[PAGEMODE] = true;
            lineSet["每页段数"] = n;
            lineSet["每行字数"] = width;
            lineSet["每页行数"] = height;
        }

        public void splitPage()
        {
            if (lineSet["每页段数"] == 0)
                splitPage(lineSet["每行字数"], lineSet["每页行数"]);
            else
                splitPage(lineSet["每页段数"]);
        }

        // 按照文本框的宽度和高度进行分页
        public void splitPage(int width, int height)
        {
            // 每段对应的行数
            pages = new List<Section>();
            string content = "";
            int nPage = 1, N = 0, st = 0;
            int n, stChar, edChar;
            for(int i = 0; i< lines.Count; i++)
            {
                var L= lines[i];
                n = L.Length / width;           
                if (n * width < L.Length)
                    n += 1;
                
                if(N+n < height)
                {
                    N += n;
                    content += $"{cataSet[CH_LF]}{L}";
                    continue;
                }
                else if (N+n == height)
                {
                    pages.Add(new Section(nPage++, st, $"{content}{cataSet[CH_LF]}{L}"));
                    N = 0;
                    content = "";
                }
                else
                {   // 此时剩余行小于新增行
                    edChar = (height - N) * width;
                    content += $"{cataSet[CH_LF]}{L[..edChar]}";
                    pages.Add(new Section(nPage++, st, content));
                    n -= height - N;
                    while(n > height)
                    {
                        n -= height;
                        stChar = edChar;
                        edChar = stChar + height * width;
                        pages.Add(new Section(nPage++, st, L[stChar..edChar]));
                    }
                    content = L[edChar..];
                    N = n;
                }
                st = i + 1;
            }

        }

        // 分页
        public void splitPage(int n)
        {
            int nPages = lines.Count / n;   //页数
            pages = Enumerable.Range(0, nPages)
                .Select(i => Section.fromPages(i, n, lines, cataSet[CH_LF]))
                .ToList();
            if (lines.Count %n != 0)
            {
                int st = nPages * n;
                pages.Add(new Section(nPages, st, 
                    joinLines(lines.GetRange(st, lines.Count - st))));
            }
        }


        private FormattedText MeasureTextWidth(TextBox tb, string text)
        {
            var tf = new Typeface(tb.FontFamily,
                tb.FontStyle, tb.FontWeight, tb.FontStretch);
            FormattedText formattedText = new
                   FormattedText(text, CultureInfo.CurrentCulture,
                   tb.FlowDirection,
                   tf, tb.FontSize, tb.Foreground);

            //, DpiUtil.GetPixelsPerDip);

            return formattedText;
        }
        #endregion

        #region 设置目录
        public string setLF(string chLF)
        {
            return chLF.Replace("\"", "")
                       .Replace("\\r", "\r")
                       .Replace("\\n", "\n")
                       .Replace("\\t", "\t");
        }

        public void addHeads(string heads)
        {
            var hs = heads.Split(cataSet[CH_LF])
                          .Select(L => L.Trim())
                          .Where(x => x != "");

            this.heads.AddRange(hs);
            update(false);
        }

        public string setRegular(ssDict dctStr,  sbDict dctBool)
        {
            string info = setRegular(dctStr);
            info += setRegular(dctBool);
            return info;
        }

        public string setRegular(sbDict dct)
        {
            string info = "";
            foreach (var key in boolSet.Keys)
            {
                if (!dct.ContainsKey(key))
                    continue;
                boolSet[key] = dct[key];
                info += dct[key] ? $"已启用{key}\r\n" 
                    : $"未启用{key}\r\n";
            }
            return info;
        }

        public string setRegular(ssDict dct)
        {
            string info = "";
            foreach (var key in cataSet.Keys)
            {
                if (!dct.ContainsKey(key))
                    continue;
                if (headLength.ContainsKey(key))
                {
                    int tmp;
                    if(!int.TryParse(dct[key], out tmp))
                        info += $"{key}参数不合法，" +
                            $"已取默认值{headLength[key]}\r\n";
                    else
                        headLength[key] = tmp;
                }
                else
                {
                    cataSet[key] = dct[key];
                    info += $"{key}为{cataSet[key]}\r\n";
                }
            }
            cataSet[CH_LF] = setLF(cataSet[CH_LF]);
            info += $"换行符为：{cataSet[CH_LF]}";
            info += $"标题长度范围：" +
                $"[{headLength[MINLEN]},{headLength[MAXLEN]}]\r\n";
            return info;
        }
        
        // orders为全局变量lines的序号
        //查找目录和封面；目录和正文的分割线
        // 判断是否存在相同目录
        public List<Section> setCatalog(string[] lines)
        {
            // 此为可能为目录的行位置
            int[] orders = Enumerable.Range(0, lines.Length)
                                     .Where(i => isHead(lines[i]))
                                     .ToArray();
            // 可能是目录的行
            string[] Ls = orders.Select(i => lines[i]).ToArray();
            if (orders.Length == 0)
                return new List<Section>();

            List<string> L = new List<string>();
            int locCatalog = -1;
            int rStart = 0;
            for (int i = 0; i < Ls.Length; i++)
            {
                if (isCataLogHead(Ls[i]))
                    locCatalog = orders[i];
                if (!titleContains(L, Ls[i], boolSet[STRICT]))
                    L.Add(Ls[i]);
                else
                {
                    rStart = i;
                    break;
                }
            }

            //如果无目录字样，那么目录开始时就是目录的标题
            if (rStart > 0 && locCatalog == -1)
                locCatalog = orders[Array.IndexOf(lines, lines[rStart])];


            List<string> titles = new List<string>();
            List<int> stSecs = new List<int>();
            bool tailCata = boolSet[TAILCATA];

            // 如果第一个标题不为0，则此标题前面是封面
            if (orders[0] > 0)
            {
                titles.Add("封面");
                stSecs.Add(0);
            }


            // 此时目录不为第一章
            // 第一个重复项是文档开始的位置
            if (locCatalog > 0 && !tailCata && orders[rStart] >= locCatalog)
            {
                    titles.Add("目录");
                    stSecs.Add(locCatalog);
            }

            // 此时目录为第一章
            else if (locCatalog == 0)
            {
                titles.Add("目录");
                stSecs.Add(0);
            }

            titles.AddRange(tailCata ? Ls[..rStart] : Ls[rStart..]);
            stSecs.AddRange(tailCata ? orders[..rStart] 
                                              : orders[rStart..]);

            // 此时目录在结尾
            if (tailCata && locCatalog>0 && locCatalog <= orders[rStart])
            {
                titles.Add("目录");
                stSecs.Add(locCatalog);
            }

            stSecs.Add(lines.Length);

            return Enumerable.Range(0, titles.Count)
                .Select(i => new Section(i, stSecs[i], titles[i],
                    joinLines(lines[stSecs[i]..stSecs[i + 1]])))
                .ToList();
        }

        public void mergeCatalog(int gap = 5)
        {
            int i = 1;
            while (i < secs.Count)
            {
                if (secs[i].gapLess(secs[i - 1], gap))
                {
                    secs[i - 1].merge(secs[i]);
                    secs.RemoveAt(i);
                }
                else
                    i++;
            }
        }


        //某个标题是目录两个字
        public bool isCataLogHead(string line)
        {
            foreach (var ch in cataSet[CATANAME])
                line = line.Replace(ch.ToString(), "");
            return line.Trim() == "";
        }
        #endregion


        #region 判断是否为标题
        // 目前可能有改进的地方
        // 1 时间判断
        public bool isHead(string line)
        {
            if (heads.Contains(line.Trim()))
                return true;
            line = Regex.Replace(line, @"\s", "");  //去除空格
            
            if (line == "")
                return false;

            foreach (var ch in cataSet[FORMAL].Split(" "))
                if (line.StartsWith(ch))
                    return true;

            // 长度判断
            if (line.Length > headLength[MAXLEN] 
             || line.Length < headLength[MINLEN])
                return false;

            // 首尾判断
            foreach (var item in cataSet[NOSTART])
                if (line.StartsWith(item))
                    return false;

            foreach (var item in cataSet[NOEND])
                if (line.EndsWith(item))
                    return false;

            var chLine = line.ToCharArray();

            //非法字符判断
            if (cataSet[ILLEGAL].Intersect(chLine).Count() > 0)
                return false;

            //必备字符判断
            if (cataSet[REQUIRE] != ""
                && cataSet[REQUIRE].Intersect(chLine).Count() == 0)
                return false;

            if (boolSet[TAILMODE])
                return isTailTitle(line);
            else
                return isHeadTitle(line);
        }

        // 标题标志在结尾
        public bool isTailTitle(string line)
        {
            int i = line.Count() - 1;

            // 后缀不符合
            if (cataSet[SUFFIX].Length > 0
                && !cataSet[SUFFIX].Contains(line[i]))
                return false;

            i -= 1;
            int j = i;
            if (cataSet[NUMBER].Length > 0)
            {
                while (cataSet[NUMBER].Contains(line[i]))
                    i--;
                if (j == i)
                    return false;
            }

            if (cataSet[PREFIX].Length == 0)
                return true;

            i += 1;
            foreach (var ch in cataSet[PREFIX].Split(" "))
                if (line[..i].EndsWith(ch))
                    return true;
            return false;

        }

        public bool isHeadTitle(string line)
        {
            int i = 0;
            if (cataSet[PREFIX].Length > 0)
            {
                var prefix = cataSet[PREFIX].Split(" ");
                foreach (var ch in prefix)
                {
                    if (!line.StartsWith(ch))
                        continue;
                    i += ch.Length;
                    break;
                }
                if (i == 0 && prefix.Contains("\"\""))
                    return false;
            }

            int j = i;
            if (cataSet[NUMBER].Length > 0)
            {
                while (cataSet[NUMBER].Contains(line[j]))
                    j++;
                if (j == i)
                    return false;
            }

            if (cataSet[SUFFIX].Length>0 
                && !cataSet[SUFFIX] .Contains(line[j]))
                return false;

            return true;

        }

        // 标题是否互相包含
        public bool titleContains(List<string> T1s, string T2, bool isRegular)
        {
            if (!isRegular)
                return T1s.Contains(T2);
            foreach (var T1 in T1s)
            {
                if (T1.StartsWith(T2) || T2.StartsWith(T1))
                    return true;
            }
            return false;
        }
        #endregion

        #region 文本处理

        //删除首尾空格
        public void Trim()
        {
            lines = lines.Select(L => L.Trim()).ToList();
            update();
        }

        // 删除空行
        public void deleteDoubleLine()
        {
            int i = 0;
            while (i < lines.Count - 1)
            {
                if (lines[i] == "" & lines[i + 1] == "")
                {
                    lines.RemoveAt(i + 1);
                    continue;
                }
                i++;
            }
            update();
        }


        public string deleteAll(string s)
        {
            string info = "全文删除";
            int i, n;
            i = lines.Select(L => L.Length).Sum();
            foreach (var item in s.Split(cataSet[CH_LF]))
            {
                lines = lines.Select(L => L.Replace(item, "")).ToList();
                n = i;
                i = lines.Select(L => L.Length).Sum();
                info += $"{(n - i) / item.Length}个【{item}】\r\n";
            }
            return info;
        }


        // 删除所有匹配字符

        // 替换
        public int Replace(string tOld, string tNew, bool useRegex=true)
        {
            if (useRegex)
                lines.Select(L => Regex.Replace(L, tOld, tNew));
            else
                lines.Select(L => L.Replace(tOld, tNew)).ToList();
            update();
            return 1;

        }

        // 合并段落
        public void mergePara(string chStop)
        {
            int i = 0;
            while (i < lines.Count - 1)
            {
                if (ContainsTitle(lines[i]))
                {
                    i++;
                    continue;
                }

                if (lines[i] != "" && !chStop.Contains(lines[i].Last()))
                {
                    lines[i] = lines[i] + lines[i + 1];
                    lines.RemoveAt(i + 1);
                    continue;
                }
                i++;
            }
            update();
        }

        // 是否包含标题
        public bool ContainsTitle(string title)
        {
            foreach (var sec in secs)
                if (sec.title.Contains(title))
                    return true;
            return false;
        }

        #endregion

        // 繁简转换
        public void Convert(bool toSim)
        {
            var method = toSim ? (ChineseConversionDirection)1 : 0;
            lines = lines.Select(L =>
                ChineseConverter.Convert(L, method)).ToList();
            update(false);
        }

        public string joinLines(List<string> lines)
        {
            return joinLines(lines.ToArray());
        }

        public string joinLines(string[] lines)
        {
            return string.Join(cataSet[CH_LF], lines);
        }

        #region 输出
        // 输出
        public string output()
        {
            return joinLines(lines);
        }

        // 按行输出
        public string output(int stLine, int count)
        {
            if (stLine >= lines.Count)
                return "error！！";
            count = int.Min(lines.Count - stLine, count);
            return joinLines(lines.GetRange(stLine, count));
        }

        #endregion
    }

    public class BookInfo
    {
        public string name;         //书名
        public string folder;       //所在目录
        public string path;         //路径
        public int nWords;          //字数
        public long nBytes;          //字节数
        public string encoding;     //编码
        public string cataMode;     //目录模式
        public string lineMode;     //分行模式
        public int readSec;         //阅读到的章节位置
        public int readLine;        //阅读到的行位
        public ssDict kvpCataMode;

        public BookInfo(string path)
        {
            FileInfo fi = new FileInfo(path);
            var enc = CharsetDetector.DetectFromFile(path).Detected.Encoding;

            this.path = path;
            name = fi.Name;
            folder = fi.DirectoryName;
            encoding = enc.EncodingName;
            nWords = File.ReadAllText(path, enc).Length;
            nBytes = fi.Length;

        }

        public void setCataMode(ssDict dct)
        {
            kvpCataMode = dct;
        }

        public void setModeInfo(string cataMode, string lineMode)
        {
            this.cataMode = cataMode;
            this.lineMode = lineMode;
        }

        public void setSchedule(int readSec, int readLine)
        {
            this.readSec = readSec;
            this.readLine = readLine;
        }

        public override string ToString()
        {
            const double KB = 1024.0;
            const double MB = 1024 * 1024.0;

            string size = nBytes > MB ? $"{nBytes / MB:f1}MB"
                                      : $"{nBytes / KB:f1}kB";

            return $"《{name}》\r\n@{folder}\r\n" +
                $"总计{nWords / 1e4:f1}万字/{size}，" +
                $"编码为{encoding}\r\n";
        }

        public static string FromPath(string path)
        {
            return $"{new BookInfo(path)}";
        }
    }

    // 书架日志
    public class BookShelfInfo
    {
        Dictionary<string, BookInfo> dctBooks = new Dictionary<string, BookInfo>();
        string root;        //根目录
        string nBooks;      //书籍数

        public void Add(string path)
        {
            dctBooks[path] = new BookInfo(path);
        }
        

        public bool Contains(string path)
        {
            return dctBooks.ContainsKey(path);
        }



    }


}
