using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Components.Locker
{
    public class TagFileController
    {
        private string BaseLine { get; set; } = string.Empty;

        public class TagSegment
        {
            public string Tag { get; private set; }
            public string Path
            {
                get
                {
                    if (Parent == null)
                    {
                        return Tag;
                    }
                    else
                    {
                        return Parent.Path + @"/" + Tag;
                    }
                }
            }
            public TagSegment Parent { get; internal set; }
            public List<TagSegment> Children { get; private set; } = new List<TagSegment>();
            public List<string> Elements { get; private set; } = new List<string>();

            public string GetTree(int depth = 0)
            {
                string tab = string.Empty;
                for (int i = 0; i < depth; i++)
                {
                    tab += "\t";
                }
                string res = tab + "<" + Tag + ">\n";
                foreach (var item in Elements)
                {
                    res += tab + "\t" + item + "\n";
                }
                foreach (var item in Children)
                {
                    res += item.GetTree(depth + 1);
                }
                res += tab + @"</" + Tag + ">\n";
                return res;
            }

            internal TagSegment(string tag, TagSegment parent)
            {
                Tag = tag;
                Parent = parent;
            }

            public TagSegment(string tag)
            {
                Tag = tag;
            }

            #region Add
            internal void AddValue(object value)
            {
                Elements.Add(value.ToString());
            }

            public void AddValue(string tag, object value)
            {
                var list = Find(tag);
                if (list.Count == 0)
                {
                    var newtag = new TagSegment(tag, this);
                    newtag.AddValue(value);
                    Children.Add(newtag);
                }
                else
                {
                    list[0].AddValue(value);
                }
            }

            public TagSegment AddTag(string tag)
            {
                var newtag = new TagSegment(tag, this);
                Children.Add(newtag);
                return newtag;
            }
            public TagSegment AddTag(TagSegment tag)
            {
                tag.Parent = this;
                Children.Add(tag);
                return tag;
            }
            #endregion

            #region Find
            public List<TagSegment> Find(string tag)
            {
                var list = new List<TagSegment>();
                foreach (var item in Children)
                {
                    if (item.Tag == tag)
                    {
                        list.Add(item);
                    }
                }
                return list;
            }
            #endregion
        }
        public TagSegment Root { get; private set; }
        public string RootTag { get { return Root.Tag; } }

        public string Tree
        {
            get
            {
                return Root.GetTree();
            }
        }

        public TagFileController(string MasterTag)
        {
            Root = new TagSegment(MasterTag, null);
        }

        public bool AddTry(string path, TagSegment tagitem)
        {
            List<string> tags = new List<string>(path.Split(new string[] { @"/" }, StringSplitOptions.RemoveEmptyEntries));
            if (tags[0] == RootTag) { tags.RemoveAt(0); }
            var target = Root;
            for (int i = 0; i < tags.Count; i++)
            {
                bool check = false;
                foreach (var item in target.Children)
                {
                    if (item.Tag == tags[0])
                    {
                        target = item;
                        check = true;
                        break;
                    }
                }
                if (!check) { return false; }
            }
            tagitem.Parent = target;
            target.Children.Add(tagitem);
            return true;
        }
        public bool AddTry(string path, object valueitem)
        {
            List<string> tags = new List<string>(path.Split(new string[] { @"/" }, StringSplitOptions.RemoveEmptyEntries));
            if (tags[0] == RootTag) { tags.RemoveAt(0); }
            var target = Root;
            for (int i = 0; i < tags.Count; i++)
            {
                bool check = false;
                foreach (var item in target.Children)
                {
                    if (item.Tag == tags[0])
                    {
                        target = item;
                        check = true;
                        break;
                    }
                }
                if (!check) { return false; }
            }
            target.AddValue(valueitem);
            return true;
        }
        public void Add(TagSegment tagitem, string path = null)
        {
            var target = Root;
            if (path != null)
            {
                List<string> tags = new List<string>(path.Split(new string[] { @"/" }, StringSplitOptions.RemoveEmptyEntries));
                if (tags[0] == RootTag) { tags.RemoveAt(0); }
                for (int i = 0; i < tags.Count; i++)
                {
                    bool check = false;
                    foreach (var item in target.Children)
                    {
                        if (item.Tag == tags[0])
                        {
                            target = item;
                            check = true;
                            break;
                        }
                    }
                    if (!check)
                    {
                        target = target.AddTag(tags[i]);
                    }
                }
            }
            tagitem.Parent = target;
            target.Children.Add(tagitem);
        }
        public void Add(object valueitem, string path = null)
        {
            var target = Root;
            if (path != null)
            {
                List<string> tags = new List<string>(path.Split(new string[] { @"/" }, StringSplitOptions.RemoveEmptyEntries));
                if (tags[0] == RootTag) { tags.RemoveAt(0); }
                for (int i = 0; i < tags.Count; i++)
                {
                    bool check = false;
                    foreach (var item in target.Children)
                    {
                        if (item.Tag == tags[0])
                        {
                            target = item;
                            check = true;
                            break;
                        }
                    }
                    if (!check)
                    {
                        target = target.AddTag(tags[i]);
                    }
                }
            }
            target.AddValue(valueitem);
        }

        public void Save(string path)
        {
            using (var fs = new System.IO.StreamWriter(path))
            {
                fs.Write(Tree);
            }
        }

        public static TagFileController Load(string path)
        {
            string text = string.Empty;
            using (var fs = new System.IO.StreamReader(path))
            {
                text = fs.ReadToEnd();
            }



            return null;
        }
    }
}
