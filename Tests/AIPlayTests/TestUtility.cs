using System;

public class TestUtility
{
  public static bool IsWindows()
  {
    return (int)Environment.OSVersion.Platform < 4; // 0 1 2 3 are windows
  }
}