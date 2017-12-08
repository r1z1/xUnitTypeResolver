
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TypeResolver.Internal;


namespace TypeResolver {

    public abstract partial class Constraint {

        private sealed class SimpleConstraint : Constraint {
            private readonly Func<Type, bool> condition_;
            private readonly string descrption_;

            public SimpleConstraint( Func<Type, bool> condition, string description )
                : base( false ) {
                Debug.Assert( condition != null );
                Debug.Assert( !string.IsNullOrEmpty( description ) );

                this.condition_ = condition;
                this.descrption_ = description;
            }

            protected override IEnumerable<LinkList<Binding>> SatisfyCore( Type availableType, LinkList<Binding> bindings ) {
                if( this.condition_( availableType ) )
                    return bindings.MakeEnumerable( );
                return Constraint.NoBindings;
            }

            protected override string ToStringCore( ) {
                return this.descrption_;
            }
        }

    }

}
