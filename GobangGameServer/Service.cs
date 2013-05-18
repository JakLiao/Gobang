using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace GobangGameServer
{
    class Service
    {
        private ListBox listbox;
        private delegate void AddItemDelegate(string str);
        private AddItemDelegate addItemDelegate;
        public Service(ListBox listbox)
        {
            this.listbox = listbox;
            addItemDelegate = new AddItemDelegate(AddItem);
        }
        /// <summary>在listbox中追加信息<</summary>
        /// <param name="str">要追加的信息</param>
        public void AddItem(string str)
        {
            if (listbox.InvokeRequired)
            {
                listbox.Invoke(addItemDelegate, str);
            }
            else
            {
                listbox.Items.Add(str);
                listbox.SelectedIndex = listbox.Items.Count - 1;
                listbox.ClearSelected();
            }
        }

        /// <summary>异步发送message给user</summary>
        public void AsyncSendToOne(User user, string message)
        {
            SendToOneDelegate d = new SendToOneDelegate(SendToOne);
            IAsyncResult result = d.BeginInvoke(user, message, null, null);
            while (result.IsCompleted == false)
            {
                Thread.Sleep(250);
            }
            d.EndInvoke(result);
        }
        public delegate void SendToOneDelegate(User user, string str);
        /// <summary>向某个客户端发送信息</summary>
        /// <param name="gameTable">指定客户</param>
        /// <param name="gameTable">信息</param>
        public void SendToOne(User user, string str)
        {
            try
            {
                user.sw.WriteLine(str);
                user.sw.Flush();
                AddItem(string.Format("向{0}发送{1}", user.userName, str));
            }
            catch
            {
                AddItem(string.Format("向{0}发送信息失败", user.userName));
            }
        }
        /// <summary>向同一桌的两个客户端发送信息</summary>
        /// <param name="gameTable">指定游戏桌</param>
        /// <param name="str">信息</param>
        public void SendToBoth(GameTable gameTable, string str)
        {
            for (int i = 0; i < 2; i++)
            {
                if (gameTable.gamePlayer[i].someone == true)
                {
                    AsyncSendToOne(gameTable.gamePlayer[i].user, str);
                }
            }
        }
        /// <summary>向所有客户端发送信息</summary>
        /// <param name="gameTable">客户列表</param>
        /// <param name="gameTable">信息</param>
        public void SendToAll(System.Collections.Generic.List<User> userList, string str)
        {
            for (int i = 0; i < userList.Count; i++)
            {
                AsyncSendToOne(userList[i], str);
            }
        }
    }
}
