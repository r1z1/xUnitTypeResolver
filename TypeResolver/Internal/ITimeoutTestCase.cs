
using Xunit.Sdk;


namespace TypeResolver.Internal {

    internal interface ITimeoutTestCase : IXunitTestCase {

        bool UseStaThread { get; }
        int Timeout { get; }

    }

}
