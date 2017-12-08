
using System.ComponentModel;
using Xunit.Sdk;


namespace Xunit.Extensions {

    /// <summary>
    /// Marks a test method as being a data fact. Data theories are tests which
    ///  are fed various bits of data from a data source, mapping to parameters on
    ///  the test method.  If the data source contains multiple rows, then the test
    ///  method is executed multiple times (once with each data row).
    /// </summary>
    [XunitTestCaseDiscoverer( "Xunit.Sdk.TimeoutFactDiscoverer", "TypeResolver" )]
    public class StaFactAttribute : TimeoutFactAttribute {

        /// <summary>Gets whether to use an STA thread to execute tests.</summary>
        [EditorBrowsable( EditorBrowsableState.Never )]
        public new bool UseStaThread { get { return true; } }

    }

}
