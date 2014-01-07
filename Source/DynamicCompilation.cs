using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Mono.CSharp;

namespace PartCatalog
{

    public delegate bool ParsingRule(AvailablePart part, HashSet<string> categories);

    class DynamicCompilationHandler
    {





        List<ParsingRule> list = new List<ParsingRule>();        
                                                        
        public void CompileFile(string path)
        {
            CompilerContext context = new CompilerContext(new CompilerSettings(), new ConsoleReportPrinter());
            Evaluator evaluator = new Evaluator(context);

            evaluator.ReferenceAssembly(typeof(ParsingRule).Assembly);

            string input = KSP.IO.File.ReadAllText<PartCatalog>(path);
            input = String.Format(@"PartCatalog.ParsingRule testVar = (part, categories) => {{ {0} }};", input);

            evaluator.Run(input);
            object output = null;
            bool result_set = false;
            evaluator.Evaluate("testVar", out output, out result_set);
            evaluator.Run("testVar = null;");
            if(result_set)
            {
                list.Add((ParsingRule)output);
            }            
            //CompiledMethod compiled = null;
            //string value = evaluator.Compile(input,out compiled);

        }

        public HashSet<string> RunRuleOnPart(AvailablePart part)
        {
            HashSet<string> categories = new HashSet<string>();
            foreach (ParsingRule rule in list)
            {
                rule(part, categories);
            }
            return categories;
        }

        public void ListCategories(AvailablePart part)
        {
            UnityEngine.Debug.Log("Got Part " + part.name);
            foreach(var cat in RunRuleOnPart(part))
            {
                UnityEngine.Debug.Log("Got category: " + cat);
            }
        }
    }
}
