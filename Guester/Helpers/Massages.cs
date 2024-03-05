using System;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Guester.Helpers
{
    public class Massges : ValueChangedMessage<string>
    {
        public Massges(string value) : base(value) { }
    }

    public class CloseOrderOptions : Massges
    {
        public CloseOrderOptions(string value) : base(value) { }
    }

    public class CloseFunctionMenu : Massges
    {
        public CloseFunctionMenu(string value) : base(value) { }
    }

    //public class CloseIsBusy : Massges
    //{
    //    public CloseIsBusy(string value) : base(value) { }
    //}
    
}
