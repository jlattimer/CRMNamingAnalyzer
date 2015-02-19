using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using TestHelper;

namespace CRMNamingAnalyzer.Test
{
	[TestClass]
	public class BracketedArgumentListCodeFixUnitTest : CodeFixVerifier
	{

		//No diagnostics expected to show up
		[TestMethod]
		public void TestMethod1()
		{
			const string test = @"";

			VerifyCSharpDiagnostic(test);
		}

		//Diagnostic and CodeFix both triggered and checked for
		[TestMethod]
		public void TestMethod2()
		{
			//Either use the full namespace + class names for referenced assemblies or add the proper reference(s) in DiagnosticVerifier.Helper.cs
			StringBuilder test = new StringBuilder();
			test.Append("using Microsoft.Xrm.Sdk;");
			test.Append("namespace CRMSimpleConsole2");
			test.Append("{");
			test.Append("    class Program");
			test.Append("    {");
			test.Append("        static void Main()");
			test.Append("        {");
			test.Append("            Microsoft.Xrm.Sdk.Entity account = new Microsoft.Xrm.Sdk.Entity(\"account\");");
			test.Append("            account[\"Name\"] = \"test1\";");
			test.Append("        }");
			test.Append("    }");
			test.Append("}");

			var expected = new DiagnosticResult
			{
				Id = BracketedArgumentListAnalyzer.DiagnosticId,
				Message = String.Format("Attribute name '{0}' contains uppercase letter(s)", "Name"),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] { new DiagnosticResultLocation("Test0.cs", 1, 216) }
			};

			VerifyCSharpDiagnostic(test.ToString(), expected);

			//Either use the full namespace + class names for referenced assemblies or add the proper reference(s) in DiagnosticVerifier.Helper.cs
			StringBuilder fixtest = new StringBuilder();
			fixtest.Append("using Microsoft.Xrm.Sdk;");
			fixtest.Append("namespace CRMSimpleConsole2");
			fixtest.Append("{");
			fixtest.Append("    class Program");
			fixtest.Append("    {");
			fixtest.Append("        static void Main()");
			fixtest.Append("        {");
			fixtest.Append("            Microsoft.Xrm.Sdk.Entity account = new Microsoft.Xrm.Sdk.Entity(\"account\");");
			fixtest.Append("            account[\"name\"] = \"test1\";");
			fixtest.Append("        }");
			fixtest.Append("    }");
			fixtest.Append("}");

			VerifyCSharpFix(test.ToString(), fixtest.ToString(), null, true);
		}

		//Diagnostic and CodeFix both triggered and checked for
		[TestMethod]
		public void TestMethod3()
		{
			//Either use the full namespace + class names for referenced assemblies or add the proper reference(s) in DiagnosticVerifier.Helper.cs
			StringBuilder test = new StringBuilder();
			test.Append("using Microsoft.Xrm.Sdk;");
			test.Append("namespace CRMSimpleConsole2");
			test.Append("{");
			test.Append("    class Program");
			test.Append("    {");
			test.Append("        static void Main()");
			test.Append("        {");
			test.Append("            Microsoft.Xrm.Sdk.Entity account = new Microsoft.Xrm.Sdk.Entity(\"account\") {[\"Name\"] = \"test1\"};");
			test.Append("        }");
			test.Append("    }");
			test.Append("}");

			var expected = new DiagnosticResult
			{
				Id = BracketedArgumentListAnalyzer.DiagnosticId,
				Message = String.Format("Attribute name '{0}' contains uppercase letter(s)", "Name"),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] { new DiagnosticResultLocation("Test0.cs", 1, 198) }
			};

			VerifyCSharpDiagnostic(test.ToString(), expected);

			//Either use the full namespace + class names for referenced assemblies or add the proper reference(s) in DiagnosticVerifier.Helper.cs
			StringBuilder fixtest = new StringBuilder();
			fixtest.Append("using Microsoft.Xrm.Sdk;");
			fixtest.Append("namespace CRMSimpleConsole2");
			fixtest.Append("{");
			fixtest.Append("    class Program");
			fixtest.Append("    {");
			fixtest.Append("        static void Main()");
			fixtest.Append("        {");
			fixtest.Append("            Microsoft.Xrm.Sdk.Entity account = new Microsoft.Xrm.Sdk.Entity(\"account\") {[\"name\"] = \"test1\"};");
			fixtest.Append("        }");
			fixtest.Append("    }");
			fixtest.Append("}");

			VerifyCSharpFix(test.ToString(), fixtest.ToString(), null, true);
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new BracketedArgumentListCodeFix();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new BracketedArgumentListAnalyzer();
		}
	}
}