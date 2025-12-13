namespace Med.Labs.Api.Middleware
{
	using System.Net;
	using System.Text.Json;

	public class ErrorHandlingMiddleware
	{
		private readonly RequestDelegate _next;

		public ErrorHandlingMiddleware(RequestDelegate next)
			=> _next = next;

		public async Task Invoke(HttpContext ctx)
		{
			try
			{
				await _next(ctx);
			}
			catch (Exception ex)
			{
				ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;

				var payload = JsonSerializer.Serialize(new
				{
					error = ex.Message,
					stack = ex.StackTrace
				});

				await ctx.Response.WriteAsync(payload);
			}
		}
	}

}
