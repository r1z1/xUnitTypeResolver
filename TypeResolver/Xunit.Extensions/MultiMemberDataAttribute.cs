
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TypeResolver;
using Xunit.Sdk;


namespace Xunit.Extensions {

    /// <summary>
    /// Generates method data combining parameters from multiple property data sources.
    /// </summary>
    public class MultiMemberDataAttribute : DataAttribute {

        private readonly string[] propertyNames_;

        /// <summary>
        /// Creates a new instance of <see cref="MultiMemberDataAttribute"/>.
        /// </summary>
        /// <param name="propertyNames">The name of the public static properties on the test class that will provide the test data.</param>
        public MultiMemberDataAttribute( params string[] propertyNames ) {
            this.propertyNames_ = propertyNames;
        }

        /// <summary>
        /// Returns all permutations of the static property data sources.
        /// </summary>
        public override IEnumerable<object[]> GetData( MethodInfo methodUnderTest ) {
            //TODO: Only works for immutable arguments.
            // Get arguments from each data source, using MemberDataAttribute.
            object[][][] dataSources = propertyNames_.Select(
                propertyName => new MemberDataAttribute( propertyName )
                    .GetData( methodUnderTest )
                    .ToArray( )
            ).ToArray( );

            // Permute indices into data sources.
            int parameterCount = methodUnderTest.GetParameters( ).Length;
            var dataSourceIndices = dataSources.Select( dataSource => Enumerable.Range( 0, dataSource.Length ) );
            foreach( int[] indices in Permuter.Permute( dataSourceIndices ) ) {
                object[] buffer = new object[parameterCount];
                int bufferIndex = 0;

                // Build arguments buffer from data source at each index.
                for( int i = 0; i < indices.Length; ++i ) {
                    int index = indices[i];
                    object[] dataSource = dataSources[i][index];
                    foreach( object data in dataSource )
                        buffer[bufferIndex++] = data;
                }

                yield return buffer;
            }
        }

    }

}
