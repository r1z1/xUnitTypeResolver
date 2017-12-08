
using System.Diagnostics;


namespace TypeResolver {

    public abstract partial class LinkList<T> {

        private sealed class ValueList : LinkList<T> {

            private readonly int count_;
            private readonly T value_;
            private readonly LinkList<T> tail_;


            [DebuggerHidden]
            public ValueList( T value, LinkList<T> tail ) {
                Debug.Assert( tail != null );

                this.value_ = value;
                this.tail_ = tail;

                this.count_ = this.tail_.Count + 1;
            }


            public override int Count {
                get { return this.count_; }
            }

            public override T Value {
                get { return this.value_; }
            }

            public override LinkList<T> Tail {
                get { return this.tail_; }
            }

        }

    }

}
