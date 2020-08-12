﻿using CommonLib.Mvvm;

namespace MFCSoftware.ViewModels
{
    public class MainWindowViewModel:BindableBase
    {
        public bool Enable
        {
            get => _channelCount <= 0;
        }

        private int _channelCount;
        public int ChannelCount
        {
            get => _channelCount;
            set
            {
                _channelCount = value;
                RaiseProperty(nameof(Enable));
            }
        }
    }
}
