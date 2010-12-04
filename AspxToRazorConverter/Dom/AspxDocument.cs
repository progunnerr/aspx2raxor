using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dlrsoft.AspxToRazorConverter.Dom
{
    public class AspxDocument
    {
        public AspxDocument(string contents)
        {
            this.Contents = contents;
        }

        protected IList<Directive> _directives = new List<Directive>();
        protected IList<Block> _blocks = new List<Block>();

        public IList<Directive> Directives { get { return _directives; } }
        public IList<Block> Blocks { get { return _blocks; } set { _blocks = value; } }

        public LanguageEnum Language { get; set; }
        public string Inherits { get; set; }
        public bool HasMaster { get; set; }
        public bool IsMaster { get; set; }
        public string Contents { get; set; }
        public int ContentStart { get; set; }
        public int ContentLength { get; set; }
    }
}
