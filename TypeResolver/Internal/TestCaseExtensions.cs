
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;


namespace TypeResolver.Internal {

    internal static class TestCaseExtensions {

        public static Task<T> RunAsync<T>( Func<T> operation, ApartmentState apartmentState = ApartmentState.Unknown ) {
            Thread thread;
            var tcs = RunAsync<T>( operation, apartmentState, out thread );
            return tcs.Task;
        }

        private static TaskCompletionSource<T> RunAsync<T>( Func<T> operation, ApartmentState apartmentState, out Thread thread ) {
            // Run on worker thread (https://github.com/Haacked/samples/blob/9de33a206e0e3fb654479580445642f4bcc0dc84/STAExamples/STATestCase.cs).
            var tcs = new TaskCompletionSource<T>( );
            thread = new Thread( ( ) => {
                try {
                    T result = operation( );
                    tcs.TrySetResult( result );
                }
                catch( Exception ex ) {
                    tcs.TrySetException( ex );
                }
            } ) { IsBackground = true };

            if( apartmentState != ApartmentState.Unknown )
                thread.SetApartmentState( apartmentState );

            thread.Start( );
            return tcs;
        }

        public static Task<RunSummary> TimeoutRunAsync( this ITimeoutTestCase testCase, IMessageBus messageBus, CancellationTokenSource cancellationTokenSource, Func<Task<RunSummary>> runTest ) {
            DateTime begin = DateTime.UtcNow;

            // Run test on STA thread (https://github.com/Haacked/samples/blob/9de33a206e0e3fb654479580445642f4bcc0dc84/STAExamples/STATestCase.cs).
            var apartmentState = testCase.UseStaThread ? ApartmentState.STA : ApartmentState.Unknown;
            Thread thread;
            var tcs = RunAsync( ( ) => {
                var testCaseTask = runTest( );
                return testCaseTask.Result;
            }, apartmentState, out thread );

            // If no timeout was specified, return task for test.
            if( testCase.Timeout < 1 )
                return tcs.Task;


            // Otherwise, wait for test to complete, or timeout to elapse.
            return Task.Run( ( ) => {
                var test = tcs.Task;
                Task delay = Task.Delay( testCase.Timeout, cancellationTokenSource.Token );
                if( Task.WaitAny( test, delay ) != 0 && !test.IsCompleted ) {
                    DateTime end = DateTime.UtcNow;
                    TimeSpan elapsed = end - begin;

                    var exception = new TimeoutException( testCase.DisplayName + " has exceeded the execution timeout of " + testCase.Timeout + "ms." );
                    var message = new TestFailed( new XunitTest( testCase, testCase.DisplayName ), (decimal)elapsed.TotalMilliseconds, null, exception );

                    if( !messageBus.QueueMessage( message ) )
                        cancellationTokenSource.Cancel( );

                    tcs.TrySetResult( new RunSummary { Total = 1, Failed = 1, Time = (decimal)elapsed.TotalMilliseconds } );
                    thread.Abort( );
                }

                return test.Result;
            }, cancellationTokenSource.Token );
        }


        public static void SerializeTimeoutTestCase( this IXunitSerializationInfo data, ITimeoutTestCase testCase ) {
            data.AddValue( "UseStaThread", testCase.UseStaThread );
            data.AddValue( "Timeout", testCase.Timeout );
        }

        public static void DeserializeTimeoutTestCase( this IXunitSerializationInfo data, out int timeout, out bool useStaThread ) {
            useStaThread = data.GetValue<bool>( "UseStaThread" );
            timeout = data.GetValue<int>( "Timeout" );
        }


        public static void SetGenericTypes<T>( this T testCase, ITypeInfo[] genericTypes )
            where T : TestMethodTestCase, IProtectedTestCase {
            var field = typeof( TestMethodTestCase )
                .GetFields( BindingFlags.NonPublic | BindingFlags.Instance )
                .Single( f => f.FieldType == typeof( ITypeInfo[] ) );

            Debug.Assert( field.GetValue( testCase ) == null, field.Name + " already initialized." );
            field.SetValue( testCase, genericTypes );
            Debug.Assert( testCase.MethodGenericTypes == genericTypes, "Setting " + field.Name + " did not initialize MethodGenericTypes." );
        }

        public static ITypeInfo[] EnsureGenericTypesInitialized<T>( this T testCase, ITypeInfo[] genericTypes = null )
            where T : TestMethodTestCase, IProtectedTestCase {
            if( genericTypes == null )
                genericTypes = testCase.TestMethod.Method.GetGenericArguments( ).ToArray( );
            Debug.Assert( genericTypes.Any( ) );

            if( testCase.MethodGenericTypes.IsNullOrEmpty( ) || testCase.Method.IsGenericMethodDefinition )
                testCase.SetGenericTypes( genericTypes );

            return testCase.MethodGenericTypes;
        }


        public static string GetUniqueIdWithGenericTypes<T>( this T testCase )
            where T : TestMethodTestCase, IProtectedTestCase {
            string uniqueId;

            if( !testCase.TestMethodArguments.IsNullOrEmpty( ) ) {
                uniqueId = testCase.GetUniqueID( );
            }
            else {
                try {
                    object[] genericTypes = ToSerializableGenericTypes( testCase.EnsureGenericTypesInitialized( ) );
                    testCase.SetTestMethodArguments( genericTypes );
                    uniqueId = testCase.GetUniqueID( );
                }
                finally {
                    testCase.SetTestMethodArguments( null );
                }
            }

            return uniqueId;
        }


        public static void SerializeGenericTypes<T>( this T testCase, IXunitSerializationInfo data )
            where T : TestMethodTestCase, IProtectedTestCase {
            string[] genericTypes = ToSerializableGenericTypes( testCase.EnsureGenericTypesInitialized( ) );
            data.AddValue( "MethodGenericTypes", genericTypes );
        }

        public static void DeserializeGenericTypes<T>( this T testCase, IXunitSerializationInfo data, out bool deserializing )
            where T : TestMethodTestCase, IProtectedTestCase {
            try {
                deserializing = true;

                // Deserialize types from data.
                var serializedTypes = data.GetValue<string[]>( "MethodGenericTypes" );
                ITypeInfo[] genericTypes = FromSerializableGenericTypes( serializedTypes );
                testCase.SetGenericTypes( genericTypes );

                // Resolve generic test method.
                if( genericTypes.Any( ) ) {
                    var testMethod = testCase.TestMethod;
                    testCase.SetMethod( testMethod.Method.MakeGenericMethod( genericTypes ) );
                    testCase.SetTestMethod( new TestMethod( testMethod.TestClass, testCase.Method ) );
                }

                // Update DisplayName.
                deserializing = false;
                testCase.FullyInitialize( );
            }
            finally {
                deserializing = false;
            }
        }


        private static string[] ToSerializableGenericTypes( ITypeInfo[] genericTypes ) {
            return Array.ConvertAll( genericTypes, SerializeType );
        }

        private static ITypeInfo[] FromSerializableGenericTypes( string[] genericTypes ) {
            return Array.ConvertAll( genericTypes, DeserializeType );
        }

        private static string SerializeType( ITypeInfo type ) {
            Type runtimeType = type.ToRuntimeType( );
            return runtimeType.AssemblyQualifiedName;
        }

        private static ITypeInfo DeserializeType( string typeName ) {
            Type type = Type.GetType( typeName );
            return Reflector.Wrap( type );
        }

    }

}
