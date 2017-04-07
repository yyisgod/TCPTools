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
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows.Media.Media3D;

namespace TCPTools {
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();
			
			socketModels.Add(new TCPServerSocket());
			socketModels.Add(new TCPClientModel());

			socketModels.Add(new UDPServerModel());
			socketModels.Add(new UDPClientModel());

			socketModels.Add(new UDPGroupSocket());

			AsynchronousSocketListener.recvEvent += RecvMsg;
			AsynchronousSocketListener.acceptEvent += acceptMsg;

			treeViewServer.ItemsSource = socketModels;
			socketModels[0].Selected = true;
			treeViewServer.Focus();
			ShowDefaultView();
        }
		
		// 成员变量
		List<IFPropertyNodeItem> socketModels = new List<IFPropertyNodeItem>();
		CurrentStatus status = new CurrentStatus();
		// 创建socket时的返回参数
		public int port;
		public IPAddress ip;
		// 一级、二级、三级序号
		public int index1 = -1;
		public int index2 = -1;
		public int index3 = -1;

		public int returnIndex1; // 创建UDP组播时的返回值
		public int returnIndex2;
		public int port2;
		
		private int socketCount = 0; // 当前1级model的数量

		//private List<SocketModel> socketModels = new List<SocketModel>();

		private void Button_Create_Click(object sender, RoutedEventArgs e) {
			if (index1 == -1)
				return;

			bool success = false;

			IFPropertyNodeItem item = socketModels[index1];
			if (item.Name == "TCPServer") {
				Window serverWin = new WinSocketCreate();
				serverWin.Owner = this;
				serverWin.Left = this.Left + this.Width / 2 - serverWin.Width / 2;
				serverWin.Top = this.Top + this.Width / 5;
				if (serverWin.ShowDialog() == true) {
					// 创建TCP服务器,修改显示界面
					if (((TCPServerSocket)item).CreateSocket(port) == 0) {
						// display
						success = true;
					} else
						MessageBox.Show("端口" + port + "已被绑定！", "创建失败:");
				}
			}
			if (item.Name == "TCPClient") {
				Window win = new WinTCPClientCreate();
				win.Owner = this;
				win.Left = this.Left + this.Width / 2 - win.Width / 2;
				win.Top = this.Top + this.Width / 5;
				if (win.ShowDialog() == true) {
					// 创建TCP客户端,修改显示界面
					if (((TCPClientModel)item).CreateSocket(new IPEndPoint(ip, port)) == 0) {
						// display
						success = true;
					} else
						MessageBox.Show("端口" + port + "已被绑定！", "创建失败:");
				}
			}
			if (item.Name == "UDPServer") {
				Window serverWin = new WinSocketCreate();
				serverWin.Owner = this;
				serverWin.Left = this.Left + this.Width / 2 - serverWin.Width / 2;
				serverWin.Top = this.Top + this.Width / 5;
				((WinSocketCreate)serverWin).portBox.Port = 50000;
				if (serverWin.ShowDialog() == true) {
					// 创建UDP服务器,修改显示界面
					if (((UDPServerModel)item).CreateSocket(port) == 0) {
						// display
						success = true; ;
					} else
						MessageBox.Show("端口" + port + "已被绑定！", "创建失败:");
				}
			}
			if (item.Name == "UDPClient") {
				Window win = new WinUDPClientCrete();
				win.Owner = this;
				win.Left = this.Left + this.Width / 2 - win.Width / 2;
				win.Top = this.Top + this.Width / 5;
				if (win.ShowDialog() == true) {
					// 创建UDP服务器,修改显示界面
					if (((UDPClientModel)item).CreateSocket(new IPEndPoint(ip, port), port2) == 0) {
						// display
						success = true; ;
					} else
						MessageBox.Show("端口" + port2 + "已被绑定！", "创建失败:");
				}
			}
			if (item.Name == "UDPGroup") {
				Window groupWin = new WinUDPGroup();
				groupWin.Owner = this;
				groupWin.Left = this.Left + this.Width / 2 - groupWin.Width / 2;
				groupWin.Top = this.Top + this.Width / 5;
				if (groupWin.ShowDialog() == true) {
					// 创建UDP组播
					if (((UDPGroupSocket)item).CreateSocket(new IPEndPoint(ip, port), returnIndex1, returnIndex2) == 0) {
						// display
						success = true;
						//treeViewServer.ItemsSource = socketModels;
					} else
						MessageBox.Show("端口" + port + "已被绑定！", "创建失败:");
				}
			}

			if (success) {
				((SocketModel)item).UpdateTreeView(item, true);
				socketCount++;
				item.Expanded = true;
				UpdateView();
				RefreshTree();
			}
        }

		private void UpdateView() {
			if (socketCount > 0) {
				GetIndex();
				rightPannel.Visibility = Visibility.Visible;
				myViewPort.Visibility = Visibility.Hidden;
				SwitchView();
				Refresh();
			} else {
				status.index1 = -1;
				status.index2 = -1;
				status.index3 = -1;
				ShowDefaultView();
			}
		}

		private void DelNode(int index1, int index2, int index3) {
			if (index1 == -1 || index2 == -1)
				return;
			int localIndex2 = index2;
			int localIndex3 = index3;

			if (index3 == -1)
				socketCount--;

			status.index1 = -1;
			status.index2 = -1;
			status.index3 = -1;

			IFPropertyNodeItem p = socketModels[index1];
			if (((SocketModel)p).DelSocket(index2, index3)) {
				((SocketModel)p).UpdateTreeView(p, false);

				// 更新选中情况
				ReSelectItem(p, localIndex2, localIndex3);
			} else {
				RefreshTree();
				UpdateView();
			}
		}

		private void Button_Del_Click(object sender, RoutedEventArgs e) {
			DelNode(index1, index2, index3);
		}

		private void ReSelectItem(IFPropertyNodeItem p1, int index2, int index3) {
			if (socketModels[this.index1] != p1) {
				RefreshTree();
				return;
			}

			if (index3 == -1) {
				p1.Expanded = true;
				if (index2 > 0) {
					p1.Children.Last().Selected = true;
				} else if (index2 == 0 && p1.Children.Count != 0) {
					p1.Children.Last().Selected = true;
				} else
					p1.Selected = true;
			} else if (index3 == -2) { //收包错误导致的删除
				if (p1.Children[index2].Children.Count != 0) {
					p1.Children[index2].Expanded = true;
					p1.Children[index2].Children[0].Selected = true;
				} else {
					p1.Children[index2].Selected = true;
				}
			} else if (index3 >= 0) {	
				if (index2 == -1 || this.index2 != index2) {
					RefreshTree();
					return;
				}

				IFPropertyNodeItem p2 = p1.Children[index2];
				p2.Expanded = true;
				if (index3 > 0) {
					p2.Children.Last().Selected = true;
				} else if (index3 == 0 && p2.Children.Count != 0) {
					p2.Children.Last().Selected = true;
				} else
					p2.Selected = true;
			}

			RefreshTree();
		}

		/// <summary>
		/// 展示默认页面
		/// </summary>
		private void ShowDefaultView() {
			rightPannel.Visibility = Visibility.Hidden;
			myViewPort.Visibility = Visibility.Visible;
            recvText.Text = "";
			sendText.Text = "";
		}

		private void HideAllCtls() {
			TsocketStatus.Visibility = Visibility.Hidden;

			BTCConnect.Visibility = Visibility.Hidden;
			BTSStartListen.Visibility = Visibility.Hidden;

			TLremoteIP.Visibility = Visibility.Hidden;
			TLremotePort.Visibility = Visibility.Hidden;
			TLremoteIP.Text = "对方IP: ";
			TLremotePort.Text = "对方端口: ";
			TLlocalIP.Visibility = Visibility.Hidden;
			TLlocalPort.Visibility = Visibility.Hidden;

			TremoteIP.Visibility = Visibility.Hidden;
			TremotePort.Visibility = Visibility.Hidden;
			TlocalIP.Visibility = Visibility.Hidden;
			TlocalPort.Visibility = Visibility.Hidden;
		}

		private void ShowTCPServerView() {
			TCPServerSocket p1 = (TCPServerSocket)socketModels[status.index1];
			TcpServerSocketObject p2 = (TcpServerSocketObject)p1.Children[status.index2];

			TsocketStatus.Visibility = Visibility.Visible;
			TLlocalPort.Visibility = Visibility.Visible;
			TlocalPort.Visibility = Visibility.Visible;
			if (status.index3 == -1) {
				BTSStartListen.Visibility = Visibility.Visible;
				TsocketStatus.Text = p2.IsListening ? "正在监听" : "未监听";
				BTSStartListen.Content = p2.IsListening ? "停止监听" : "启动监听";

				TlocalPort.Text = ((IPEndPoint)p2.socket.LocalEndPoint).Port.ToString();
			} else {
				RemoteSocketObject p3 = (RemoteSocketObject)p2.Children[status.index3];

				BTCConnect.Visibility = Visibility.Visible;
				TsocketStatus.Text = p3.socket.Connected ? "已连接" : "未连接";
				BTCConnect.Content = "断开连接";
				BTCConnect.IsEnabled = p3.socket.Connected;

				TLremoteIP.Visibility = Visibility.Visible;
				TremoteIP.Visibility = Visibility.Visible;
				TLremotePort.Visibility = Visibility.Visible;
				TremotePort.Visibility = Visibility.Visible;

				TremoteIP.Text = ((IPEndPoint)p3.socket.RemoteEndPoint).Address.ToString();
				TremotePort.Text = ((IPEndPoint)p3.socket.RemoteEndPoint).Port.ToString();
				TlocalPort.Text = ((IPEndPoint)p3.socket.LocalEndPoint).Port.ToString();
            }
		}

		private void ShowTCPClientView() {
			TCPClientModel p1 = (TCPClientModel)socketModels[status.index1];
			TcpClientSocketObject p2 = (TcpClientSocketObject)p1.Children[status.index2];

			TsocketStatus.Visibility = Visibility.Visible;
			if (p2.socket != null && p2.socket.Connected)
				TsocketStatus.Text = "已连接";
			else
				TsocketStatus.Text = "未连接";
            BTCConnect.Visibility = Visibility.Visible;
			BTCConnect.IsEnabled = true;
			if (p2.socket != null && p2.socket.Connected)
				BTCConnect.Content = "断开";
			else
				BTCConnect.Content = "连接";

			TLremoteIP.Visibility = Visibility.Visible;
			TremoteIP.Visibility = Visibility.Visible;
			TLremotePort.Visibility = Visibility.Visible;
			TremotePort.Visibility = Visibility.Visible;
			TLlocalPort.Visibility = Visibility.Visible;
			TlocalPort.Visibility = Visibility.Visible;

			TremoteIP.Text = p2.remoteIpEP.Address.ToString();
			TremotePort.Text = p2.remoteIpEP.Port.ToString();
			if (p2.socket != null && p2.socket.Connected)
				TlocalPort.Text = ((IPEndPoint)p2.socket.LocalEndPoint).Port.ToString();
			else
				TlocalPort.Text = "-1";
        }

		private void ShowUDPServerView() {
			UDPServerModel p1 = (UDPServerModel)socketModels[status.index1];
			UdpServerSocketObject p2 = (UdpServerSocketObject)p1.Children[status.index2];

			TLlocalPort.Visibility = Visibility.Visible;
			TlocalPort.Visibility = Visibility.Visible;
			TlocalPort.Text = ((IPEndPoint)p2.socket.LocalEndPoint).Port.ToString();
			if (status.index3 != -1) {
				BTCConnect.Visibility = Visibility.Visible;
				BTCConnect.IsEnabled = true;
				BTCConnect.Content = "删除";

				TLremoteIP.Visibility = Visibility.Visible;
				TremoteIP.Visibility = Visibility.Visible;
				TLremotePort.Visibility = Visibility.Visible;
				TremotePort.Visibility = Visibility.Visible;

				RemoteSocketObject p3 = (RemoteSocketObject)p2.Children[status.index3];

				TremoteIP.Text = ((IPEndPoint)p3.remoteEP).Address.ToString();
				TremotePort.Text = ((IPEndPoint)p3.remoteEP).Port.ToString();
			}
		}

		private void ShowUDPClientView() {
			UDPClientModel p1 = (UDPClientModel)socketModels[status.index1];
			UdpClientSocketObject p2 = (UdpClientSocketObject)p1.Children[status.index2];

			TLremoteIP.Visibility = Visibility.Visible;
			TremoteIP.Visibility = Visibility.Visible;
			TLremotePort.Visibility = Visibility.Visible;
			TremotePort.Visibility = Visibility.Visible;
			TLlocalPort.Visibility = Visibility.Visible;
			TlocalPort.Visibility = Visibility.Visible;

			TremoteIP.Text = p2.remoteIpEP.Address.ToString();
			TremotePort.Text = p2.remoteIpEP.Port.ToString();
			TlocalPort.Text = ((IPEndPoint)p2.socket.LocalEndPoint).Port.ToString();
		}

		private void ShowUDPGroupView() {
			UDPGroupSocket p1 = (UDPGroupSocket)socketModels[status.index1];
			UdpGroupSocketObject p2 = (UdpGroupSocketObject)p1.Children[status.index2];

			TLremoteIP.Visibility = Visibility.Visible;
			TremoteIP.Visibility = Visibility.Visible;
			TLremotePort.Visibility = Visibility.Visible;
			TremotePort.Visibility = Visibility.Visible;
			TLlocalPort.Visibility = Visibility.Visible;
			TlocalPort.Visibility = Visibility.Visible;

			TLremoteIP.Text = "组播地址: ";
			TLremotePort.Text = "组播端口: ";

			TremoteIP.Text = p2.remoteIpEP.Address.ToString();
			TremotePort.Text = p2.remoteIpEP.Port.ToString();
			TlocalPort.Text = TremotePort.Text;
		}

		/// <summary>
		/// 切换不同服务的页面
		/// </summary>
		private void SwitchView() {
			if (index1 < 0  && index2 < 0)
				return;
			if (index2 >= 0) {
				status.index1 = index1;
				status.index2 = index2;
				status.index3 = index3;
			} else {
				if (status.index2 < 0)
					return;
			}

			HideAllCtls();

			SocketModel p1 = (SocketModel)socketModels[status.index1];


			if (p1 is TCPServerSocket) {
				ShowTCPServerView();
			} else if (p1 is TCPClientModel) {
				ShowTCPClientView();
			} else if (p1 is UDPServerModel) {
				ShowUDPServerView();
			}else if (p1 is UDPClientModel) {
				ShowUDPClientView();
            } else if (p1 is UDPGroupSocket) {
				ShowUDPGroupView();
            } else {
				status.index1 = status.index2 = status.index3 = -1;
			}
		}

		private void sendBtn_Click(object sender, RoutedEventArgs e) {
			if (status.index1 == -1 || status.index2 == -1)
				return;

			if (sendText.Text != "") {
				((SocketModel)socketModels[status.index1]).SendPacket(status.index2, status.index3, sendText.Text);
				Refresh();
            }
		}

		private void GetIndex() {
			IFPropertyNodeItem item = treeViewServer.SelectedItem as IFPropertyNodeItem;

			if (item == null) {
				index1 = index2 = index3 = -1;
				return;
			}

			IFPropertyNodeItem p1 = null;
			IFPropertyNodeItem p2 = null;
			IFPropertyNodeItem p3 = null;

			while (true) {
				p1 = item;
				if (p1.Parent == null) {
					break;
				}

				p2 = p1;
				p1 = p2.Parent;

				if (p1.Parent == null) {
					break;
				}

				p3 = p2;
				p2 = p1;
				p1 = p2.Parent;
				break;
			}

			index1 = index2 = index3 = -1;
			int index = 0;
			foreach (var c in socketModels) {
				if (c == p1)
					break;
				index++;
			}
			index1 = index;

			if (p2 != null) {
				index = 0;
				foreach (var c in p1.Children) {
					if (c == p2)
						break;
					index++;
				}
				index2 = index;

				if (p3 != null) {
					index = 0;
					foreach (var c in p2.Children) {
						if (c == p3)
							break;
						index++;
					}
					index3 = index;
				}
			}
		}

		private void GetIndex(RemoteSocketObject obj, out int index2, out int index3) {
			IFPropertyNodeItem p2 = obj.Parent;
			IFPropertyNodeItem p1 = p2.Parent;

			if (p2.Children != null)
				index3 = p2.Children.IndexOf(obj);
			else
				index3 = -1;
			index2 = p1.Children.IndexOf(p2);
		}

		private void treeViewServer_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
			if (e.OldValue == e.NewValue)
				return;

			if (index1 != -1)
				if (socketModels[index1] != null)
					((SocketModel)socketModels[index1]).SetSendText(index2, index3, sendText.Text);

			GetIndex();
			UpdateView();
        }

		private void Refresh() {
			if (status.index1 == -1 || status.index2 == -1) {
				return;
			}

			string sendStr = "";
			string recvStr = "";

			sendStr = ((SocketModel)socketModels[status.index1]).GetSendText(status.index2, status.index3);
			recvStr = ((SocketModel)socketModels[status.index1]).GetText(status.index2, status.index3);

			recvText.Text = recvStr;
			sendText.Text = sendStr;
        }

		private void RefreshTree() {
			IFPropertyNodeItem item = treeViewServer.SelectedItem as IFPropertyNodeItem;
			treeViewServer.ItemsSource = null;
			treeViewServer.ItemsSource = socketModels;
			var lastItem = FocusManager.GetFocusedElement(this);
			treeViewServer.Focus();
			lastItem.Focus();
		}

		private void RefreshDisplay() {
			RefreshTree();
		}

		private void RemoveRemoteItem() {
			if (index1 < 0)
				return;
			
			GetIndex(ObjNeedRm, out index2, out index3);
			if (index2 >= 0 && ObjNeedRm.Parent is TcpServerSocketObject && socketModels[index1] == ObjNeedRm.Parent)
				ReSelectItem((SocketModel)ObjNeedRm.Parent.Parent, index2, -2);
			else {
				((TCPClientModel)ObjNeedRm.Parent.Parent).Disconnect(index2);
				RefreshTree(); //TODO: 这里转换TCP client显示状态
			}
			UpdateView();
		}

		private RemoteSocketObject ObjNeedRm = null;
		private void RecvMsg(object sender, RecvEventArgs e) {
			if (!e.IsErr) {
                if (e.RemoteSocketObj.Parent is UdpServerSocketObject) {
					SocketObject obj = (SocketObject)e.RemoteSocketObj.Parent;
					bool isSelected = false;
					if (socketModels[index1] == (SocketModel)obj.Parent && obj.Parent.Children[index2] == obj)
						isSelected = true;
					((SocketModel)obj.Parent).UpdateSubTreeView(obj, isSelected);
				}
				Dispatcher.Invoke(Refresh);
				Dispatcher.Invoke(RefreshTree);
            } else {
				ObjNeedRm = e.RemoteSocketObj;
				Dispatcher.Invoke(RemoveRemoteItem);
			}
		}

        private void acceptMsg(object sender, AcceptEventArgs e) {
			if (e.SocketObj != null) {
				bool isSelected = false;
				if (socketModels[index1] == (SocketModel)e.SocketObj.Parent && e.SocketObj.Parent.Children[index2] == e.SocketObj)
					isSelected = true;
				((SocketModel)e.SocketObj.Parent).UpdateSubTreeView(e.SocketObj, isSelected);
			}
			Dispatcher.Invoke(RefreshTree);
		}

		private void recvText_TextChanged(object sender, TextChangedEventArgs e) {
			recvText.ScrollToEnd();
		}

		private void Button_Exit_Click(object sender, RoutedEventArgs e) {
			Close();
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			AsynchronousSocketListener.recvEvent -= RecvMsg;
			AsynchronousSocketListener.acceptEvent -= acceptMsg;
			foreach (var c in socketModels)
				if (c != null)
					((SocketModel)c).CloseAll();
		}

		private void BTSStartListen_Click(object sender, RoutedEventArgs e) {
			TCPServerSocket p1 = socketModels[status.index1] as TCPServerSocket;
			if (p1 == null || status.index2 < 0)
				return;

			TcpServerSocketObject p2 = (TcpServerSocketObject)p1.Children[status.index2];

			if (p2.IsListening) {
				p1.StopListening(status.index2);
			} else {
				p1.StartListening(status.index2);
            }
			RefreshDisplay();
		}

		private void BTCConnect_Click(object sender, RoutedEventArgs e) {
			SocketModel p1 = (SocketModel)socketModels[status.index1];

			if (p1 is TCPServerSocket) {
				DelNode(status.index1, status.index2, status.index3);
			} else if (p1 is TCPClientModel) {
				TcpClientSocketObject p2 = (TcpClientSocketObject)p1.Children[status.index2];

				if (p2.socket != null && p2.socket.Connected) {
					((TCPClientModel)p1).Disconnect(status.index2);
				} else {
					((TCPClientModel)p1).Connect(status.index2);
                }
            }

			RefreshDisplay();
		}

		private void helpBtn_Click(object sender, RoutedEventArgs e) {
			MessageBox.Show("Powered by God Y.", "Help:");
		}
	}

	internal class CurrentStatus {
		public int index1 = -1;
		public int index2 = -1;
		public int index3 = -1;
	}
}
