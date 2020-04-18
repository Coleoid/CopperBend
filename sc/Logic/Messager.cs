using System;
using System.Collections.Generic;
using System.Linq;
using CopperBend.Contract;
using CopperBend.Fabric;
using Microsoft.Xna.Framework.Input;
using SadConsole.Input;

namespace CopperBend.Logic
{
    public class Messager : IMessager, IPanelService
    {
        private const int messageLimitBeforePause = 3;  //0.1: artificially low for short-term testing
        private int messagesSentSincePause = 0;

        private ModeNode ModeNode { get; set; }
        public Queue<AsciiKey> InputQueue { get; private set; }
        public Queue<string> MessageQueue { get; private set; }
        public Messager(ModeNode node)
        {
            ModeNode = node;
            InputQueue = new Queue<AsciiKey>();
            MessageQueue = new Queue<string>();
        }

        public void RegisterWithPanel(IServicePanel isp)
        {
            //  Maybe not much?  Still unsure how to sequence ISP events.
        }

        //POST-CONSTRUCTOR dependency:
        /// <summary> MessageWindow must be set on Messager after Engine constructs it. </summary>
        public IMessageLogWindow MessageWindow { get; set; }

        public Func<bool> ShouldClearQueueOnEscape { get; set; }

        public void QueueInput(IReadOnlyCollection<AsciiKey> keys)
        {
            //0.K
            foreach (var key in keys)
            {
                // Escape key processing may skip the normal input queue,
                // to make 'Quit Game' as reliably available as possible.
                if (key == Keys.Escape && ShouldClearQueueOnEscape())
                {
                    InputQueue.Clear();
                    return;
                }

                InputQueue.Enqueue(key);
            }
        }

        public AsciiKey GetNextKeyPress()
        {
            return IsInputReady() ? GetNextInput() : new AsciiKey { Key = Keys.None };
        }

        public bool IsInputReady() => InputQueue.Count > 0;
        private AsciiKey GetNextInput() => InputQueue.Dequeue();
        public void ClearPendingInput() => InputQueue.Clear();



        public void WriteLineIfPlayer(IBeing being, string message)
        {
            if (being.IsPlayer) WriteLine(message);
        }

        private Dictionary<Messages, bool> SeenMessages { get; } = new Dictionary<Messages, bool>();

        /// <summary> First time running across this message in this game run? </summary>
        public bool FirstTimeFor(Messages key)
        {
            var firstTime = !SeenMessages.ContainsKey(key);
            if (firstTime)
                SeenMessages.Add(key, true);

            return firstTime;
        }

        /// <summary>
        /// This allows messages to adapt based on the Being involved and
        /// what messages have already been seen, how many times, et c.
        /// </summary>
        public void Message(IBeing being, Messages messageKey)
        {
            Guard.Against(messageKey == Messages.Unset, "Must set message key");
            if (!being.IsPlayer) return;

            switch (messageKey)
            {
            case Messages.BarehandRotDamage:
                if (FirstTimeFor(messageKey))
                {
                    //0.2  promote to alert
                    WriteLine("I tear a chunk off the ground.  It fights back--burns my hands.");
                    WriteLine("The stuff withers away from where I grab it.");
                }
                else
                {
                    WriteLine("I hit it, and the stuff withers.");
                }

                break;

            case Messages.RotDamageSpreads:
                WriteLine("The damage to this stuff spreads outward.  Good.");
                break;

            default:
                var need_message_for_key = $"Must code message for key [{messageKey}].";
                WriteLine(need_message_for_key);
                throw new Exception(need_message_for_key);
            }
        }

        public void WriteLine(string message)
        {
            AddMessage(message);
        }

        public void Prompt(string message)
        {
            MessageWindow.Prompt(message);
        }

        public void More()
        {
            PromptUserForMoreAndPend();
        }


        public void AddMessage(string newMessage)
        {
            MessageQueue.Enqueue(newMessage);
            ShowMessages();
        }

        //0.1: ResetMessagesSentSincePause() needs to be called all through the ICS.
        public void ResetMessagesSentSincePause() => messagesSentSincePause = 0;

        private void ShowMessages()
        {
            //0.2: There are probably other modes where we want to suspend messages.
            while (ModeNode.CurrentMode != EngineMode.MessagesPendingUserInput && MessageQueue.Any())
            {
                if (messagesSentSincePause >= messageLimitBeforePause)
                {
                    PromptUserForMoreAndPend();
                    return;
                }

                var nextMessage = MessageQueue.Dequeue();
                MessageWindow.WriteLine(nextMessage);
                messagesSentSincePause++;
            }
        }

        private void PromptUserForMoreAndPend()
        {
            MessageWindow.WriteLine("-- more --");
            ModeNode.PushEngineMode(EngineMode.MessagesPendingUserInput, HandleMessagesPending);
        }

        private void HandleMessagesPending()
        {
            for (AsciiKey k = GetNextKeyPress(); k.Key != Keys.None; k = GetNextKeyPress())
            {
                if (k.Key == Keys.Space)
                {
                    ResetMessagesSentSincePause();
                    ModeNode.PopEngineMode();
                    ShowMessages();  // ...which may have enough messages to pend us again.
                }
            }
        }

        #region LargeMessages, currently empty
        //  The engine calls here when we're in EngineMode.LargeMessagePending
        public void HandleLargeMessage()
        {
            //RLKeyPress press = GameWindow.GetNextKeyPress();
            //while (press != null
            //       && press.Key != RLKey.Escape
            //       && press.Key != RLKey.Enter
            //       && press.Key != RLKey.KeypadEnter
            //       && press.Key != RLKey.Space
            //)
            //{
            //    press = GameWindow.GetNextKeyPress();
            //}

            //if (press == null) return;

            HideLargeMessage();
        }

        public void HideLargeMessage()
        {
            throw new NotImplementedException();
        }
        #endregion

    }
}
