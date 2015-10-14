using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace AssignReturnValueAnalyser.Test
{
    [TestClass]
	public class ReturnValueMustNotBeIgnoredTests : CodeFixVerifier
	{
        [TestMethod]
		public void BlankContent()
		{
			VerifyCSharpDiagnostic("");
		}

        // TODO: More cases which are correctly NOT identified as being wrong

        [TestMethod]
        public void DirectFunctionCallThatIgnoresValue()
        {
            var testContent = GetTestContent("GetString(\"x\");");

            var expected = new DiagnosticResult
            {
                Id = "RetVal",
                Message = "The return value of 'GetString' should not be ignored",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", testContent.LineNumberOfStartOfTestContent, testContent.ColumNumberOfStartOfTestContent) }
            };

            VerifyCSharpDiagnostic(testContent.Content, expected);
        }

        [TestMethod]
        public void FunctionCallAsMemberAccessThatIgnoresValueTestMethod()
        {
            var testContent = GetTestContent("this.GetString(\"x\");");

            var expected = new DiagnosticResult
            {
                Id = "RetVal",
                Message = "The return value of 'GetString' should not be ignored",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", testContent.LineNumberOfStartOfTestContent, testContent.ColumNumberOfStartOfTestContent) }
            };

            VerifyCSharpDiagnostic(testContent.Content, expected);
        }

        [TestMethod]
        public void MemberAccessOnFunctionCallResult()
        {
            var testContent = GetTestContent("if (this.GetString(\"x\").Length > 0) { }");

            VerifyCSharpDiagnostic(testContent.Content);
        }

        private static TestContentSummary GetTestContent(string testStatements)
        {
            return new TestContentSummary(
                @"
                using System;
                using ProductiveRage.RuleAttributes;

                namespace ConsoleApplication1
                {
                    class Class1
                    {   
                        public void Test()
                        {   
                            " + testStatements + @"
                        }

                        [ReturnValueMustNotBeIgnored]
                        public string GetString(string name)
                        {
                            return name;
                        }
                    }
                }

                namespace ProductiveRage.RuleAttributes
                {
                    [AttributeUsage(AttributeTargets.Method)]
                    public sealed class ReturnValueMustNotBeIgnoredAttribute : Attribute { }
                }",
                lineNumberOfStartOfTestContent: 11,
                columNumberOfStartOfTestContent: 29
            );
        }

        public class TestContentSummary
        {
            public TestContentSummary(string content, int lineNumberOfStartOfTestContent, int columNumberOfStartOfTestContent)
            {
                if (string.IsNullOrEmpty(content))
                    throw new ArgumentException("Must be a non-null, non-blank value", nameof(content));
                if (lineNumberOfStartOfTestContent < 1)
                    throw new ArgumentException("Must be a positive integer", nameof(lineNumberOfStartOfTestContent));
                if (columNumberOfStartOfTestContent < 1)
                    throw new ArgumentException("Must be a positive integer", nameof(columNumberOfStartOfTestContent));

                Content = content;
                LineNumberOfStartOfTestContent = lineNumberOfStartOfTestContent;
                ColumNumberOfStartOfTestContent = columNumberOfStartOfTestContent;
            }

            public string Content { get; }
            public int LineNumberOfStartOfTestContent { get; }
            public int ColumNumberOfStartOfTestContent { get; }
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new NullCodeFixProvider();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new AssignReturnValueAnalyzer();
		}
	}
}