using System.Collections.Generic;
using System.Linq;
using Next.Core.Errors;

namespace Next.Core
{
    public class Notification
    {
        public static readonly Notification Sucess = new Notification();

        private readonly List<Error> _list = new List<Error>();
        public IEnumerable<Error> Errors => _list;

        public bool HasErrors => Errors.Any();

        public void Add(Error error)
        {
            _list.Add(error);
        }

        public static Notification<TResult> Create<TResult>(
            TResult result,
            IEnumerable<Error> errors)
        {
            return new Notification<TResult>(result, errors);
        }

        public static Notification<TResult> Create<TResult>(
            IEnumerable<Error> errors)
        {
            return Create<TResult>(default, errors);
        }

        public static Notification<TResult> Create<TResult>(params Error[] errors)
        {
            return Create<TResult>(default, errors);
        }

        public static Notification Create(params Error[] errors)
        {
            var notification = new Notification();
            errors.ToList().ForEach(e => notification.Add(e));
            return notification;
        }
    }

    public class Notification<T> : Notification
    {
        public T Result { get; }

        public Notification(T result)
        {
            Result = result;
        }

        internal Notification(
            T result, 
            IEnumerable<Error> errors)
        {
            Result = result;
            errors?.ToList().ForEach(e => Add(e));
        }
    }
}
