using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;

namespace TCPTools.组件 {
	/// <summary>
	/// IpBox.xaml 的交互逻辑
	/// </summary>
	public partial class IpBox : UserControl {
		public IpBox() {
			InitializeComponent();
			SetValue(IpProperty, IPAddress.Any);
			byte[] ip = new byte[4];
			ip[0] = Convert.ToByte(IP1);
			ip[1] = Convert.ToByte(IP2);
			ip[2] = Convert.ToByte(IP3);
			ip[3] = Convert.ToByte(IP4);
			try {
				SetValue(IpProperty, new IPAddress(ip));
			} catch (Exception) { }
		}

		static IpBox() {
			IpProperty = DependencyProperty.Register("IP", typeof(IPAddress), typeof(IpBox));
			Ip1Property = DependencyProperty.Register("IP1", typeof(string), typeof(IpBox)
				, new PropertyMetadata("192", new PropertyChangedCallback(OnIPChanged)));
			Ip2Property = DependencyProperty.Register("IP2", typeof(string), typeof(IpBox)
				, new PropertyMetadata("168", new PropertyChangedCallback(OnIPChanged)));
			Ip3Property = DependencyProperty.Register("IP3", typeof(string), typeof(IpBox)
				, new PropertyMetadata("1", new PropertyChangedCallback(OnIPChanged)));
			Ip4Property = DependencyProperty.Register("IP4", typeof(string), typeof(IpBox)
				, new PropertyMetadata("1", new PropertyChangedCallback(OnIPChanged)));
		}

		private static bool IsIpVaild(string value) {
			string ip = value;
			try {
				Int16 ipNum = Convert.ToInt16(ip);
				if (0 <= ipNum && ipNum <= 255)
					return true;
				else {
					return false;
				}
            } catch (Exception) {
				return false;
			}
		}

		public IPAddress IP {
			get { return (IPAddress)GetValue(IpProperty);  }
			//set { SetValue(IpProperty, value); }
		}

		public string IP1 {
			get { return (string)GetValue(Ip1Property);  }
			set { SetValue(Ip1Property, value); }
		}

		public string IP2 {
			get { return (string)GetValue(Ip2Property);  }
			set { SetValue(Ip2Property, value); }
		}

		public string IP3 {
			get { return (string)GetValue(Ip3Property);  }
			set { SetValue(Ip3Property, value); }
		}

		public string IP4 {
			get { return (string)GetValue(Ip4Property); }
			set { SetValue(Ip4Property, value); }
		}

		public static bool IsVaildIP { get; set; }

		public static DependencyProperty IpProperty;
		public static DependencyProperty Ip1Property;
		public static DependencyProperty Ip2Property;
		public static DependencyProperty Ip3Property;
		public static DependencyProperty Ip4Property;

		private static void OnIPChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
			IpBox box = (IpBox)sender;

			try {
				byte[] ip = new byte[4];
				ip[0] = Convert.ToByte(box.IP1);
				ip[1] = Convert.ToByte(box.IP2);
				ip[2] = Convert.ToByte(box.IP3);
				ip[3] = Convert.ToByte(box.IP4);
				if (ip[0] > 0 && ip[0] < 255 && ip[1] >= 0 && ip[1] < 255
					&& ip[2] >= 0 && ip[2] < 255 && ip[3] > 0 && ip[3] < 255)
					IsVaildIP = true;
				else
					IsVaildIP = false;
				box.SetValue(IpProperty, new IPAddress(ip));
			} catch (Exception) { }
		}

		private void KeyDeal(TextBox textBox, KeyEventArgs e) {
			if (staticFuncs.IsNumKey(e)) {
				e.Handled = false;
				if (textBox.Text == "0" && e.Key != Key.Tab)
					textBox.Text = "";
			} else
				e.Handled = true;
			
		}

		private void TextDeal(TextBox textBox) {
			if (!IsIpVaild(textBox.Text) && textBox.Text != "")
				textBox.Text = "0";
		}

		private void textBox_TextChanged(object sender, TextChangedEventArgs e) {
			TextDeal((TextBox)sender);
		}

		private void textBox_KeyDown(object sender, KeyEventArgs e) {
			KeyDeal((TextBox)sender, e);
		}

		private void textBox_GotFocus(object sender, RoutedEventArgs e) {
			((TextBox)sender).SelectAll();
        }
	}
}
