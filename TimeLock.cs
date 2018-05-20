using System;
using Stratis.SmartContracts;

public class TimeLock : SmartContract
{
    public TimeLock(ISmartContractState smartContractState, ulong duration, Address owner)
    : base(smartContractState)
    {
        EndBlock = Block.Number + duration;
        LogMessage("End Block: " + EndBlock);
        Owner = owner;
        LogMessage("Owner: " + Owner);
    }

    private void LogMessage(string LogEntry)
    {
        // This log is useful for debugging.
        // However, rewriting all this data is inefficient with regards to gas! Not for production use.
        Log = Log + LogEntry + " | ";
    }

    public void Deposit()
    {
        LogMessage("Deposited: " + Message.Value + " from " + Message.Sender);
    }

    public void Withdraw()
    {
        try
        {
            Assert(Message.Sender == Owner);
            Assert(Block.Number >= EndBlock);
            TransferFunds(Message.Sender, this.Balance);
            LogMessage("Withdraw processed.");
        }
        catch (Exception e)
        {
            LogMessage("Exception: " + e);
        }
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

    public Address Owner
    {
        get
        {
            return PersistentState.GetObject<Address>("Owner");
        }
        set
        {
            PersistentState.SetObject<Address>("Owner", value);
        }
    }

    public ulong EndBlock
    {
        get
        {
            return PersistentState.GetObject<ulong>("EndBlock");
        }
        set
        {
            PersistentState.SetObject<ulong>("EndBlock", value);
        }
    }

}