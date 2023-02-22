using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace yolov3TinyMauiOnnx.Model
{
	public class DebugManagement : INotifyPropertyChanged
    {
        #region PropertyChangedSetup
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        private string b_DebugString;

        public string B_DebugString
        {
            get
            {
                return b_DebugString;
            }
            set
            {
                b_DebugString = value;
                OnPropertyChanged();
            }
        }

        public DebugManagement()
		{
            B_DebugString = "No message updates";
		}

        public void SetDebugMessage(string message)
        {
            B_DebugString = message;
        }
	}
}

