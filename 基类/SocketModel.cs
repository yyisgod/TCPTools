using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TCPTools {
	/// <summary>
	/// 用来作为不同服务器、客户端处理类的基类
	/// </summary>
	abstract public class SocketModel : IFPropertyNodeItem {
		protected List<IFPropertyNodeItem> socketObjs = new List<IFPropertyNodeItem>();

		public SocketModel() {
			Name = "UnKnown";
			Icon = IconResources.ICON_UNKNOWN;
			DisplayName = "UnKnown";
			Children = socketObjs;
			Parent = null;
		}

		public void UpdateTreeView(IFPropertyNodeItem item, bool expend = false) {
			if (expend) {
				item.Expanded = true;
				item.Children.Last().Selected = true;
			}
		}

		public static string GetDisplayName(IPEndPoint ipEP) {
			return ipEP.Address.ToString() + "[" + ipEP.Port + "]";
		}

		public static string GetDisplayName(EndPoint ep) {
			return GetDisplayName((IPEndPoint)ep);
        }

		// 自动选择、展开子节点
		public virtual void UpdateSubTreeView(IFPropertyNodeItem item, bool selected = false) {
			item.Parent.Expanded = item.Expanded = true;
			if (selected && item.Children.Count == 1)
				item.Children[0].Selected = true;
		}

		// 删除2级节点时删除所有3级节点
		protected virtual void	DelAllRemoteSocketObj(int index2) {

		}

		// 断开2级节点
		protected virtual void DisconnectSocketObj(int index2) {

		}

		// 删除2级节点
		protected virtual bool DelSocketObj(int index2) {
			try {
				DelAllRemoteSocketObj(index2);
				DisconnectSocketObj(index2);
				SocketObject tObj = (SocketObject)socketObjs[index2];
				tObj.socket.Close();
			} catch (Exception) {
				return false;
			} finally {
				socketObjs.RemoveAt(index2);
			}

			return true;
		}

		// 删除3级节点
		protected virtual bool DelRemoteSocketObj(int index2, int index3) {
			try {
				RemoteSocketObject obj = (RemoteSocketObject)((SocketObject)socketObjs[index2]).remoteSocketObjs[index3];
				obj.socket.Shutdown(SocketShutdown.Both);
				obj.socket.Disconnect(false);
				obj.socket.Close();
			} catch(Exception) {
				return false;
			} finally {
				((SocketObject)socketObjs[index2]).remoteSocketObjs.RemoveAt(index3);
			}

			return true;
		}

		// 删除某节点
		public bool DelSocket(int index2, int index3) {
			if (index2 >= socketObjs.Count || index2 < 0)
				return false;

			if (index3 == -1) {
				return DelSocketObj(index2);
			} else if (index3 < ((SocketObject)socketObjs[index2]).remoteSocketObjs.Count) {
				return DelRemoteSocketObj(index2, index3);
			}

			return true;
		}

		// 发送数据
		public virtual void SendPacket(int index2, int index3, string text) {
			if (index2 != -1 && index2 < socketObjs.Count
				&& index3 != -1 && index3 < ((SocketObject)socketObjs[index2]).remoteSocketObjs.Count) {
				byte[] sendBytes = Encoding.ASCII.GetBytes(text);
				RemoteSocketObject remoteSocketObj = (RemoteSocketObject)((SocketObject)socketObjs[index2]).remoteSocketObjs[index3];
				remoteSocketObj.socket.SendTo(sendBytes , remoteSocketObj.remoteEP);

				SocketData sendData = new SocketData();
				sendData.time = DateTime.Now;
				sendData.type = 1;
				sendData.data = sendBytes.GetBytes();
				remoteSocketObj.dataList.Add(sendData);
				remoteSocketObj.sb.Append(remoteSocketObj.genSendString(sendData));

				SetSendText(index2, index3, ""); // 清空待发数据
			}
		}

		// 获取接收的文本
		public virtual string GetText(int index2, int index3) {
			if (index2 != -1 && index2 < socketObjs.Count
				&& index3 != -1 && index3 < ((SocketObject)socketObjs[index2]).remoteSocketObjs.Count) {
				return ((RemoteSocketObject)((SocketObject)socketObjs[index2]).remoteSocketObjs[index3]).sb.ToString();
			} else
				return null;
		}

		// 设置暂存的发送文本
		public virtual void SetSendText(int index2, int index3, string sendText) {
			if (index2 != -1 && index2 < socketObjs.Count
				&& index3 != -1 && index3 < ((SocketObject)socketObjs[index2]).remoteSocketObjs.Count) {
				((RemoteSocketObject)((SocketObject)socketObjs[index2]).remoteSocketObjs[index3]).sendTextTmp = sendText;
			}
		}

		// 获取暂存的发送文本
		public virtual string GetSendText(int index2, int index3) {
			if (index2 != -1 && index2 < socketObjs.Count
				&& index3 != -1 && index3 < ((SocketObject)socketObjs[index2]).remoteSocketObjs.Count) {
				return ((RemoteSocketObject)((SocketObject)socketObjs[index2]).remoteSocketObjs[index3]).sendTextTmp;
			} else
				return null;
		}

		// 关闭所有2级节点
		public void CloseAll() {
			int num = socketObjs.Count;
			while (num-- > 0) {
				DelSocketObj(0);
            }
		}

		// 接口实现
		public string Icon { get; set; }
		public string DisplayName { get; set; }
		public string Name { get; set; }
		public bool Expanded { get; set; }
		public bool Selected { get; set; }
		public IFPropertyNodeItem Parent { get; set; }

		public List<IFPropertyNodeItem> Children { get; set; }
	}
}
