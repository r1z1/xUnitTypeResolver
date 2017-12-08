
using Xunit.Sdk;


namespace Xunit.Extensions {

    /// <summary>
    /// Marks a test method as being a data theory. Data theories are tests which
    ///  are fed various bits of data from a data source, mapping to parameters on
    ///  the test method.  If the data source contains multiple rows, then the test
    ///  method is executed multiple times (once with each data row).
    /// </summary>
    [XunitTestCaseDiscoverer( "Xunit.Sdk.GenericTheoryDiscoverer", "TypeResolver" )]
    public class GenericTheoryAttribute : TimeoutTheoryAttribute {

        /// <summary>Gets or sets whether to use an STA thread to execute tests.</summary>
        public new bool UseStaThread { get; set; }

    }

}
