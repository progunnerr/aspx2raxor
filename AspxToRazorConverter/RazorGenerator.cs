using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

using Dlrsoft.AspxToRazorConverter.Dom;

namespace Dlrsoft.AspxToRazorConverter
{
    public class RazorGenerator
    {
        public void Generate(AspxDocument doc, string outfile)
        {
            using (StreamWriter sw = new StreamWriter(outfile, false, new UTF8Encoding(true)))
            {
                if (!string.IsNullOrEmpty(doc.Inherits))
                {
                    //sw.WriteLine(string.Format("@inherits {0}", doc.Inherits));
                }


                foreach (Directive directive in doc.Directives)
                {
                    PageDirective pageDirective = directive as PageDirective;
                    if (pageDirective != null)
                    {
                        if (!string.IsNullOrEmpty(pageDirective.MasterPageFile))
                        {
                            sw.WriteLine(string.Format("@{{\r\n\tLayout = \"{0}\";\r\n}}", pageDirective.MasterPageFile.Replace(".Master", ".cshtml")));
                        }
                    }

                    ImportDirective importDirective = directive as ImportDirective;
                    if (importDirective != null)
                    {
                        sw.WriteLine(string.Format("@using {0}", importDirective.Namespace));
                    }
                }

                bool afterCodeBlock = false;
                foreach (Block block in doc.Blocks)
                {
                    HtmlBlock htmlBlock = block as HtmlBlock;
                    if (htmlBlock != null)
                    {
                        string contents = doc.Contents.Substring(block.Index, block.Length);
                        Regex regex = new Regex("^\\s*<"); //Check if started with <
                        if (!afterCodeBlock || regex.IsMatch(contents))
                        {
                            sw.Write(contents); //Write the contents directlhy
                        }
                        else
                        {
                            //Wrap it inside <text>...</text>
                            sw.Write("<text>");
                            sw.Write(contents);
                            sw.Write("</text>");
                        }
                    }

                    afterCodeBlock = false;
                    CodeBlock codeBlock = block as CodeBlock;
                    if (codeBlock != null)
                    {
                        if (codeBlock.IsFirst)
                        {
                            sw.WriteLine(" @{");
                        }

                        sw.WriteLine("\r\n" + doc.Contents.Substring(block.Index + 2, block.Length - 4));

                        if (codeBlock.IsLast)
                        {
                            sw.WriteLine("}");
                        }

                        afterCodeBlock = true;
                    }

                    ExpressionBlock expressionBlock = block as ExpressionBlock;
                    if (expressionBlock != null)
                    {
                        sw.Write(string.Format("@{0} ", doc.Contents.Substring(block.Index + 3, block.Length - 5).Trim()));
                    }

                    CommentBlock commentBlock = block as CommentBlock;
                    if (commentBlock != null)
                    {
                        sw.Write(string.Format("@*{0}*@", doc.Contents.Substring(block.Index + 3, block.Length - 6)));
                    }

                    ContentPlaceHolderBlock cphBlock = block as ContentPlaceHolderBlock;
                    if (cphBlock != null)
                    {
                        if ("MainContent".Equals(cphBlock.ID))
                        {
                            sw.WriteLine("\r\n\t@RenderBody()");
                        }
                    }
                }
            }
        }
    }
}
