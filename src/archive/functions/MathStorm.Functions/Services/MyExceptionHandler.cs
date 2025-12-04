namespace MathStorm.Functions.Services;

public class MyExceptionHandler : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var logger = context.GetLogger(context.FunctionDefinition.Name);
            logger.LogError($"Unexpected Error In {context.FunctionDefinition.Name}: {ExceptionHelper.GetExceptionMessage(ex)}");
        }
    }
}

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