
using Xunit.Abstractions;


namespace TypeResolver.Internal {

    internal interface IProtectedTestCase {

        ITypeInfo[] MethodGenericTypes { get; }

        string GetUniqueID( );
        void FullyInitialize( );
        void SetMethod( IMethodInfo method );
        void SetTestMethod( ITestMethod testMethod );
        void SetTestMethodArguments( object[] testMethodArguments );

    }

}
