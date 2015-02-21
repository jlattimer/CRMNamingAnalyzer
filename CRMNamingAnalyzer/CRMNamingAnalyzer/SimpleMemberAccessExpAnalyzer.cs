using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace CRMNamingAnalyzer
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class SimpleMemberAccessExpAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "Dynamics CRM Attribute Name Analyzer - Simple Member Access Expression";
		internal const string Title = "Attribute name contains uppercase letters";
		internal const string MessageFormat = "Attribute name '{0}' contains uppercase letter(s)";
		internal const string Category = "Naming";

		internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, true);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxNodeAction(AnalyzeSmae, SyntaxKind.SimpleMemberAccessExpression);
		}

		private static void AnalyzeSmae(SyntaxNodeAnalysisContext context)
		{
			SyntaxNode node = context.Node;
			if (!node.IsKind(SyntaxKind.SimpleMemberAccessExpression)) return;

			MemberAccessExpressionSyntax mae = (MemberAccessExpressionSyntax)node;

			//account.Attributes.Add("name", "test");
			IdentifierNameSyntax identifier = mae.DescendantNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();
			if (identifier == null) return;

			ITypeSymbol type = context.SemanticModel.GetTypeInfo(identifier).Type;
			if (type == null) return;

			//If type is not Microsoft.Xrm.Sdk.Entity - exit
			if (type.ToString() != "Microsoft.Xrm.Sdk.Entity") return;

			//If arguement does not contain an upper case letter - exit
			var arguments = mae.Parent.DescendantNodes().OfType<ArgumentSyntax>();
			if (!arguments.Any()) return;

			if (!arguments.First().ToString().ToCharArray().Any(char.IsUpper)) return;

			var diagnostic = Diagnostic.Create(Rule, arguments.First().GetLocation(), arguments.First().Expression.GetFirstToken().ValueText);			
            context.ReportDiagnostic(diagnostic);
		}
	}
}