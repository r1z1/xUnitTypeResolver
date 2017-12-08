
using System.Collections.Generic;
using System.Linq;
using TypeResolver.Extensions;
using TypeResolver.Internal;
using Xunit.Abstractions;


namespace Xunit.Sdk {

    /// <summary>
    /// Implementation of <see cref="IXunitTestCaseDiscoverer"/> that supports finding test cases
    /// using another discoverer.
    /// </summary>
    public abstract class DiscovererWrapper : IXunitTestCaseDiscoverer {
        private readonly IXunitTestCaseDiscoverer discoverer_;
        private readonly IMessageSink diagnosticMessageSink_;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscovererWrapper"/> class.
        /// </summary>
        /// <param name="discoverer">The discoverer used to discover test cases</param>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        protected DiscovererWrapper( IXunitTestCaseDiscoverer discoverer, IMessageSink diagnosticMessageSink ) {
            this.discoverer_ = discoverer;
            this.diagnosticMessageSink_ = diagnosticMessageSink;
        }

        /// <inheritdoc/>
        public IEnumerable<IXunitTestCase> Discover( ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute ) {
            TestMethodDisplay defaultMethodDisplay = discoveryOptions.MethodDisplayOrDefault( );
            bool useStaThread = factAttribute.GetNamedArgument<bool>( "UseStaThread" );
            int timeout = factAttribute.GetNamedArgument<int>( "Timeout" );

            var concreteMethods = GetConcreteMethods( testMethod );
            if( !concreteMethods.Any( ) )
                return new ExecutionErrorTestCase( this.diagnosticMessageSink_, defaultMethodDisplay, testMethod, "Could not find any arguments to satisfy the test method " + testMethod.Method.ToRuntimeMethod( ).GetDescriptiveName( ) )
                    .MakeEnumerable<IXunitTestCase>( );

            return
                from concreteMethod in concreteMethods
                from testCase in this.discoverer_.Discover( discoveryOptions, concreteMethod, factAttribute )
                select ConvertTestCase( testCase, defaultMethodDisplay, timeout, useStaThread );
        }

        /// <summary>
        /// Enumerates the concrete test methods for the specified source test method.
        /// </summary>
        /// <param name="testMethod">The method to enumerate.</param>
        /// <returns>All concrete version of <paramref name="testMethod"/> (the default implementation returns <paramref name="testMethod"/>).</returns>
        protected virtual ICollection<ITestMethod> GetConcreteMethods( ITestMethod testMethod ) {
            return new[] { testMethod };
        }

        /// <summary>
        /// Converts a <see cref="XunitTestCase"/> into an equivalent test case for the discoverer.
        /// </summary>
        /// <param name="testCase">The test case to convert.</param>
        /// <param name="defaultMethodDisplay">The default test method display.</param>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages.</param>
        /// <param name="timeout">The amount of time the test case is allowed to run in milliseconds, or a value less than 1 to indicate no timeout.</param>
        /// <param name="useStaThread">Whether to use an STA thread to execute tests.</param>
        /// <returns>The converted test case (the default implementation returns a <see cref="TimeoutXunitTestCase"/>).</returns>
        protected virtual XunitTestCase ConvertXunitTestCase( XunitTestCase testCase, TestMethodDisplay defaultMethodDisplay, IMessageSink diagnosticMessageSink, int timeout, bool useStaThread ) {
            return new TimeoutXunitTestCase( diagnosticMessageSink, defaultMethodDisplay, testCase.TestMethod, testCase.TestMethodArguments, timeout, useStaThread );
        }

        /// <summary>
        /// Converts a <see cref="XunitTheoryTestCase"/> into an equivalent theory test case for the discoverer.
        /// </summary>
        /// <param name="testCase">The test case to convert.</param>
        /// <param name="defaultMethodDisplay">The default test method display.</param>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages.</param>
        /// <param name="timeout">The amount of time the test case is allowed to run in milliseconds, or a value less than 1 to indicate no timeout.</param>
        /// <param name="useStaThread">Whether to use an STA thread to execute tests.</param>
        /// <returns>The converted test case (the default implementation returns a <see cref="TimeoutXunitTheoryTestCase"/>).</returns>
        protected virtual XunitTheoryTestCase ConvertTheoryTestCase( XunitTheoryTestCase testCase, TestMethodDisplay defaultMethodDisplay, IMessageSink diagnosticMessageSink, int timeout, bool useStaThread ) {
            return new TimeoutXunitTheoryTestCase( diagnosticMessageSink, defaultMethodDisplay, testCase.TestMethod, timeout, useStaThread );
        }

        private IXunitTestCase ConvertTestCase( IXunitTestCase testCase, TestMethodDisplay defaultMethodDisplay, int timeout, bool useStaThread ) {
            var theory = testCase as XunitTheoryTestCase;
            if( theory != null )
                return ConvertTheoryTestCase( theory, defaultMethodDisplay, this.diagnosticMessageSink_, timeout, useStaThread );

            if( testCase.GetType( ) == typeof( XunitTestCase ) )
                return ConvertXunitTestCase( (XunitTestCase)testCase, defaultMethodDisplay, this.diagnosticMessageSink_, timeout, useStaThread );

            return testCase;
        }
    }

}
