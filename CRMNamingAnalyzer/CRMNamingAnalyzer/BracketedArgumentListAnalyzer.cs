using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Text;

namespace CRMNamingAnalyzer
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class BracketedArgumentListAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "Dynamics CRM Attribute Name Analyzer - Bracketed Argument List";
		internal const string Title = "Attribute name contains uppercase letters";
		internal const string MessageFormat = "Attribute name '{0}' contains uppercase letter(s)";
		internal const string Category = "Naming";

		internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, true);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxNodeAction(AnalyzeBal, SyntaxKind.BracketedArgumentList);
		}

		private static void AnalyzeBal(SyntaxNodeAnalysisContext context)
		{
			SyntaxNode node = context.Node;
			if (!node.IsKind(SyntaxKind.BracketedArgumentList)) return;

			BracketedArgumentListSyntax bal = (BracketedArgumentListSyntax)node;
			ITypeSymbol type = null;

			//account["name"] = "test";
			if (bal.Parent.IsKind(SyntaxKind.ElementAccessExpression))
			{
				ElementAccessExpressionSyntax parent = (ElementAccessExpressionSyntax)bal.Parent;
				if (!parent.Expression.IsKind(SyntaxKind.IdentifierName)) return;

				IdentifierNameSyntax identifier = (IdentifierNameSyntax)parent.Expression;
				if (identifier != null)
					type = context.SemanticModel.GetTypeInfo(identifier).Type;
			}

			//Entity account = new Entity("account") { ["name"] = "test" };
			if (bal.Parent.IsKind(SyntaxKind.ImplicitElementAccess))
			{
				ImplicitElementAccessSyntax parent = (ImplicitElementAccessSyntax)bal.Parent;
				ObjectCreationExpressionSyntax identifier = parent.Ancestors().OfType<ObjectCreationExpressionSyntax>().First();
				if (identifier != null)
					type = context.SemanticModel.GetTypeInfo(identifier).Type;
			}

			if (type == null) return;
		
			//If type is not Microsoft.Xrm.Sdk.Entity - exit
			if (type.ToString() != "Microsoft.Xrm.Sdk.Entity") return;

			//If arguement does not contain an upper case letter - exit
			if (!bal.Arguments.ToString().ToCharArray().Any(char.IsUpper)) return;

			var diagnostic = Diagnostic.Create(Rule, node.GetLocation(), bal.Arguments[0].Expression.GetFirstToken().ValueText);
			context.ReportDiagnostic(diagnostic);
		}
	}
}
