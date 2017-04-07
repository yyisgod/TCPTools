using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TCPTools {
	public interface IFPropertyNodeItem {
		string Icon { get; set; }
		string DisplayName { get; set; }
		string Name { get; set; }
		bool Expanded { get; set; }
		bool Selected { get; set; }
		IFPropertyNodeItem Parent { get; set; }

		List<IFPropertyNodeItem> Children {
			get; set;
		}

	}

	public class PropertyNodeItem : IFPropertyNodeItem {

		public PropertyNodeItem() {
			Children = new List<IFPropertyNodeItem>();
		}

		public string Icon { get; set; }
		public string DisplayName { get; set; }
		public string Name { get; set; }
		public bool Expanded { get; set; }
		public bool Selected { get; set; }
		public IFPropertyNodeItem Parent { get; set; }

		public List<IFPropertyNodeItem> Children { get; set; }
	}

	public static class IconResources {
		public static readonly string ICON_SERVER = "资源/服务器.png";
		public static readonly string ICON_CLIENT = "资源/客户端.png";
		public static readonly string ICON_UNKNOWN = "资源/未知.png";
		public static readonly string ICON_GROUP = "资源/组播.png";

	}
}
