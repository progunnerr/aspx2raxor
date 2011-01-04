using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.RegularExpressions;
using System.IO;
using Dlrsoft.AspxToRazorConverter.Dom;
using Dlrsoft.AspxToRazorConverter.CSharp;

namespace Dlrsoft.AspxToRazorConverter
{
    /// <summary>
    /// A single regular expression based parser
    /// </summary>
    public class AspxParser
    {
        //private string _contents;
        private AspxDocument _doc;
        private static Regex _noneSpaceRegex = new Regex("\\S");
        private static SimpleDirectiveRegex _simpleDirectiveRegex = new SimpleDirectiveRegex();
        private static Regex _contentRegex = new Regex(ContentRegexPattern, RegexOptions.Singleline);
        private static Regex _codeRegex = new Regex("(" + CodeRegexPattern + "|" + ContentPlaceHolderRegexPattern + ")", RegexOptions.Singleline);

        public const string CSHARP = "C#";
        public const string VB = "VB";
        //public const string ControlElementRegExPattern = @"<{0}(\W[^>]*)?(/>|>[\w\W]*?</{0}>)";
        public const string ContentRegexPattern = "<asp:Content(\\s+ID=\"(?<id>\\w+)\"|\\s+ContentPlaceHolderID=\"(?<cphid>\\w+)\"|\\s+runat=\"server\")+\\s*>(?<content>.*?)</asp:Content>";
        public const string CodeRegexPattern = "<%(?!@)(?<code>.*?)%>";
        public const string ContentPlaceHolderRegexPattern = "<asp:ContentPlaceHolder(\\s+ID=\"(?<id>\\w+)\"|\\s+runat=\"server\")+\\s*/>";

        public AspxDocument Parse(TextReader reader)
        {
            return Parse(reader.ReadToEnd());
        }

        public AspxDocument Parse(string contents)
        {
            //_contents = contents;
            _doc = new AspxDocument(contents) 
            {
                ContentStart = 0,
                ContentLength = contents.Length
            };
            
            //1. Extract the directives
            ParseDirectives();

            //2. If has master page, extract hte MainContent
            if (_doc.HasMaster)
            {
                ParseMainContent();
            }

            //3. Extract Code Blocks
            ParseCodeBlocks();

            //4. Find all the literals between code blocks
            ParseLiteralBlocks();

            //5. Parse C# code
            CodeBlockStream stream = new CodeBlockStream(_doc);
            AstGenerator generator = new AstGenerator();
            generator.Build(stream);

            return _doc;
        }

        protected void ParseDirectives()
        {
            MatchCollection matchs = _simpleDirectiveRegex.Matches(_doc.Contents);
            Match lastMatch = null;
            foreach (Match match in matchs)
            {
                lastMatch = match;
                Group attrname = match.Groups["attrname"];
                Group attrval = match.Groups["attrval"];

                string directiveName = attrname.Captures[0].Value;
                switch (directiveName.ToLower())
                {
                    case "page":
                        ParsePageDirective(attrname, attrval);
                        break;
                    case "control":
                        ParsePageDirective(attrname, attrval);
                        break;
                    case "import":
                        ParseImportDirective(attrname, attrval);
                        break;

                }
            }
            //Move ContentStart to end of last matchs
            if (lastMatch != null)
            {
                _doc.ContentStart = lastMatch.Index + lastMatch.Length;
            }
        }

        protected void ParsePageDirective(Group attrname, Group attrval)
        {
            PageDirective pd = new PageDirective();
            string name;
            string value;
            for (int i = 1; i < attrname.Captures.Count; i++)
            {
                name = attrname.Captures[i].Value;
                value = attrval.Captures[i].Value;
                switch (name.ToLower())
                {
                    case "language":
                        switch (value)
                        {
                            case CSHARP:
                                _doc.Language = LanguageEnum.CSharp;
                                break;
                            case VB:
                                _doc.Language = LanguageEnum.VB;
                                break;
                        }
                        break;
                    case "inherits":
                        _doc.Inherits = value;
                        break;
                    case "masterpagefile":
                        _doc.HasMaster = true;
                        pd.MasterPageFile = value;
                        break;
                }
            }
            _doc.Directives.Add(pd);
        }

        protected void ParseImportDirective(Group attrname, Group attrval)
        {
            ImportDirective id = new ImportDirective();
            string name;
            string value;
            for (int i = 1; i < attrname.Captures.Count; i++)
            {
                name = attrname.Captures[i].Value;
                value = attrval.Captures[i].Value;
                switch (name.ToLower())
                {
                    case "namespace":
                        id.Namespace = value;
                        break;
                }
            }
            _doc.Directives.Add(id);
        }

        protected void ParseMainContent()
        {
            MatchCollection matches = _contentRegex.Matches(_doc.Contents);
            foreach (Match m in matches)
            {
                string id = m.Groups["id"].Value;
                string cphid = m.Groups["cphid"].Value;
                if (cphid.Equals("MainContent"))
                {
                    Group contentGroup = m.Groups["content"];
                    _doc.Contents = contentGroup.Value;
                    _doc.ContentStart = 0;
                    _doc.ContentLength = contentGroup.Length;
                }
            }
        }

        protected void ParseCodeBlocks()
        {
            MatchCollection matches = _codeRegex.Matches(_doc.Contents);
            CodeBlock cb = null;
            foreach (Match m in matches)
            {
                string s = m.Value;
                Block block = null;
                if (s.StartsWith("<%=") || s.StartsWith("<%:"))
                {
                    block = new ExpressionBlock();
                }
                else if (s.StartsWith("<%--"))
                {
                    block = new CommentBlock();
                }
                else if (s.StartsWith("<asp:ContentPlaceHolder"))
                {
                    string id = m.Groups["id"].Value;
                    block = new ContentPlaceHolderBlock { ID = id };
                }
                else
                {
                    block = new CodeBlock();
                    if (cb == null)
                    {
                        cb = (CodeBlock)block;
                        cb.IsFirst = true;
                    }
                    else
                    {
                        cb.NextCodeBlock = (CodeBlock)block;
                        cb = (CodeBlock)block;
                    }
                }
                block.Index = m.Index;
                block.Length = m.Length;
                _doc.Blocks.Add(block);
            }
        }

        protected void ParseLiteralBlocks()
        {
            int index = _doc.ContentStart;
            IList<Block> blocks = new List<Block>();

            foreach (Block block in _doc.Blocks)
            {
                if (block.Index > index)
                {
                    //There is a literal
                    blocks.Add(new HtmlBlock() { Index = index, Length = block.Index - index });
                }
                blocks.Add(block);
                index = block.Index + block.Length;
            }
            if (index < _doc.ContentLength)
            {
                blocks.Add(new HtmlBlock() { Index = index, Length = _doc.ContentLength - index });
            }
            _doc.Blocks = blocks;
        }



    }
}
