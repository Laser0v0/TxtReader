using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TxtReader
{
    enum SectionType { 常规, 目录, 封面, 封底}

    public class Section
    {
        public int order;           // 章节号
        public string title;
        public string content;      // 文本内容
        public List<string> pages;  // 章节分页
        public int stLine;          // 起始行号
        public int wCount;          // 字数

        
        public Section(int order, int stLine, string title, string content)
        {
            this.order = order;
            this.stLine = stLine;
            this.title = title.Trim();
            this.content = content.Trim();
            wCount = content.Length;
        }

        // 输入为页号和内容
        public Section(int i, int st, string content)
        {
            order = i;
            stLine = st;
            title = $"第{i}页";
            this.content = content.Trim();
            wCount = content.Length;
        }

        public static Section fromPages(int i, int step, List<string> lines, string chLF)
        {
            int st = i * step;
            string content = string.Join(chLF, lines.GetRange(st, step));
            return new Section(i, st, content);
        }

        public void merge(Section sec)
        {
            title += $" {sec.title}";
            content += $"{Environment.NewLine}{sec.content}";
        }

        public static int operator -(Section left, Section right)
        {
            return left.order - right.order;
        }

        public bool gapLess(Section right,  int gap)
        {
            return stLine-right.stLine < gap;
        }

    }


}
