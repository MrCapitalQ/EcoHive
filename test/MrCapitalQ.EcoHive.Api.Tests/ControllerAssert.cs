using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace MrCapitalQ.EcoHive.Api.Tests
{
    internal static class ControllerAssert
    {
        public static void IsStatusCode(IConvertToActionResult result, int expectedStatusCode)
            => IsStatusCode(result.Convert(), expectedStatusCode);

        public static void IsStatusCode(IActionResult result, int expectedStatusCode)
        {
            var statusCodeActionResult = Assert.IsAssignableFrom<IStatusCodeActionResult>(result);
            Assert.Equal((int)expectedStatusCode, statusCodeActionResult.StatusCode);
        }

        public static T IsObjectResult<T>(IConvertToActionResult result, int expectedStatusCode)
            where T : class
            => IsObjectResult<T>(result.Convert(), expectedStatusCode);

        public static T IsObjectResult<T>(IActionResult result, int expectedStatusCode)
            where T : class
            => IsObjectResultCore<T>(result, expectedStatusCode);

        public static T IsCreatedAtRouteResult<T>(IActionResult result, string expectedRouteName)
            where T : class
        {
            var actual = IsObjectResultCore<T>(result, StatusCodes.Status201Created);

            var createdAtRouteResult = Assert.IsType<CreatedAtRouteResult>(result);
            Assert.Equal(expectedRouteName, createdAtRouteResult.RouteName);

            return actual;
        }

        private static T IsObjectResultCore<T>(IActionResult result, int expectedStatusCode)
            where T : class
        {
            var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
            Assert.Equal(expectedStatusCode, objectResult.StatusCode);
            return Assert.IsAssignableFrom<T>(objectResult.Value);
        }
    }
}
