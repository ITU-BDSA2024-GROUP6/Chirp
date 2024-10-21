using System;

namespace MyChat.Razor.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException() : base() { }
        public NotFoundException(string message) : base(message) { }
        public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class DuplicateAuthorException : Exception
    {
        public DuplicateAuthorException() : base() { }
        public DuplicateAuthorException(string message) : base(message) { }
        public DuplicateAuthorException(string message, Exception innerException) : base(message, innerException) { }
    }
}