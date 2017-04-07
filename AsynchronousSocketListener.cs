using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading;

namespace TCPTools {
	// 事件参数
	internal class RecvEventArgs : EventArgs {
		private readonly RemoteSocketObject remoteSocketObj;
		private readonly bool isErr;

		public RecvEventArgs(RemoteSocketObject socketObj, bool isErr = false) {
			remoteSocketObj = socketObj;
			this.isErr = isErr;
		}

		public RemoteSocketObject RemoteSocketObj { get { return remoteSocketObj; } }
		public bool IsErr { get { return isErr; } }
    }

	internal class AcceptEventArgs : EventArgs {
		private readonly SocketObject socketObj;

		public AcceptEventArgs(SocketObject socketObj) {
			this.socketObj = socketObj;
		}

		public SocketObject SocketObj { get { return socketObj; } }
	}

	// 静态函数
	internal static class AsynchronousSocketListener {
		// 事件
        public static event EventHandler<RecvEventArgs> recvEvent;
		public static event EventHandler<AcceptEventArgs> acceptEvent;

		private static void OnRecv(RecvEventArgs e) {
			EventHandler<RecvEventArgs> temp = Volatile.Read(ref recvEvent);

			if (temp != null) temp(null, e);
		}

		public static void OnAccept(AcceptEventArgs e) {
			EventHandler<AcceptEventArgs> temp = Volatile.Read(ref acceptEvent);

			if (temp != null) temp(null, e);
		}

		internal static void StartRead(SocketObject socketObj) {
			RemoteSocketObject remoteSocketObj = new RemoteSocketObject();
			remoteSocketObj.socket = socketObj.socket;  // 除了TCP服务器，都是这样
			remoteSocketObj.Parent = socketObj;
			remoteSocketObj.remoteEP = socketObj.remoteIpEP;
			socketObj.remoteSocketObjs.Add(remoteSocketObj);
			remoteSocketObj.socket.BeginReceive(remoteSocketObj.buffer, 0, RemoteSocketObject.BufferSize, 0,
			new AsyncCallback(AsynchronousSocketListener.readCallback), remoteSocketObj);
		}

		// callback函数
		internal static void UDPServerReadCallback(IAsyncResult ar) {
			RemoteSocketObject listenSocketObj = (RemoteSocketObject)ar.AsyncState;
			Socket handler = listenSocketObj.socket;
			EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

			try {
				int recvCnt = handler.EndReceiveFrom(ar, ref remoteEndPoint);

				if (recvCnt > 0) {
					SocketData recvData = new SocketData();
					recvData.time = DateTime.Now;
					recvData.type = 0;
					recvData.data = listenSocketObj.buffer.GetBytes(recvCnt);
					
					UdpServerSocketObject parent = (UdpServerSocketObject)listenSocketObj.Parent;
					RemoteSocketObject dstObj = null;
					foreach (var c in parent.remoteSocketObjs)
						if (((RemoteSocketObject)c).remoteEP.ToString() == remoteEndPoint.ToString()) {
							dstObj = (RemoteSocketObject)c;
							break;
						}
					if (dstObj == null) {
						dstObj = new RemoteSocketObject();
						dstObj.socket = handler;
						dstObj.Parent = listenSocketObj.Parent;
						dstObj.remoteEP = remoteEndPoint;
                        dstObj.DisplayName = SocketModel.GetDisplayName(dstObj.remoteEP);
						dstObj.Icon = IconResources.ICON_CLIENT;
						((SocketObject)dstObj.Parent).remoteSocketObjs.Add(dstObj);
					}

					dstObj.dataList.Add(recvData);
					dstObj.sb.Append(dstObj.genRecvString(recvData));

					OnRecv(new RecvEventArgs(dstObj));

					EndPoint tempEP = (EndPoint)(new IPEndPoint(IPAddress.Any, 0));
					handler.BeginReceiveFrom(listenSocketObj.buffer, 0, RemoteSocketObject.BufferSize, 0,
						ref tempEP, new AsyncCallback(UDPServerReadCallback), listenSocketObj);
				} else {
				}
			} catch (Exception) {
			}
		}

