
using System;


namespace TypeResolver {

    public abstract partial class LinkList<T> {

        private sealed class EmptyList : LinkList<T> {

            public override int Count {
                get { return 0; }
            }

            public override T Value {
                get { throw new InvalidOperationException( "Empty list has no value." ); }
            }

            public override LinkList<T> Tail {
                get { throw new InvalidOperationException( "Empty list has not tail." ); }
            }
        }


    }

}
