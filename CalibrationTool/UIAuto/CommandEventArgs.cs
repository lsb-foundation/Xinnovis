using System;
using System.Collections.Generic;

namespace CalibrationTool.UIAuto
{
    public sealed class CommandEventArgs : EventArgs
    {
        public CommandElement Command { get;}
        public ActionElement Action { get; }
        public List<ParameterElement> Parameters { get; }

        public CommandEventArgs(CommandElement command, ActionElement action, List<ParameterElement> parameters) : base()
        {
            Command = command;
            Action = action;
            Parameters = parameters;
        }
    }
}
