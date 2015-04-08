﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarQube.Analyzers.Helpers;
using SonarQube.Analyzers.SonarQube.Settings;
using SonarQube.Analyzers.SonarQube.Settings.Sqale;

namespace SonarQube.Analyzers.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [SqaleConstantRemediation("5min")]
    [SqaleSubCharacteristic(SqaleSubCharacteristic.Readability)]
    [Rule(DiagnosticId, RuleSeverity, Description, IsActivatedByDefault)]
    [Tags("bug", "cwe", "misra")]
    [LegacyKey("AssignmentInSubExpressionCheck", "AssignmentInSubExpression", "AssignmentWithinCondition", "AssignmentInsideSubExpression")]
    public class AssignmentInsideSubExpression : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1121";
        internal const string Description = "Assignment should not be used inside sub-expressions";
        internal const string MessageFormat = "Extract this assignment outside of the sub-expression.";
        internal const string Category = "SonarQube";
        internal const Severity RuleSeverity = Severity.Major;
        internal const bool IsActivatedByDefault = true;

        internal static DiagnosticDescriptor Rule = 
            new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, 
                RuleSeverity.ToDiagnosticSeverity(), IsActivatedByDefault, 
                helpLinkUri: "http://nemo.sonarqube.org/coding_rules#rule_key=csharpsquid%3AAssignmentInsideSubExpression");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (IsInSubExpression(c.Node))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, c.Node.GetLocation()));
                    }
                },
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxKind.AddAssignmentExpression,
                SyntaxKind.SubtractAssignmentExpression,
                SyntaxKind.MultiplyAssignmentExpression,
                SyntaxKind.DivideAssignmentExpression,
                SyntaxKind.ModuloAssignmentExpression,
                SyntaxKind.AndAssignmentExpression,
                SyntaxKind.ExclusiveOrAssignmentExpression,
                SyntaxKind.OrAssignmentExpression,
                SyntaxKind.LeftShiftAssignmentExpression,
                SyntaxKind.RightShiftAssignmentExpression);
        }

        private static bool IsInSubExpression(SyntaxNode node)
        {
            var expression = node.Parent.FirstAncestorOrSelf<ExpressionSyntax>(ancestor => ancestor != null);

            return expression != null &&
                   !AllowedParentExpressionKinds.Contains(expression.Kind());
        }

        private static IEnumerable<SyntaxKind> AllowedParentExpressionKinds
        {
            get
            {
                return new[]
                {
                    SyntaxKind.ParenthesizedLambdaExpression,
                    SyntaxKind.SimpleLambdaExpression,
                    SyntaxKind.AnonymousMethodExpression,
                    SyntaxKind.ObjectInitializerExpression
                };
            }
        }
    }
}
