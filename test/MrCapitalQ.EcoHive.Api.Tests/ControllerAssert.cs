using Microsoft.AspNetCore.Mvc;

namespace MrCapitalQ.EcoHive.Api.Tests
{
    internal static class ControllerAssert
    {
        public static T IsOkObjectResult<T>(IActionResult result) where T : class
        {
            return IsObjectResult<OkObjectResult, T>(result);
        }

        public static T IsCreatedAtRouteResult<T>(IActionResult result, string expectedRouteName) where T : class
        {
            var actual = IsObjectResult<CreatedAtRouteResult, T>(result);

            var createdAtRouteResult = (CreatedAtRouteResult)result;
            Assert.Equal(expectedRouteName, createdAtRouteResult.RouteName);

            return actual;
        }

        private static T IsObjectResult<TObjectResult, T>(IActionResult result)
            where TObjectResult : ObjectResult
            where T : class
        {
            Assert.IsType<TObjectResult>(result);

            var value = ((TObjectResult)result).Value;
            Assert.IsAssignableFrom<T>(((TObjectResult)result).Value);

            var actual = value as T;
            Assert.NotNull(actual);
            return actual;
        }
    }
}
