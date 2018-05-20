using System;
using Stratis.SmartContracts;

public class PseudoRandom : SmartContract
{
    public PseudoRandom(ISmartContractState smartContractState)
    : base(smartContractState)
    {
        LastRandInt = 0;
        LogMessage("Initialized");
    }

    public void LogMessage(string LogEntry)
    {
        var untruncatedLog = Log + " " + LogEntry;
        //Log holds last 500 characters
        Log = untruncatedLog.Substring(Math.Max(untruncatedLog.Length - 500, 0));
    }

    public uint GetPseudoRandInt()
    {
        ulong pseudoRandIntPreModulus = (ulong)AddressAsInt(Block.Coinbase) + (ulong)AddressAsInt(Message.Sender) + Block.Number + (ulong)LastRandInt + Message.GasLimit.Value;
        uint pseudoRandInt = (uint)(pseudoRandIntPreModulus % UInt32.MaxValue); //Modulo Bias
        LastRandInt = pseudoRandInt;
        LogMessage("Pseudo random int generated: " + pseudoRandInt);
        return pseudoRandInt;
    }

    public uint GetPseudoRandIntWithMax(uint Max)
    {
        // Keeping max low to prevent significant modulo bias
        Assert(Max <= 1000);
        Assert(Max > 0);
        var randInt = GetPseudoRandInt() % Max; //Modulo Bias
        LogMessage("Pseudorandom int with max " + Max + ": " + randInt);
        return randInt;
    }

    private uint AddressAsInt(Address a)
    {
        var overMaxInt = HashStringTo18DigitNumber(a.Value);
        uint returnUint = (uint)(overMaxInt % UInt32.MaxValue); //Modulo Bias
        return returnUint;
    }

    public bool CoinFlip()
    {
        var flipResult = GetPseudoRandInt() % 2 == 0; //Modulo Bias
        LogMessage("Flip result: " + flipResult);
        return flipResult;
    }

    public string Log
    {
        get
        {
            return PersistentState.GetObject<string>("Log");
        }
        set
        {
            PersistentState.SetObject<string>("Log", value);
        }
    }

    public uint LastRandInt
    {
        get
        {
            return PersistentState.GetObject<uint>("LastRandInt");
        }
        set
        {
            PersistentState.SetObject<uint>("LastRandInt", value);
        }
    }

    public ulong HashStringTo18DigitNumber(string s)
    {
        byte[] bytes = StringToByteArray(s);
        var firstHash = Keccak256(bytes);
        var hashedBytes = Keccak256(firstHash);
        var stringOfNumbers = byteArrayToNumberString(hashedBytes);
        var startPositionOfStringSlice = Math.Max(stringOfNumbers.Length - UInt64.MaxValue.ToString().Length + 2, 0);
        var max18CharacterString = stringOfNumbers.Substring(startPositionOfStringSlice);
        var returnval = (ulong)Int64.Parse(max18CharacterString);
        LogMessage("Hashed " + s + " to " + returnval);
        return returnval;
    }

    public string HashStringToAlphaNumericString(string s)
    {
        byte[] bytes = StringToByteArray(s);
        var firstHash = Keccak256(bytes);
        var hashedBytes = Keccak256(firstHash);
        var alphanumericString = byteArrayToAlphaNumericString(hashedBytes);
        LogMessage("Hashed " + s + " to " + alphanumericString);
        return alphanumericString;
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