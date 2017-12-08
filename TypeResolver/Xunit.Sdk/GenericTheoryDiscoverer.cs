
using System.Collections.Generic;
using System.Linq;
using TypeResolver;
using Xunit.Abstractions;


namespace Xunit.Sdk {

    /// <summary>
    /// Implementation of <see cref="IXunitTestCaseDiscoverer"/> that supports finding test cases
    /// on methods decorated with <see cref="Xunit.Extensions.GenericTheoryAttribute"/>.
    /// </summary>
    public class GenericTheoryDiscoverer : DiscovererWrapper {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericTheoryDiscoverer"/> class.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        public GenericTheoryDiscoverer( IMessageSink diagnosticMessageSink )
            : base( new TheoryDiscoverer( diagnosticMessageSink ), diagnosticMessageSink ) { }

        /// <inheritdoc/>
        protected override ICollection<ITestMethod> GetConcreteMethods( ITestMethod testMethod ) {
            var methodInfo = testMethod.Method.ToRuntimeMethod( );
            var concreteMethods = GenericTypeResolver.GetConcreteMethods( methodInfo );

            return concreteMethods
                .Select( concreteMethod => (ITestMethod)new TestMethod( testMethod.TestClass, Reflector.Wrap( concreteMethod ) ) )
                .ToList( );
        }

        /// <inheritdoc/>
        protected override XunitTestCase ConvertXunitTestCase( XunitTestCase testCase, TestMethodDisplay defaultMethodDisplay, IMessageSink diagnosticMessageSink, int timeout, bool useStaThread ) {
            return new GenericXunitTestCase( diagnosticMessageSink, defaultMethodDisplay, testCase.TestMethod, testCase.TestMethodArguments, timeout, useStaThread );
        }

        /// <inheritdoc/>
        protected override XunitTheoryTestCase ConvertTheoryTestCase( XunitTheoryTestCase testCase, TestMethodDisplay defaultMethodDisplay, IMessageSink diagnosticMessageSink, int timeout, bool useStaThread ) {
            return new GenericXunitTheoryTestCase( diagnosticMessageSink, defaultMethodDisplay, testCase.TestMethod, timeout, useStaThread );
        }
    }

}
