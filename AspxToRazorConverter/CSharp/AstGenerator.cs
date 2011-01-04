using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr.Runtime;
using Antlr.Runtime.Tree;
using AntlrCSharp;

namespace Dlrsoft.AspxToRazorConverter.CSharp
{
    public class AstGenerator
    {
        public List<Node> Build(string source)
        {
            ICharStream charStream = new ANTLRStringStream(source);
            return Build(charStream);
        }

        public List<Node> Build(ICharStream charStream)
        {
            var lexer = new csLexer(charStream);
            var parser = new csParser(new CommonTokenStream(lexer));

            var statement_list_return = parser.statement_list();
            var root = (ITree)statement_list_return.Tree;

            var nodes = new List<Node>();

            if (root.IsNil)
            {
                for (int i = 0; i < root.ChildCount; i++)
                {
                    var node = root.GetChild(i);
                    nodes.Add(Build(node));
                }
            }
            else
            {
                nodes.Add(Build(root));
            }

            return nodes;
        }

        public Node Build(ITree node)
        {
            if (node == null)
                return null;

            switch (node.Type)
            {
                //case csLexer.IF:

                default:
                    //throw new Exception(
                    //    String.Format("Unrecognized token '{0}'", Name(node))
                    //);
                    Console.WriteLine(node.Type);
                    return null;
            }
        }

        static public string Name(int type)
        {
            return csParser.tokenNames[type];
        }

        static public string Name(ITree node)
        {
            return Name(node.Type);
        }
    }
}
