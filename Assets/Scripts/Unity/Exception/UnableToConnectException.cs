using System;

public class UnableToConnectException : Exception
{
  public UnableToConnectException()
  {
  }

  public UnableToConnectException(string message)
      : base(message)
  {
  }

  public UnableToConnectException(string message, Exception inner)
      : base(message, inner)
  {
  }
}