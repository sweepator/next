using System.Threading;

namespace Next.Application.Pipelines
{
    internal sealed class OperationContextAccessor : IOperationContextAccessor
    {
        private static readonly AsyncLocal<OperationContextHolder> OperationContextCurrent = new();

        private OperationContextAccessor()
        {
        }

        public static OperationContextAccessor Instance { get; } = new ();

        public IOperationContext Context
        {
            get => OperationContextCurrent.Value?.Context;
            set
            {
                var holder = OperationContextCurrent.Value;
                if (holder != null)
                {
                    // Clear current OperationContext trapped in the AsyncLocals, as its done.
                    holder.Context = null;
                }

                if (value != null)
                {
                    // Use an object indirection to hold the OperationContext in the AsyncLocal,
                    // so it can be cleared in all ExecutionContexts when its cleared.
                    OperationContextCurrent.Value = new OperationContextHolder { Context = value };
                }
            }
        }

        private class OperationContextHolder
        {
            public IOperationContext Context;
        }
    }
}
