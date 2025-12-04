namespace MathStorm.Core.Helpers;

public static class ExceptionHelper
{
    public static string GetExceptionMessage(Exception ex)
    {
        var message = string.Empty;
        if (ex == null)
        {
            return message;
        }

        if (ex.Message != null)
        {
            message += ex.Message;
        }

        if (ex.InnerException == null)
        {
            return message;
        }

        if (ex.InnerException.Message != null)
        {
            message += " " + ex.InnerException.Message;
        }

        if (ex.InnerException.InnerException == null)
        {
            return message;
        }

        if (ex.InnerException.InnerException.Message != null)
        {
            message += " " + ex.InnerException.InnerException.Message;
        }

        if (ex.InnerException.InnerException.InnerException == null)
        {
            return message;
        }

        if (ex.InnerException.InnerException.InnerException.Message != null)
        {
            message += " " + ex.InnerException.InnerException.InnerException.Message;
        }

        return message;
    }
}
