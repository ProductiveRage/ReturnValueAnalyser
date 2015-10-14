using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ProductiveRage.RuleAttributes;

namespace AssignReturnValueAnalyser
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class AssignReturnValueAnalyzer : DiagnosticAnalyzer
	{
        public const string DiagnosticId = "RetVal";
        private const string Category = "Design";
        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            GetLocalizableString(nameof(Resources.AnalyzerTitle)),
            GetLocalizableString(nameof(Resources.AnalyzerMessageFormat)),
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: GetLocalizableString(nameof(Resources.AnalyzerDescription))
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

		public override void Initialize(AnalysisContext context)
		{
            context.RegisterSyntaxNodeAction(LookForGetString, SyntaxKind.InvocationExpression);
        }

        private void LookForGetString(SyntaxNodeAnalysisContext context)
        {
            var invocation = context.Node as InvocationExpressionSyntax;
            if (invocation == null)
                return;

            // Note: Check these conditions first because they're cheaper than examining the method (having to call GetSymbolInfo)
            if ((context.Node.Parent == null) || DoesParentIndicateThatTheReturnValueIsBeingConsumed(context.Node.Parent))
                return;

            // If it looks like it might be somewhere that a method is being called (either as a member expression - eg. "a.GetValue()" - or
            // as a direct method call - eg. "GetValue()") then determine whether it's a method that we're interested in
            IMethodSymbol targetMethodIfAny;
            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
            if (memberAccess != null)
                targetMethodIfAny = context.SemanticModel.GetSymbolInfo(memberAccess).Symbol as IMethodSymbol;
            else
            {
                var identifierName = invocation.Expression as IdentifierNameSyntax;
                if (identifierName != null)
                    targetMethodIfAny = context.SemanticModel.GetSymbolInfo(identifierName).Symbol as IMethodSymbol;
                else
                    targetMethodIfAny = null;
            }
            if ((targetMethodIfAny == null)
            || targetMethodIfAny.ReturnsVoid
            || targetMethodIfAny.GetAttributes().Any(a => a.ToString() != typeof(ReturnValueMustNotBeIgnoredAttribute).FullName))
                return;

            // If it IS a non-void-returning method, decorated with the ReturnValueMustNotBeIgnored attribute, whose return value seems to
            // be getting thrown away, then warn about it!
            context.ReportDiagnostic(Diagnostic.Create(
                Rule,
                context.Node.GetLocation(),
                targetMethodIfAny.Name
            ));
        }

        private static bool DoesParentIndicateThatTheReturnValueIsBeingConsumed(SyntaxNode nodeParent)
        {
            if (nodeParent == null)
                throw new ArgumentNullException(nameof(nodeParent));

            if ((nodeParent is ArgumentSyntax)         // It's fine if the value is passed as an argument to another function
            || (nodeParent is BinaryExpressionSyntax)  // It's fine if the value is in a comparison (eg. a == b)
            || (nodeParent is EqualsValueClauseSyntax) // It's fine if a reference is being set to the return value (eg. a = b)
            || (nodeParent is ReturnStatementSyntax))  // It's fine if the value is being used as method return value
                return true;

            // If it's wrapped in brackets, then unwrap it and apply the same logic to that unwrapped content
            if (nodeParent is ParenthesizedExpressionSyntax)
            {
                if ((nodeParent.Parent == null) || DoesParentIndicateThatTheReturnValueIsBeingConsumed(nodeParent.Parent))
                    return true;
            }
            return false;
        }

        private static LocalizableString GetLocalizableString(string nameOfLocalizableResource)
        {
            return new LocalizableResourceString(nameOfLocalizableResource, Resources.ResourceManager, typeof(Resources));
        }
    }
}
