using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.SideChannels;
using System.Text;
using System;

public class CustomSideChannel : SideChannel
{
    public CustomSideChannel()
    {
        ChannelId = new Guid("621f0a70-4f87-11ea-a6bf-784f4387d1f7");
    }

    protected override void OnMessageReceived(IncomingMessage msg)
    {
        var receivedString = msg.ReadString();
        MessageEventArgs msgEventArgs = new MessageEventArgs();
        msgEventArgs.message = receivedString;

        // Pass the message up through a new event to be handled by the agent
        OnMessageToPass(msgEventArgs);
    }

    public void SendStringToPython(string stringToSend)
    {
        using (var msgOut = new OutgoingMessage())
            {
                msgOut.WriteString(stringToSend);
                QueueMessageToSend(msgOut);
            }
    }

    // Define a new event to pass the received messages along
    public event EventHandler<MessageEventArgs> MessageToPass;
    protected virtual void OnMessageToPass(MessageEventArgs message)
    {
        EventHandler<MessageEventArgs> handler = MessageToPass;
        handler?.Invoke(this, message);
    }
}

public class MessageEventArgs : EventArgs
{
    public string message { get; set; }
}