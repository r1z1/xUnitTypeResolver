
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using TypeResolver;
using TypeResolver.Internal;
using Xunit.Sdk;


namespace Xunit.Extensions {

    /// <summary>
    /// Generates method data by instantiating all creatable types.
    /// </summary>
    public class InstanceDataAttribute : DataAttribute {

        /// <summary>
        /// Finds all creatable instances for each of the parameter types on the <paramref name="methodUnderTest"/>.
        /// </summary>
        public override IEnumerable<object[]> GetData( MethodInfo methodUnderTest ) {
            var parameters = methodUnderTest.GetParameters( );
            var parameterCreators = parameters.Select( ( p ) => TypeCreator.GetCreators( p.ParameterType ).AsEnumerable( ) );

            bool needsSta =
                   Thread.CurrentThread.GetApartmentState( ) != ApartmentState.STA
                && methodUnderTest.GetCustomAttributes<FactAttribute>( ).Any( IsStaTestAttribute );

            IEnumerable<object[]> instanceData;
            if( !needsSta ) {
                instanceData = GetDataCore( parameterCreators );
            }
            else {
                var task = TestCaseExtensions.RunAsync( ( ) => GetDataCore( parameterCreators ).ToArray( ), ApartmentState.STA );
                instanceData = task.Result;
            }

            return instanceData;
        }


        private static IEnumerable<object[]> GetDataCore( IEnumerable<IEnumerable<IInstanceCreator>> parameterCreators ) {
            foreach( var permutation in Permuter.Permute( parameterCreators ) ) {
                object[] data = new object[permutation.Length];
                for( int i = 0; i < data.Length; ++i )
                    data[i] = permutation[i].CreateInstance( );

                yield return data;
            }
        }

        private static bool IsStaTestAttribute( FactAttribute testAttribute ) {
            var genericTestAttribute = testAttribute as GenericTheoryAttribute;
            if( genericTestAttribute != null )
                return genericTestAttribute.UseStaThread;

            return testAttribute is StaFactAttribute
                || testAttribute is StaTheoryAttribute;
        }

    }

}
