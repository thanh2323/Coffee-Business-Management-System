namespace CoffeeShop.Application.Common
{
    public class ServiceResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }

        public static ServiceResult Success(string message = "Success")
            => new ServiceResult { IsSuccess = true, Message = message };

        public static ServiceResult Failed(string message)
            => new ServiceResult { IsSuccess = false, Message = message };
    }
}
