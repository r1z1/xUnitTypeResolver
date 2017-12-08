// #define TEST_TIMEOUT
#if !DEBUG && TEST_TIMEOUT
Only use TEST_TIMEOUT to run failing tests in Debug.
#endif

using System.Diagnostics;
using System.Threading;


namespace Xunit.Extensions {

    public class TestTimeoutAttributes {

        private const int BaseTimeout = 500;


        [TimeoutFact]
        public void TimeoutFact_runs_test_on_default_thread_apartment( ) {
            ApartmentState actual = Thread.CurrentThread.GetApartmentState( );

            Assert.NotEqual( ApartmentState.STA, actual );
        }

        [TimeoutFact( Timeout = BaseTimeout )]
        public void TimeoutFact_passes_test_that_runs_within_timeout( ) {
            var stopwatch = Stopwatch.StartNew( );

            Thread.Sleep( BaseTimeout / 2 );

            Assert.True( true, string.Format( "Test ran within {0}ms timeout: {1}ms", BaseTimeout, stopwatch.ElapsedMilliseconds ) );
        }

#if TEST_TIMEOUT
        [TimeoutFact( Timeout = BaseTimeout )]
#endif
        public void TimeoutFact_fails_test_that_runs_longer_than_timeout( ) {
            var stopwatch = Stopwatch.StartNew( );

            Thread.Sleep( BaseTimeout * 2 );

            Assert.True( false, string.Format( "Test exceeded {0}ms timeout: {1}ms", BaseTimeout, stopwatch.ElapsedMilliseconds ) );
        }


        [TimeoutTheory]
        [InlineData( ApartmentState.STA )]
        public void TimeoutTheory_runs_test_on_default_thread_apartment( ApartmentState expected ) {
            ApartmentState actual = Thread.CurrentThread.GetApartmentState( );

            Assert.NotEqual( expected, actual );
        }

        [TimeoutTheory( Timeout = BaseTimeout )]
        [InlineData( BaseTimeout / 2, true )]
#if TEST_TIMEOUT
        [InlineData( BaseTimeout * 2, false )]
#endif
        public void TimeoutTheory_passes_test_when_run_within_timeout( int timeout, bool expected ) {
            var stopwatch = Stopwatch.StartNew( );

            Thread.Sleep( timeout );

            Assert.True( expected, "Test ran for " + stopwatch.ElapsedMilliseconds + "ms" );
        }


        [StaFact]
        public void StaFact_runs_test_on_STA_thread( ) {
            ApartmentState actual = Thread.CurrentThread.GetApartmentState( );

            Assert.Equal( ApartmentState.STA, actual );
        }

        [StaFact( Timeout = BaseTimeout )]
        public void StaFact_passes_test_that_runs_within_timeout( ) {
            var stopwatch = Stopwatch.StartNew( );

            Thread.Sleep( BaseTimeout / 2 );

            Assert.True( true, string.Format( "Test ran within {0}ms timeout: {1}ms", BaseTimeout, stopwatch.ElapsedMilliseconds ) );
        }

#if TEST_TIMEOUT
        [StaFact( Timeout = BaseTimeout )]
#endif
        public void StaFact_fails_test_that_runs_longer_than_timeout( ) {
            var stopwatch = Stopwatch.StartNew( );

            Thread.Sleep( BaseTimeout * 2 );

            Assert.True( false, string.Format( "Test exceeded {0}ms timeout: {1}ms", BaseTimeout, stopwatch.ElapsedMilliseconds ) );
        }


        [StaTheory]
        [InlineData( ApartmentState.STA )]
        public void StaTheory_runs_test_on_STA_thread( ApartmentState expected ) {
            ApartmentState actual = Thread.CurrentThread.GetApartmentState( );

            Assert.Equal( expected, actual );
        }

        [StaTheory( Timeout = BaseTimeout )]
        [InlineData( BaseTimeout / 2, true )]
#if TEST_TIMEOUT
        [InlineData( BaseTimeout * 2, false )]
#endif
        public void StaTheory_passes_test_when_run_within_timeout( int timeout, bool expected ) {
            var stopwatch = Stopwatch.StartNew( );

            Thread.Sleep( timeout );

            Assert.True( expected, "Test ran for " + stopwatch.ElapsedMilliseconds + "ms" );
        }

    }

}
