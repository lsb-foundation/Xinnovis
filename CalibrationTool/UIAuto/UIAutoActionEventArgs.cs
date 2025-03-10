﻿using System;
using System.Collections.Generic;

namespace CalibrationTool.UIAuto
{
    public sealed class UIAutoActionEventArgs : EventArgs
    {
        public CommandElement Command { get;}
        public ActionElement Action { get; }
        public List<ParameterElement> Parameters { get; }

        public UIAutoActionEventArgs(CommandElement command, ActionElement action, List<ParameterElement> parameters) : base()
        {
            Command = command;
            Action = action;
            Parameters = parameters;
        }
    }
}
