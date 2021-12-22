using System.Linq;
using Next.Cqrs.Commands;

namespace Next.Core
{
    public static class NotificationExtensions
    {
        public static bool TryGetErrorResponse<TCommandResponse>(this Notification notification, out TCommandResponse response)
            where TCommandResponse : CommandResponse, new()
        {
            response = default;

            if (!notification.HasErrors)
            {
                return false;
            }

            var error = notification.Errors.Single();
            response = CommandResponse.Fail<TCommandResponse>(error);
            return true;
        }
        
        public static bool TryGetErrorResponse(this Notification notification, out CommandResponse commandResponse)
        {
            return notification.TryGetErrorResponse<CommandResponse>(out commandResponse);
        }
        
        public static CommandResponse ToErrorResponse(this Notification notification)
        {
            var error = notification.Errors.Single();
            return CommandResponse.Fail(error);
        }
        
        public static CommandResponse ToErrorResponse<TCommandResponse>(this Notification notification)
            where TCommandResponse : CommandResponse, new()
        {
            var error = notification.Errors.Single();
            return CommandResponse.Fail<TCommandResponse>(error);
        }
    }
}
