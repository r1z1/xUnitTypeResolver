
using Xunit.Abstractions;


namespace Xunit.Sdk {

    /// <summary>
    /// Implementation of <see cref="IXunitTestCaseDiscoverer"/> that supports finding test cases
    /// on methods decorated with <see cref="Xunit.Extensions.TimeoutTheoryAttribute"/> or <see cref="Xunit.Extensions.StaTheoryAttribute"/>.
    /// </summary>
    public class TimeoutTheoryDiscoverer : DiscovererWrapper {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeoutTheoryDiscoverer"/> class.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        public TimeoutTheoryDiscoverer( IMessageSink diagnosticMessageSink )
            : base( new TheoryDiscoverer( diagnosticMessageSink ), diagnosticMessageSink ) { }
    }

}
