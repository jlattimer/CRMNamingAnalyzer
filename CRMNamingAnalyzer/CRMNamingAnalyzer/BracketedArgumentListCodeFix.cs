using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CRMNamingAnalyzer
{
	[ExportCodeFixProvider("BracketedArgumentListCodeFix", LanguageNames.CSharp), Shared]
	public class BracketedArgumentListCodeFix : CodeFixProvider
	{
		public sealed override ImmutableArray<string> GetFixableDiagnosticIds()
		{
			return ImmutableArray.Create(BracketedArgumentListAnalyzer.DiagnosticId);
		}

		public sealed override FixAllProvider GetFixAllProvider()
		{
			return WellKnownFixAllProviders.BatchFixer;
		}

		public sealed override async Task ComputeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			var diagnostic = context.Diagnostics.First();
			var diagnosticSpan = diagnostic.Location.SourceSpan;

			// Find the type declaration identified by the diagnostic.
			var declaration = root.FindToken(diagnosticSpan.Start).Parent;

			// Register a code action that will invoke the fix.
			context.RegisterFix(
				CodeAction.Create("Make lowercase", c => MakeLowercaseAsync(context.Document, declaration, c)),
				diagnostic);
		}

		private async Task<Document> MakeLowercaseAsync(Document document, SyntaxNode node, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken);
			BracketedArgumentListSyntax bal = (BracketedArgumentListSyntax)node;
			var newFieldName = bal.Arguments.FirstOrDefault().ToString().ToLowerInvariant();

			var newNode = SyntaxFactory.BracketedArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>().Add(
						SyntaxFactory.Argument(null, SyntaxFactory.Token(SyntaxKind.None),
						SyntaxFactory.IdentifierName(newFieldName))));

			var newRoot = root.ReplaceNode(node, newNode);

			return document.WithSyntaxRoot(newRoot);
		}
	}
}