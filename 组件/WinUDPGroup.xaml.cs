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
using System.Windows.Shapes;
using System.Net;
using System.Net.NetworkInformation;

namespace TCPTools {
	/// <summary>
	/// WinUDPGroup.xaml 的交互逻辑
	/// </summary>
	public partial class WinUDPGroup : Window {
		public WinUDPGroup() {
			InitializeComponent();

			interfaces = NetworkInterface.GetAllNetworkInterfaces();
			comboDev.ItemsSource = interfaces;
			comboDev.SelectedIndex = 0;

			ipBox.Focus();
		}

		NetworkInterface[] interfaces;
		UnicastIPAddressInformationCollection ipInf;

		private void confirm_Click(object sender, RoutedEventArgs e) {
			((MainWindow)Owner).ip = ipBox.IP;
			((MainWindow)Owner).port = portBox.Port;
			((MainWindow)Owner).returnIndex1 = comboDev.SelectedIndex;
			((MainWindow)Owner).returnIndex2 = comboIP.SelectedIndex;
            DialogResult = true;
			Close();
		}

		private void cancel_Click(object sender, RoutedEventArgs e) {
			DialogResult = false;
			Close();
		}

		private void comboDev_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			ipInf = interfaces[comboDev.SelectedIndex].GetIPProperties().UnicastAddresses;
			comboIP.ItemsSource = ipInf;
			comboIP.SelectedIndex = 0;
		}
	}
}
