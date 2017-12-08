
using System;
using System.Collections;
using System.Linq;
using Xunit;
using Xunit.Extensions;


namespace TypeResolver {

    public class TestTypeLoader {

        [Theory]
        [InlineData( null, false )]                     // null
        [InlineData( typeof( int ), true )]             // struct
        [InlineData( typeof( int? ), true )]            // struct
        [InlineData( typeof( Type ), false )]           // abstract
        [InlineData( typeof( DateTime ), true )]        // struct
        [InlineData( typeof( Exception ), true )]       // class
        [InlineData( typeof( IEnumerable ), false )]    // interface
        [InlineData( typeof( Environment ), false )]    // static, not factory
        [InlineData( typeof( FactoryClass ), true )]    // static factory
        public void IsUsableType_returns_expected_value( Type type, bool expectedIsUsable ) {
            bool isUsable = TypeLoader.IsUsableType( type );

            Assert.Equal( expectedIsUsable, isUsable );
        }


        [Fact]
        public void GetUsableTypes_only_returns_types_from_reference_type_assemblies( ) {
            Type referenceType = typeof( TestTypeLoader );
            var referenceAssembly = referenceType.Assembly;

            var usableTypes = TypeLoader.GetUsableTypes( referenceType );

            Assert.NotEmpty( usableTypes );
            foreach( var type in usableTypes )
                Assert.Equal( referenceAssembly, type.Assembly );
        }

        [Fact]
        public void GetUsableTypes_uses_assemblies_referenced_by_type_argument( ) {
            Type referenceType = typeof( GenericClass<> ).GetGenericArguments( ).Single( );
            var referenceAssembly = referenceType.Assembly;

            var usableTypes = TypeLoader.GetUsableTypes( referenceType );
            var inReferenceAssembly = usableTypes.Where( t => t.Assembly == referenceAssembly );
            var inOtherAssembly = usableTypes.Where( t => t.Assembly != referenceAssembly );

            Assert.NotEmpty( inReferenceAssembly );
            Assert.NotEmpty( inOtherAssembly );
        }


        private static class FactoryClass { }

        private sealed class GenericClass<T> where T : IEnumerable { public IEnumerator GetEnumerator( ) { yield break; } }

    }

}
