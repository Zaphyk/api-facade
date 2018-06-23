using System;
using System.Collections.Generic;
using System.Globalization;
using ApiFacade.Parser;

namespace ApiFacade.Writer
{
    public abstract class FacadeWriter 
    {
        protected static Dictionary<MethodWriterType, Type> MethodWriters { get; }
        protected FacadeClass Class { get; }
        protected FacadeStringWriter Writer { get; }

        static FacadeWriter()
        {
            MethodWriters = new Dictionary<MethodWriterType, Type>
            {
                {MethodWriterType.Static, typeof(FacadeStaticMethodWriter)},
            };
        }

        protected FacadeWriter(FacadeClass Class)
        {
            this.Class = Class;
            this.Writer = new FacadeStringWriter();
        }

        public string Build()
        {

            this.SetIndentation(0);
            this.AppendUsings();

            this.SetIndentation(1);
            this.AppendComments();
            this.DefineClass();

            this.SetIndentation(2);
            this.DefineMethods();

            this.SetIndentation(1);
            this.CloseBracket();

            this.SetIndentation(0);
            this.CloseBracket();

            return Writer.ToString();
        }

        private void AppendUsings() 
        {
            for(var i = 0; i < Class.Usings.Length; i++) 
            {
                Writer.AppendLine($"using {Class.Usings[i]};");
            }
            Writer.AppendLine(string.Empty);
            Writer.AppendLine($"namespace {FacadeClass.Namespace}");
            Writer.AppendLine("{");
        }

        private void AppendComments() 
        {
            Writer.AppendLine("/*");
            Writer.AppendLine($"* Created {DateTime.Now.ToString(CultureInfo.CurrentCulture)}");
            Writer.AppendLine("* This code was automatically generated by api-facade");
            Writer.AppendLine("* https://github.com/Zaphyk/api-facade");
            Writer.AppendLine("*/");
        }

        protected virtual void DefineClass() 
        {
            Writer.Append(ClassDeclaration);
            Writer.AppendLine(string.Empty);
            Writer.AppendLine("{");
        }

        protected virtual void DefineMethods()
        {
            for (var i = 0; i < Class.Methods.Length; i++)
            {
                var methodWriter = (FacadeMethodWriter) Activator.CreateInstance(MethodWriters[Class.Methods[i].Type],
                    new object[]{Class, Class.Methods[i]});
                methodWriter.Write(Writer);
                if(i < Class.Methods.Length-1) Writer.AppendLine(string.Empty);
            }
        }

        private void CloseBracket()
        {
            Writer.AppendLine("}");
        }

        private void SetIndentation(int Level)
        {
            Writer.IndentationLevel = Level;
        }

        protected virtual string ClassDeclaration => $"{Class.ClassModifier ?? string.Empty} class {Class.Name} ";
    }
}