		internal static void readCallback(IAsyncResult ar) {
			RemoteSocketObject remoteSocketObj = (RemoteSocketObject)ar.AsyncState;
			Socket handler = remoteSocketObj.socket;

			try {
				int recvCnt = handler.EndReceive(ar);

				if (recvCnt > 0) {
					SocketData recvData = new SocketData();
					recvData.time = DateTime.Now;
					recvData.type = 0;
					recvData.data = remoteSocketObj.buffer.GetBytes(recvCnt);
					remoteSocketObj.dataList.Add(recvData);
					string typeStr = recvData.type == 0 ? "[接收]" : "[发送]";
					remoteSocketObj.sb.Append(recvData.time.ToShortTimeString() + " " + typeStr + "：" + Encoding.ASCII.GetString(recvData.data, 0, recvCnt) + "\n");
					OnRecv(new RecvEventArgs(remoteSocketObj));

					handler.BeginReceive(remoteSocketObj.buffer, 0, RemoteSocketObject.BufferSize, 0,
						new AsyncCallback(readCallback), remoteSocketObj);
				} else {
					if (remoteSocketObj.Parent as TcpServerSocketObject != null)
						remoteSocketObj.Parent.Children.Remove(remoteSocketObj);
					remoteSocketObj.socket.Disconnect(true);
					OnRecv(new RecvEventArgs(remoteSocketObj, true));
				}
			} catch (Exception) {
			}
		}

		internal static void StartListening(object state) {
			TcpServerSocketObject socketObj = (TcpServerSocketObject)state;

			try {
				socketObj.socket.Listen(255);
				socketObj.IsListening = true;
                while (socketObj.IsListening) {
					socketObj.doneEvent.Reset();

					socketObj.socket.BeginAccept(
						new AsyncCallback(acceptCallback),
						socketObj);

					socketObj.doneEvent.WaitOne();
				}
			} catch (Exception) { }
		}

		internal static void acceptCallback(IAsyncResult ar) {
			TcpServerSocketObject socketObj = (TcpServerSocketObject)ar.AsyncState;

			try {
				Socket remoteSocket = socketObj.socket.EndAccept(ar);

				socketObj.doneEvent.Set();

				RemoteSocketObject remoteSocketObj = new RemoteSocketObject();
				remoteSocketObj.socket = remoteSocket;
				remoteSocketObj.Parent = socketObj;
				remoteSocketObj.remoteEP = remoteSocket.RemoteEndPoint;
                remoteSocketObj.DisplayName = SocketModel.GetDisplayName(remoteSocketObj.remoteEP);
				remoteSocketObj.Icon = IconResources.ICON_CLIENT;
				socketObj.remoteSocketObjs.Add(remoteSocketObj);
				OnAccept(new AcceptEventArgs(socketObj));
                remoteSocket.BeginReceive(remoteSocketObj.buffer, 0, RemoteSocketObject.BufferSize, 0,
					new AsyncCallback(readCallback), remoteSocketObj);
			} catch (Exception) { }
		}

		internal static void connectCallback(IAsyncResult ar) {
			TcpClientSocketObject socketObj = (TcpClientSocketObject)ar.AsyncState;

			try {
				socketObj.socket.EndConnect(ar);

				socketObj.connectDone.Set();
			} catch (Exception) { }
		}

		// 默认文本显示
		internal static string GenRecvString(SocketData recvData) {
			string typeStr = recvData.type == 0 ? "[接收]" : "[发送]";
			return (recvData.time.ToShortTimeString() + " " + typeStr + "：" + Encoding.ASCII.GetString(recvData.data, 0, recvData.data.Length) + "\n");
		}

		internal static string GenSendString(SocketData sendData) {
			string typeStr = sendData.type == 0 ? "[接收]" : "[发送]";
			return (sendData.time.ToShortTimeString() + " " + typeStr + "：" + Encoding.ASCII.GetString(sendData.data, 0, sendData.data.Length) + "\n");
		}
	}
}