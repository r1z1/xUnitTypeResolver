
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TypeResolver.Extensions;


namespace TypeResolver {

    /// <summary>
    /// Represents a binding of a generc type argument to a concrete type.
    /// </summary>
    public sealed class Binding : IEquatable<Binding> {

        /// <summary>
        /// An empty list of bindngs.
        /// </summary>
        public static readonly LinkList<Binding> EmptyBindings = LinkList<Binding>.Empty;

        /// <summary>
        /// Returns the first binding found in the collection of bindings using the specified <paramref name="argument"/>;
        /// otherwise, <see langword="null"/>.
        /// </summary>
        [DebuggerHidden]
        public static Binding ForArgument( IEnumerable<Binding> bindings, Type argument ) {
            Debug.Assert( bindings != null );
            Debug.Assert( argument != null );

            return bindings.FirstOrDefault( ( b ) => argument == b.Argument );
        }

        /// <summary>
        /// Returns <see langword="true"/> if the <paramref name="argument"/> is included in the collection of bindings;
        /// otherwise, <see langword="false"/>.
        /// </summary>
        [DebuggerHidden]
        public static bool ContainsArgument( IEnumerable<Binding> bindings, Type argument ) {
            Debug.Assert( bindings != null );
            Debug.Assert( argument != null );

            return bindings.Any( ( b ) => argument == b.Argument );
        }

        /// <summary>
        /// Returns <see langword="true"/> if the <paramref name="type"/> is included in the collection of bindings;
        /// otherwise, <see langword="false"/>.
        /// </summary>
        [DebuggerHidden]
        public static bool ContainsType( IEnumerable<Binding> bindings, Type type ) {
            Debug.Assert( bindings != null );
            Debug.Assert( type != null );

            return bindings.Any( ( b ) => type == b.Type );
        }


        /// <summary>
        /// The bound generic type argument, or <see langword="null"/> if the binding indicates the exclusion of the <see cref="Binding.Type"/>.
        /// </summary>
        public readonly Type Argument;

        /// <summary>
        /// The concrete <see cref="Type"/> bound to the <see cref="Binding.Argument"/>.
        /// </summary>
        public readonly Type Type;


        /// <summary>
        /// Creates a new <see cref="Binding"/> instance.
        /// </summary>
        public Binding( Type argument, Type type ) {
            Debug.Assert( type != null );
            if( argument != null ) {
                Debug.Assert( argument.IsGenericParameter );
                Debug.Assert( !type.ContainsGenericParameters );
                Debug.Assert( type.Is( argument ) );
            }

            this.Argument = argument;
            this.Type = type;
        }


        /// <summary>
        /// Returns a <see cref="String"/> representation of the object.
        /// </summary>
        public override string ToString( ) {
            string argumentName = (this.Argument != null
                ? this.Argument.GetDescriptiveName( )
                : "<null>");
            string typeName = this.Type.GetDescriptiveName( );
            return argumentName + " = " + typeName;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Binding"/> is equal to the current <see cref="Binding"/>.
        /// </summary>
        public bool Equals( Binding binding ) {
            return binding != null
                && object.Equals( this.Argument, binding.Argument )
                && object.Equals( this.Type, binding.Type );
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current <see cref="Object"/>B.
        /// </summary>
        public override bool Equals( object obj ) {
            return this.Equals( obj as Binding );
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        public override int GetHashCode( ) {
            int argumentHashCode = (this.Argument ?? typeof( object )).GetHashCode( );
            int typeHashCode = this.Type.GetHashCode( );
            return argumentHashCode
                 ^ typeHashCode;
        }

    }

}
