
using System;
using System.ComponentModel;
using TypeResolver.Internal;
using Xunit.Abstractions;


namespace Xunit.Sdk {

    /// <summary>
    /// Implementation of <see cref="XunitTheoryTestCase"/> that resolves generic test methods.
    /// </summary>
    [Serializable]
    public class GenericXunitTheoryTestCase : TimeoutXunitTheoryTestCase, IProtectedTestCase {
        private bool deserializing_;

        /// <summary>Initializes a new instance of the <see cref="GenericXunitTheoryTestCase"/> class when deserializing.</summary>
        [EditorBrowsable( EditorBrowsableState.Never )]
        [Obsolete( "Called by the de-serializer", true )]
        public GenericXunitTheoryTestCase( ) { }

        /// <inheritdoc cref="XunitTheoryTestCase(IMessageSink,TestMethodDisplay,ITestMethod)"/>
        public GenericXunitTheoryTestCase( IMessageSink diagnosticMessageSink, TestMethodDisplay testMethodDisplay, ITestMethod testMethod, int timeout, bool useStaThread )
            : base( diagnosticMessageSink, testMethodDisplay, testMethod, timeout, useStaThread ) {
        }

        /// <inheritdoc/>
        protected override void Initialize( ) {
            if( !this.deserializing_ ) {
                this.EnsureGenericTypesInitialized( );
                base.Initialize( );
            }
        }

        /// <inheritdoc/>
        protected override string GetUniqueID( ) {
            return this.GetUniqueIdWithGenericTypes( );
        }

        /// <inheritdoc/>
        public override void Serialize( IXunitSerializationInfo data ) {
            base.Serialize( data );

            this.SerializeGenericTypes( data );
        }

        /// <inheritdoc/>
        public override void Deserialize( IXunitSerializationInfo data ) {
            base.Deserialize( data );

            this.DeserializeGenericTypes( data, out this.deserializing_ );
        }

#if TEST_DISPLAY_NAME
        /// <inheritdoc/>
        public override Task<RunSummary> RunAsync( IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource ) {
            diagnosticMessageSink.OnMessage( new DiagnosticMessage( "[{0}] Running {1}", this.GetType( ).GetDescriptiveName( ), this.DisplayName ) );

            return base.RunAsync( diagnosticMessageSink, messageBus, constructorArguments, aggregator, cancellationTokenSource );
        }
#endif

        #region IProtectedTestCase Members

        ITypeInfo[] IProtectedTestCase.MethodGenericTypes {
            get { return base.MethodGenericTypes; }
        }

        string IProtectedTestCase.GetUniqueID( ) {
            return base.GetUniqueID( );
        }

        void IProtectedTestCase.FullyInitialize( ) {
            base.EnsureInitialized( );
            base.Initialize( );
        }

        void IProtectedTestCase.SetMethod( IMethodInfo method ) {
            base.Method = method;
        }

        void IProtectedTestCase.SetTestMethod( ITestMethod testMethod ) {
            base.TestMethod = testMethod;
        }

        void IProtectedTestCase.SetTestMethodArguments( object[] testMethodArguments ) {
            base.TestMethodArguments = testMethodArguments;
        }

        #endregion
    }

}
