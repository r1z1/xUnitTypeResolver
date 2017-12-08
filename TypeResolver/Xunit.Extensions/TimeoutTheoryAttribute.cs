
using System.ComponentModel;
using Xunit.Sdk;


namespace Xunit.Extensions {

    /// <summary>
    /// Marks a test method as being a data theory. Data theories are tests which
    ///  are fed various bits of data from a data source, mapping to parameters on
    ///  the test method.  If the data source contains multiple rows, then the test
    ///  method is executed multiple times (once with each data row).
    /// </summary>
    [XunitTestCaseDiscoverer( "Xunit.Sdk.TimeoutTheoryDiscoverer", "TypeResolver" )]
    public class TimeoutTheoryAttribute : TheoryAttribute {

        /// <summary>Gets or sets the amount of time the test case is allowed to run in milliseconds.</summary>
        /// <remarks>Any value less than 1 indicates no timeout.</remarks>
        public int Timeout { get; set; }

        /// <summary>Gets whether to use an STA thread to execute tests.</summary>
        [EditorBrowsable( EditorBrowsableState.Never )]
        public bool UseStaThread { get { return false; } }

    }

}
