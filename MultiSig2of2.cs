using Stratis.SmartContracts;

//This smart contract functions as a 2 of 2 multisig account. It has not been audited or extensively tested. Use at your own risk.

public class MultiSig2of2 : SmartContract
{
    // Constructor takes two parameters - the 2 owners of the multisig. They must agree to make any outgoing transactions.
    public MultiSig2of2(ISmartContractState smartContractState, Address owner1, Address owner2)
    : base(smartContractState)
    {
        Owner1 = owner1;
        Owner2 = owner2;
        LogMessage("Constructor called. Owners: " + owner1 + ", " + owner2);
    }

    // Used internally for logging. The Log can be inspected externally through the API.
    private void LogMessage(string LogEntry)
    {
        // This log is useful for debugging.
        // However, rewriting all this data is inefficient with regards to gas! Not for production use.
        Log = Log + LogEntry + " | ";
    }

    // Explicit method for depositing into the multisig contract
    public void Deposit()
    {
        LogMessage("Deposit made from " + Message.Sender + " for amount " + Message.Value);
    }

    // Used by each of the owners to request a withdrawal from the multisig to an external address
    public void Send(Address requestedAddress, ulong requestedAmount)
    {
        LogMessage("Send request from " + Message.Sender + " for amount " + requestedAmount + " to address " + requestedAddress);
        if (Message.Sender != Owner1 && Message.Sender != Owner2)
        {
            LogMessage("Rejecting non-owner request.");
            return;
        }
        Assert(Message.Sender == Owner1 || Message.Sender == Owner2);
        Assert(requestedAmount > 0);
        RequestedAmounts[Message.Sender.Value] = requestedAmount;
        RequestedAddresses[Message.Sender.Value] = requestedAddress;
        if (RequestedAmounts[Owner1.Value] == RequestedAmounts[Owner2.Value] && RequestedAddresses[Owner1.Value] == RequestedAddresses[Owner2.Value])
        {
            LogMessage("Both owners agree.");
            LogMessage("Approved send request for amount " + requestedAmount + " to address " + requestedAddress);
            ITransferResult transferResult = TransferFunds(requestedAddress, requestedAmount);
            if (transferResult.Success)
            {
                LogMessage("Transfer Succesful");
                RequestedAmounts[Owner1.Value] = 0;
                RequestedAmounts[Owner2.Value] = 0;
            }
            else
            {
                LogMessage("Transfer failed. Reason: " + transferResult.ThrownException);
            }
        }
        else
        {
            LogMessage("Both owners do not yet agree. No transfer yet.");
            if (RequestedAmounts[Owner1.Value] > 0)
            {
                LogMessage("Owner1 wants to send " + RequestedAmounts[Owner1.Value] + " to " + RequestedAddresses[Owner1.Value]);
            }
            if (RequestedAmounts[Owner2.Value] > 0)
            {
                LogMessage("Owner2 wants to send " + RequestedAmounts[Owner2.Value] + " to " + RequestedAddresses[Owner2.Value]);
            }
        }
    }

    // State variables:

    public Address Owner1
    {
        get
        {
            return PersistentState.GetObject<Address>("Owner1");
        }
        set
        {
            PersistentState.SetObject<Address>("Owner1", value);
        }
    }

    public Address Owner2
    {
        get
        {
            return PersistentState.GetObject<Address>("Owner2");
        }
        set
        {
            PersistentState.SetObject<Address>("Owner2", value);
        }
    }

    public ISmartContractMapping<ulong> RequestedAmounts
    {
        get
        {
            return PersistentState.GetMapping<ulong>("RequestedAmounts");
        }
    }

    public ISmartContractMapping<Address> RequestedAddresses
    {
        get
        {
            return PersistentState.GetMapping<Address>("RequestedAddresses");
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


}