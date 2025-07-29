 public class DuplicateLaneException : Exception
    {
        public DuplicateLaneException(string message) : base(message) { }
    }

    public class InvalidSubredditException : Exception
    {
        public InvalidSubredditException(string message) : base(message) { }
    }