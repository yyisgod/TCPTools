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

namespace TCPTools.组件 {
	/// <summary>
	/// PortBox.xaml 的交互逻辑
	/// </summary>
	public partial class PortBox : UserControl {
		public PortBox() {
			InitializeComponent();
		}

		static PortBox() {
			portProperty = DependencyProperty.Register("Port", typeof(UInt16), typeof(PortBox));
		}

		public static DependencyProperty portProperty;

		public UInt16 Port {
			get { return (UInt16)GetValue(portProperty); }
			set { SetValue(portProperty, value); }
		}

		private void portTextBox_KeyDown(object sender, KeyEventArgs e) {
			if (staticFuncs.IsNumKey(e))
				e.Handled = false;
			else
				e.Handled = true;

			return;
		}

		private void portTextBox_TextChanged(object sender, TextChangedEventArgs e) {
			UInt16 port;
			if (!UInt16.TryParse(portTextBox.Text, out port))
				portTextBox.Text = "0";

			Port = UInt16.Parse(portTextBox.Text);
		}
	}
}
