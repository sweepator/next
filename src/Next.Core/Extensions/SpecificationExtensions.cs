using System;
using Next.Core.Errors;

namespace Next.Core.Specifications
{
    public static class SpecificationExtensions
    {
        public static bool TryGetNotificationErrorIfNotSatisfied<T>(
            this ISpecification<T> specification,
            T obj,
            Error error,
            out Notification notification)
        {
            notification = Notification.Sucess;
            
            if (specification == null)
            {
                throw new ArgumentNullException(nameof(specification));
            }
            
            if (error == null)
            {
                throw new ArgumentNullException(nameof(error));
            }

            if (!specification.IsSatisfiedBy(obj))
            {
                notification = Notification.Create(error);
            }

            return true;
        }
    }
}