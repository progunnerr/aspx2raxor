using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr.Runtime;
using Dlrsoft.AspxToRazorConverter.Dom;

namespace Dlrsoft.AspxToRazorConverter
{
    public class CodeBlockStream : ANTLRStringStream
    {
        protected AspxDocument _doc;
        protected CodeBlock _currentCodeBlock;
        protected int _currentEndIndex;

        public CodeBlockStream(AspxDocument doc) : base(doc.Contents)
        {
            this._doc = doc;

            foreach (Block b in _doc.Blocks)
            {
                CodeBlock cb = b as CodeBlock;
                if (cb != null)
                {
                    _currentCodeBlock = cb;
                    _currentEndIndex = _currentCodeBlock.Index + _currentCodeBlock.Length;
                    this.Seek(_currentCodeBlock.Index + 2); //Skip "<%"
                    break;
                }
            }
        }

        public override int LA(int i)
        {
            if (_currentCodeBlock == null)
            {
                return (int)CharStreamConstants.EOF;
            }

            int j = i;
            if (p + i > _currentEndIndex - 2)
            {
                j = p + i - _currentEndIndex + 2; //Do not include "%>"
                _currentCodeBlock = _currentCodeBlock.NextCodeBlock;
                if (_currentCodeBlock == null)
                {
                    return (int)CharStreamConstants.EOF;
                }
                _currentEndIndex = _currentCodeBlock.Index + _currentCodeBlock.Length;
                this.Seek(_currentCodeBlock.Index + 2); //Skip "<%"
            }
            return base.LA(j);
        }
    }
}
