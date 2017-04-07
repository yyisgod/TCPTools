using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TCPTools {
	static class staticFuncs {
		public static bool IsNumKey(KeyEventArgs e) {
			if (e.Key == Key.Tab)
				return true;

			if ((e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)) {
				return true;
			} else if ((e.Key >= Key.D0 && e.Key <= Key.D9) && e.KeyboardDevice.Modifiers != ModifierKeys.Shift) {
				return true;
			} else
				return false;
		}
	}
}
