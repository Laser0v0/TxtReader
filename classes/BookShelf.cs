using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using FolderBrowserForWPF;


namespace TxtReader
{
    internal class BookShelf
    {
        public string root;        //根目录
        public string name;        //最上层的文件夹
        public List<BookShelf> childs = new List<BookShelf>();
        public string[] books = new string[] { };


        public BookShelf(string root)
        {
            init(root);
        }

        public BookShelf()
        {
            var fbd = new Dialog();
            if (fbd.ShowDialog() != true)
                return;
            init(fbd.FileName);
        }

        public void init(string root)
        {
            this.root = root;
            name = Path.GetFileName(root);
            foreach (var d in Directory.GetDirectories(root))
            {
                if (!isBookShelf(root))
                    continue;
                childs.Add(new BookShelf(d));
            }
            books = getAllChilds(root);
        }

        public string[] getAllChilds(string path)
        {
            return Directory
                .GetFiles(path, "*.txt", SearchOption.TopDirectoryOnly)
                .Select(f => Path.GetFileName(f))
                .ToArray();
        }

        public bool isBookShelf(string path)
        {
            var txts = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
            return txts.Length > 0;
        }

        // 应该会更快吧
        public bool isBookShelfRec(string path)
        {
            var txts = Directory.GetFiles(path, 
                "*.txt", SearchOption.TopDirectoryOnly);
            if (txts.Length > 0) return true;
            foreach(var p in Directory.GetDirectories(path))
                if(isBookShelfRec(p)) return true;
            return false;
        }

        public override string ToString()
        {
            string info = $"{name}\r\n";
            foreach (var bs in childs)
                info += bs.ToString() + "\r\n";
            foreach (var book in books)
                info += $"-{book}\r\n";
            return info;
        }

        public int findBookOrder(string book)
        {
            // 未找到返回-1
            return Array.IndexOf(books, book);
        }


    }
}
