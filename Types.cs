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
	public class SocketObject : IFPropertyNodeItem {
		public SocketObject() {
			Children = remoteSocketObjs;
		}

		public Socket socket = null;
		public ManualResetEvent doneEvent = new ManualResetEvent(true);
		public IPEndPoint remoteIpEP;
		public List<IFPropertyNodeItem> remoteSocketObjs = new List<IFPropertyNodeItem>();

		// 接口实现
		public string Icon { get; set; }
		public string DisplayName { get; set; }
		public string Name { get; set; }
		public bool Expanded { get; set; }
		public bool Selected { get; set; }
		public IFPropertyNodeItem Parent { get; set; }

		public List<IFPropertyNodeItem> Children { get; set; }
	}

	public class TcpClientSocketObject : SocketObject {
		public TcpClientSocketObject() {
			Icon = IconResources.ICON_CLIENT;
			Name = "TCPClientItem";
		}

		public ManualResetEvent connectDone = new ManualResetEvent(false);
	}

	public class UdpServerSocketObject : SocketObject {
		public UdpServerSocketObject() {
			Icon = IconResources.ICON_SERVER;
			Name = "UDPServerItem";
		}

		public RemoteSocketObject listenSocketObj = null;
	}

	public class UdpClientSocketObject : SocketObject {
		public UdpClientSocketObject() {
			Icon = IconResources.ICON_CLIENT;
			Name = "UDPClientItem";
		}
	}

	public class TcpServerSocketObject : SocketObject {
		public TcpServerSocketObject() {
			Icon = IconResources.ICON_SERVER;
			Name = "TCPServerItem";
		}

		public bool IsListening = false;
		public IAsyncResult acceptAr = null;
	}

	public class UdpGroupSocketObject : SocketObject {
		public UdpGroupSocketObject() {
			Icon = IconResources.ICON_GROUP;
			Name = "UDPGroupItem";
		}
	}

	/// <summary>
	/// 报文数据
	/// </summary>
	public class SocketData {
		public DateTime time;
		public int type; // 0 = 接收的数据， 1 = 发送的数据
		public byte[] data = null;
	}

	// 远端信息
	public delegate string genSendStringDelegate(SocketData sendData);
	public delegate string genRecvStringDelegate(SocketData recvData);

	public class RemoteSocketObject : IFPropertyNodeItem{
		public RemoteSocketObject() {
			Children = new List<IFPropertyNodeItem>();
			Name = "RemoteSocketItem";
			Icon = IconResources.ICON_UNKNOWN;
		}

		public Socket socket = null;
		public EndPoint remoteEP = null;
		public const int BufferSize = 1024;
		public byte[] buffer = new byte[BufferSize];
		public List<SocketData> dataList = new List<SocketData>();
		public StringBuilder sb = new StringBuilder(); // 文本形式的dataList
		public string sendTextTmp; // 临时保存的发送文本
		public genSendStringDelegate genSendString = AsynchronousSocketListener.GenSendString;
		public genRecvStringDelegate genRecvString = AsynchronousSocketListener.GenRecvString;

		// 接口实现
		public string Icon { get; set; }
		public string DisplayName { get; set; }
		public string Name { get; set; }
		public bool Expanded { get; set; }
		public bool Selected { get; set; }
		public IFPropertyNodeItem Parent { get; set; }

		public List<IFPropertyNodeItem> Children { get; set; }
	}

	// 扩展函数
	public static class ByteArrayExtensions {
		public static void CopyToEx(this byte[] src, byte[] dst, int len) {
			for (int i = 0; i < len; i++) {
				dst[i] = src[i];
			}
		}

		public static byte[] GetBytes(this byte[] src, int len) {
			byte[] outBytes = new byte[len];

			for (int i = 0; i < len; i++) {
				outBytes[i] = src[i];
			}

			return outBytes;
		}

		public static byte[] GetBytes(this byte[] src) {
			byte[] outBytes = new byte[src.Length];

			src.CopyTo(outBytes, 0);

			return outBytes;
		}
	}
}
