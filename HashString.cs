using System;
using Stratis.SmartContracts;

public class HashString : SmartContract
{
    public HashString(ISmartContractState smartContractState)
    : base(smartContractState)
    {
    }

    public ulong HashStringTo18DigitNumber(string s)
    {
        byte[] bytes = StringToByteArray(s);
        var firstHash = Keccak256(bytes);
        var hashedBytes = Keccak256(firstHash);
        var stringOfNumbers = byteArrayToNumberString(hashedBytes);
        return (ulong)Int64.Parse(stringOfNumbers.Substring(Math.Max(stringOfNumbers.Length - UInt64.MaxValue.ToString().Length, 0)));
    }

    public string HashStringToAlphaNumericString(string s)
    {
        byte[] bytes = StringToByteArray(s);
        var firstHash = Keccak256(bytes);
        var hashedBytes = Keccak256(firstHash);
        return byteArrayToAlphaNumericString(hashedBytes);
    }

    private byte[] StringToByteArray(string s)
    {
        var chars = s.ToCharArray();
        byte[] bytes = new byte[chars.Length];
        var loopcount = 0;
        foreach (var singleCharacter in chars)
        {
            bytes[loopcount] = (byte)chars[loopcount];
            loopcount++;
        }
        return bytes;
    }

    private string byteArrayToNumberString(byte[] bytes)
    {
        var outputString = "";
        foreach (var singleByte in bytes)
        {
            outputString = outputString + singleByte.ToString();
        }
        return outputString;
    }

    private string byteArrayToAlphaNumericString(byte[] bytes)
    {
        var base58 = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
        var outputString = "";
        foreach (var singleByte in bytes)
        {
            outputString = outputString + base58[(int)singleByte % 58];
        }
        return outputString;
    }
}