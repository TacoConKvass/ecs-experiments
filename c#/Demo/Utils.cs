using System;
using System.Collections;
using System.Text;

namespace Demo.Utils;

public static class Assert {
	public static int TestNumber = 0;
	public static int FailedCount = 0;

	public static void Success(string test, bool expected) {
		Console.Write($"[Test {++TestNumber}{(TestNumber < 10 ? " " : "")} - ");
		Console.ForegroundColor = expected ? ConsoleColor.Green : ConsoleColor.Red;
		Console.Write($"{(expected ? "Passed \u2713" : "Failed \u2717")}");
		Console.ResetColor();
		Console.Write($"]: {test}\n");
		if (!expected) FailedCount++;
	}
	
	public static void Failure(string test, bool expected) => Success(test, !expected); 
	
	public static void FinalStatus() => Console.WriteLine($"\nPassed {TestNumber - FailedCount} / {TestNumber} tests");
}

public static class BitArrayExtension {
	public static string BinaryValues(this BitArray bitset) {
		StringBuilder sb = new StringBuilder(bitset.Count);
		for (int i = 0; i < bitset.Count; i++) sb.Append(bitset[i] ? '1' : '0');
		return sb.ToString();
	}
}