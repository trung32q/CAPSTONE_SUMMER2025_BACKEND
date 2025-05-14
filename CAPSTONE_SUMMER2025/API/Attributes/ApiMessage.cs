namespace API.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ApiMessage : Attribute
    {
        public string Message { get; }

        public ApiMessage(string message)
        {
            Message = message;
        }
    }
}
