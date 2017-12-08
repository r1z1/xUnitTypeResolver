
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;


namespace Xunit.Extensions {

    public class TestInstanceDataAttribute {

        private static Type[] GetParameterTypes( MethodInfo method ) {
            Assert.NotNull( method );

            return method.GetParameters( ).Select( ( p ) => p.ParameterType ).ToArray( );
        }


        [Fact]
        public void InstanceDataAttribute_succeeds_for_parameterless_method( ) {
            var method = ParameterlessMethod( );

            var attribute = new InstanceDataAttribute( );
            var data = attribute.GetData( method );

            var item = Assert.Single( data );
            Assert.Empty( item );
        }

        [Fact]
        public void InstanceDataAttribute_succeeds_for_parameterized_method( ) {
            var method = ParameterizedMethod( 0 );
            var parameterTypes = GetParameterTypes( method );

            var attribute = new InstanceDataAttribute( );
            var data = attribute.GetData( method );

            var item = Assert.Single( data );
            var argument = Assert.Single( item );
            Assert.Equal( parameterTypes[0], argument.GetType( ) );
        }


        [Theory]
        [InlineData( ApartmentState.STA )]
        [InlineData( ApartmentState.MTA )]
        public void InstanceDataAttribute_succeeds_for_method_requiring_STA_thread( ApartmentState apartmentState ) {
            var method = StaTestMethod( null );
            var parameterTypes = GetParameterTypes( method );

            object[][] data = null;
            var thread = new Thread( ( ) => {
                var attribute = new InstanceDataAttribute( );
                data = attribute.GetData( method ).ToArray( );
            } ) { IsBackground = true };
            thread.SetApartmentState( apartmentState );
            thread.Start( );
            thread.Join( );

            var item = Assert.Single( data );
            var argument = Assert.Single( item );
            Assert.Equal( parameterTypes[0], argument.GetType( ) );
        }


        private MethodInfo ParameterlessMethod( ) {
            return MethodBase.GetCurrentMethod( ) as MethodInfo;
        }

        private MethodInfo ParameterizedMethod( int parameter ) {
            return MethodBase.GetCurrentMethod( ) as MethodInfo;
        }


        [StaTheory]
        [InlineData( default( StaTestData ) )]
        private MethodInfo StaTestMethod( StaTestData parameter ) {
            return MethodBase.GetCurrentMethod( ) as MethodInfo;
        }

        public sealed class StaTestData {

            public StaTestData( ) {
                ApartmentState actual = Thread.CurrentThread.GetApartmentState( );
                Assert.Equal( ApartmentState.STA, actual );
            }

        }

    }

}
