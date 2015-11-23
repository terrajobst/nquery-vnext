using System;

namespace NQuery
{
    internal static class ExceptionBuilder
    {
        public static Exception UnexpectedValue(object value)
        {
            var message = value == null
                ? @"A null value was unexpected"
                : $"The value '{value}' of type {value.GetType().Name} was unexpected";

            return new InvalidOperationException(message);
        }
    }
}