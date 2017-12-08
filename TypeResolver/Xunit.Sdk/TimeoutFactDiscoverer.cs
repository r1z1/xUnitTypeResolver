
using Xunit.Abstractions;


namespace Xunit.Sdk {

    /// <summary>
    /// Implementation of <see cref="IXunitTestCaseDiscoverer"/> that supports finding test cases
    /// on methods decorated with <see cref="Xunit.Extensions.TimeoutFactAttribute"/> or <see cref="Xunit.Extensions.StaFactAttribute"/>.
    /// </summary>
    public class TimeoutFactDiscoverer : DiscovererWrapper {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeoutFactDiscoverer"/> class.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        public TimeoutFactDiscoverer( IMessageSink diagnosticMessageSink )
            : base( new FactDiscoverer( diagnosticMessageSink ), diagnosticMessageSink ) { }
    }

}
