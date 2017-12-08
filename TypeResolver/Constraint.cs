
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using TypeResolver.Internal;


namespace TypeResolver {

    /// <summary>
    /// Represents a constraint on a generic type argument.
    /// </summary>
    public abstract partial class Constraint {

        private static readonly IEnumerable<LinkList<Binding>> NoBindings = new LinkList<Binding>[0];

        private static readonly Constraint ReferenceTypeConstraint = new SimpleConstraint( ( t ) => t.IsClass, "Is reference type" );
        private static readonly Constraint NonNullableValueTypeConstraint = new SimpleConstraint( IsNonNullableValueType, "Is non-nullable value type" );
        private static readonly Constraint DefaultConstructorConstraint = new SimpleConstraint( HasDefaultConstructor, "Has default constructor" );

        private readonly bool isUsageConstraint_;


        /// <summary>
        /// Returns <see langword="true"/> if the constraint is for the usage of a generic argument in another argument
        /// (as opposed to a direct constraint on the argument itself);
        /// otherwise, <see langword="false"/>.
        /// </summary>
        public bool IsUsageConstraint {
            get { return this.isUsageConstraint_; }
        }


        private Constraint( bool isUsageConstraint ) {
            this.isUsageConstraint_ = isUsageConstraint;
        }


        /// <summary>
        /// Returns <see langword="true"/> if the type has a public default constructor;
        /// otherwise, <see langword="false"/>.
        /// </summary>
        public static bool HasDefaultConstructor( Type type ) {
            if( type == null )
                return false;

            if( type.IsValueType )
                return true;

            var defaultConstructor = type.GetConstructor( Type.EmptyTypes );
            return defaultConstructor != null;
        }

        /// <summary>
        /// Returns <see langword="true"/> if the type is a non-nullable value type;
        /// otherwise, <see langword="false"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNonNullableValueType( Type type ) {
            if( type == null || !type.IsValueType )
                return false;

            if( !type.IsGenericType )
                return true;

            return type.GetGenericTypeDefinition( ) != typeof( Nullable<> );
        }


        /// <summary>
        /// Retrieves a constraint requiring inheritance from the specified <paramref name="baseType"/>.
        /// </summary>
        public static Constraint GetInheritanceConstraint( Type baseType ) {
            return Constraint.GetInheritanceConstraint( baseType, false );
        }

        private static Constraint GetInheritanceConstraint( Type baseType, bool isUsageConstraint ) {
            Debug.Assert( baseType != null );

            Type constraintType = baseType.IsGenericParameter ? typeof( void ) : baseType;
            return new RequiredBaseTypeConstraint( constraintType, isUsageConstraint );
        }

        /// <summary>
        /// Retrieves all of the constraints associated with a generic type.
        /// </summary>
        public static LinkList<Constraint> GetConstraints( Type argument, out Assembly[] referencedAssemblies ) {
            Debug.Assert( argument != null );
            Debug.Assert( argument.IsGenericParameter );

            var constraints = new List<Constraint>( );
            var references = new HashSet<Assembly>( );


            // Add derivation constraints (i.e. "T : Class, Interface, Generic<T>, ...").
            foreach( Type baseType in argument.GetGenericParameterConstraints( ) ) {
                var baseTypeConstraint = Constraint.GetInheritanceConstraint( baseType, false );
                constraints.Add( baseTypeConstraint );
                references.Add( baseType.Assembly );
            }


            // Add argument usage constraints (i.e. "U : IEnumerable<T>"; "Method<T>( Argument<T> arg )"; etc)
            //  if there are no derivation constraints.
            if( constraints.Count == 0 ) {
                var initialPotentials =
                    argument.DeclaringMethod != null
                        ? argument.DeclaringMethod
                            .GetParameters( )
                            .Select( ( p ) => p.ParameterType )
                            .Concat( argument.DeclaringMethod.GetGenericArguments( ) )
                        : argument.DeclaringType.GetGenericArguments( );
                var potentialUsageTypes = new VisitorQueue<Type>( initialPotentials );

                foreach( Type usageType in potentialUsageTypes ) {
                    Type[] usageTypeArguments = usageType.GetGenericArguments( );
                    potentialUsageTypes.AddRange( usageTypeArguments );

                    if( usageTypeArguments.Contains( argument ) ) {
                        constraints.Add( Constraint.GetInheritanceConstraint( usageType, true ) );
                        references.Add( usageType.Assembly );
                    }
                }
            }


            // Add keyword constraints (i.e. "T : class, struct, new( )").
            var attributes = argument.GenericParameterAttributes;
            if( (attributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0 )
                constraints.Add( ReferenceTypeConstraint );
            if( (attributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0 )
                constraints.Add( NonNullableValueTypeConstraint );
            if( (attributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0 )
                constraints.Add( DefaultConstructorConstraint );


            referencedAssemblies = new Assembly[references.Count];
            references.CopyTo( referencedAssemblies );
            return constraints.ToLinkList( );
        }

        /// <summary>
        /// Retrieves all bindings satisfying the constraints for the specified <see cref="Type"/>.
        /// </summary>
        public static void SatisfyConstraints( BindingCollection bindings, Type type, LinkList<Constraint> constraints ) {
            Debug.Assert( bindings != null );
            Debug.Assert( type != null );
            Debug.Assert( constraints != null );

            bindings.Reduce( constraints, ( b, c ) => c.SatisfyCore( type, b ) );
        }


        /// <summary>
        /// Returns a list of <see cref="Binding"/>s that satisfy the given constraint.
        /// </summary>
        [DebuggerHidden]
        public IEnumerable<LinkList<Binding>> Satisfy( Type availableType, LinkList<Binding> bindings ) {
            Debug.Assert( availableType != null );
            Debug.Assert( bindings != null );

            return this.SatisfyCore( availableType, bindings );
        }

        /// <summary>
        /// Returns a <see cref="String"/> representation of the object.
        /// </summary>
        public sealed override string ToString( ) {
            return this.ToStringCore( );
        }


        /// <summary>
        /// Implements the logic behind <see cref="Constraint.Satisfy"/>.
        /// </summary>
        protected abstract IEnumerable<LinkList<Binding>> SatisfyCore( Type availableType, LinkList<Binding> bindings );

        /// <summary>
        /// Implements the logic behind <see cref="Constraint.ToString"/>.
        /// </summary>
        protected abstract string ToStringCore( );

    }

}
