using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr.Runtime.Tree;

namespace Dlrsoft.AspxToRazorConverter.CSharp
{
    public enum NodeType
    {
        Assign, Identifier, Number, Null,
        MemberAccess, Call, If, Eq, Block,
        String, ClrNew, Lambda, While, BinaryOp,
        Object, New, AutoProperty, Return,
        UnaryOp, Logical, PostfixOperator,
        TypeOf, Boolean, Void, StrictCompare,
        UnsignedRightShift, ForStep, ForIn,
        Break, Continue, With, Try, Catch,
        Throw, IndexAccess, Delete, In,
        Switch,
        InstanceOf,
        Regex
    }

    abstract public class Node
    {
        public readonly NodeType Type;
        public readonly int Line;
        public readonly int Column;

        public Node(NodeType type, ITree node)
        {
            Line = node.Line;
            Column = node.CharPositionInLine;
            Type = type;
        }

        public string Print()
        {
            var writer = new StringBuilder();

            Print(writer);

            return writer.ToString();
        }

        public virtual void Print(StringBuilder writer, int indent = 0)
        {
            var indentStr = new String(' ', indent * 2);
            writer.AppendLine(indentStr + "(" + Type + ")");
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }
}
