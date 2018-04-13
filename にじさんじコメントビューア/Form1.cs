using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using Codeplex.Data;
using System.Threading;

namespace get_youtubelive_comments_gui
{
    public partial class Form1 : Form
    {
        string channel_id = null;
        string channel_title = null;
        string video_id = null;
        string video_title = null;
        string api_key = null;
        string live_chat_id = null;

        bool show_owners_message = true;

        List<string> white_list = new List<string>();  

        dynamic messages_object = null;
        Dictionary<string, object[]> messages_dictionary = new Dictionary<string, object[]>();
        List<string> messages_ids = new List<string>();
        List<string> messages_ids_old = new List<string>();
        List<string> messages_ids_diff = new List<string>();

        public class ItemSet
        {
            // DisplayMemberとValueMemberにはプロパティで指定する仕組み
            public String ItemDisp { get; set; }
            public String ItemValue { get; set; }

            // プロパティをコンストラクタでセット
            public ItemSet(String d, String v)
            {
                ItemDisp = d;
                ItemValue = v;
            }
        }

        public Form1()
        {
            InitializeComponent();

            try
            {
                // csvファイルを開く
                using (var sr = new StreamReader(@"white_list.csv", System.Text.Encoding.GetEncoding("shift_jis")))
                {
                    // ストリームの末尾まで繰り返す
                    for (int i = 0; !sr.EndOfStream; i++)
                    {
                        // ファイルから一行読み込む
                        string line = sr.ReadLine();

                        listBox1.Items.Add(line);

                    }
                }
            }
            catch (Exception ex)
            {
                // ファイルを開くのに失敗したとき
                MessageBox.Show(ex.ToString(),
                                "エラー",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }

            List<ItemSet> src = new List<ItemSet>();
            src.Add(new ItemSet("月ノ美兎", "UCD-miitqNY3nyukJ4Fnf4_A"));/// 1つでItem１つ分となる
            src.Add(new ItemSet("樋口楓", "UCsg-YqdqQ-KFF0LNk23BY4A"));
            src.Add(new ItemSet("静凛", "UC6oDys1BGgBsIC3WhG1BovQ"));
            src.Add(new ItemSet("鈴谷アキ", "UCt9qik4Z-_J-rj3bKKQCeHg"));
            src.Add(new ItemSet("エルフのえる", "UCYKP16oMX9KKPbrNgo_Kgag"));
            src.Add(new ItemSet("モイラ", "UCvmppcdYf4HOv-tFQhHHJMA"));
            src.Add(new ItemSet("勇気ちひろ", "UCLO9QDxVL4bnvRRsz6K4bsQ"));
            src.Add(new ItemSet("渋谷ハジメ", "UCeK9HFcRZoTrvqcUCtccMoQ"));
            src.Add(new ItemSet("夕陽リリ", "UC48jH1ul-6HOrcSSfoR02fQ"));
            src.Add(new ItemSet("家長むぎ", "UC_GCs6GARLxEHxy1w40d6VQ"));
            src.Add(new ItemSet("剣持刀也", "UCv1fFr156jc65EMiLbaLImw"));
            src.Add(new ItemSet("物述有栖", "UCt0clH12Xk1-Ej5PXKGfdPA"));
            src.Add(new ItemSet("鈴鹿詩子", "UCwokZsOK_uEre70XayaFnzA"));
            src.Add(new ItemSet("野良猫", "UCBiqkFJljoxAj10SoP2w2Cg"));
            src.Add(new ItemSet("伏見ガク", "UCXU7YYxy_iQd3ulXyO-zC2w"));
            src.Add(new ItemSet("森中花咲", "UCtpB6Bvhs1Um93ziEDACQ8g"));
            src.Add(new ItemSet("ギルザレンIII世", "UCUzJ90o1EjqUbk2pBAy0_aw"));

            // ComboBoxに表示と値をセット
            comboBox1.DataSource = src;
            comboBox1.DisplayMember = "ItemDisp";
            comboBox1.ValueMember = "ItemValue";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            channel_id = comboBox1.SelectedValue.ToString();

            /*switch (comboBox1.SelectedValue.ToString().Length)
            {
                case 24:
                    channel_id = comboBox1.SelectedValue.ToString();
                    break;

                case 11:
                    video_id = comboBox1.Text;
                    break;

                default:
                    textBox1.Text = "接続に失敗しました。";
                    return;
            }*/

            api_key = textBox3.Text;

            if (video_id == null)
            {
                var video_id_request = WebRequest.Create("https://www.googleapis.com/youtube/v3/search?part=snippet&channelId=" + channel_id + "&type=video&eventType=live&key=" + api_key);

                try
                {
                    using (var video_id_response = video_id_request.GetResponse())
                    {
                        using (var video_id_stream = new StreamReader(video_id_response.GetResponseStream(), Encoding.UTF8))
                        {

                            var video_id_object = DynamicJson.Parse(video_id_stream.ReadToEnd());


                            string live = video_id_object.items[0].snippet.liveBroadcastContent;
                            if (live != "live")
                            {
                                textBox1.Text = "Error: ただいま配信は行われていません";
                                return;
                            }

                            video_id = video_id_object.items[0].id.videoId;

                            video_title = video_id_object.items[0].snippet.title;

                            channel_title = video_id_object.items[0].snippet.channelTitle;

                            /*var video_id_regex = new Regex("href=\"\\/watch\\?v=(.+?)\"", RegexOptions.IgnoreCase | RegexOptions.Singleline);

                            var video_id_match = video_id_regex.Match(video_id_stream.ReadToEnd());

                            if (!video_id_match.Success)
                            {
                                textBox1.Text = "Error: ストリーミングが見つかりませんでした";
                                return;
                            }

                            var index1 = video_id_match.Value.LastIndexOf('=') + 1;
                            var index2 = video_id_match.Value.LastIndexOf('"');

                            video_id = video_id_match.Value.Substring(index1, index2 - index1);*/
                        }
                    }
                }
                catch
                {
                    textBox1.Text = "Error: ストリーミングの検索に失敗しました";
                    return;
                }
            }

            var live_chat_id_request = WebRequest.Create("https://www.googleapis.com/youtube/v3/videos?part=liveStreamingDetails&id=" + video_id + "&key=" + api_key);

            try
            {
                using (var live_chat_id_response = live_chat_id_request.GetResponse())
                {
                    using (var live_chat_id_stream = new StreamReader(live_chat_id_response.GetResponseStream(), Encoding.UTF8))
                    {
                        var live_chat_id_object = DynamicJson.Parse(live_chat_id_stream.ReadToEnd());

                        live_chat_id = live_chat_id_object.items[0].liveStreamingDetails.activeLiveChatId;

                        

                        if (live_chat_id == null)
                        {
                            textBox1.Text = "Error: Live Chat IDの取得に失敗しました";
                            return;
                        }

                    }
                }
            }
            catch
            {
                textBox1.Text = "Error: Live Chat IDの取得に失敗しました";
                return;
            }

            textBox1.Text = "「" + channel_title + "」の「" + video_title + "」に接続しました\r\n";

            timer1.Tick += new EventHandler(get_Comment);
            timer1.Enabled = true;
            
        }

        private void get_Comment(object sender, EventArgs e)
        {
            {
                var messages_request = WebRequest.Create("https://www.googleapis.com/youtube/v3/liveChat/messages?part=snippet,authorDetails&liveChatId=" + live_chat_id + "&key=" + api_key);

                messages_object = null;

                try
                {
                    using (var messages_response = messages_request.GetResponse())
                    {
                        using (var messages_stream = new StreamReader(messages_response.GetResponseStream()))
                        {
                            messages_object = DynamicJson.Parse(messages_stream.ReadToEnd());
                        }
                    }
                }
                catch
                {
                    textBox1.Text = "Error: コメントの取得に失敗しました";
                }

                messages_ids.Clear();
                messages_dictionary.Clear();

                foreach (var value in messages_object.items)
                {
                    messages_ids.Add(value.id);

                    messages_dictionary.Add(value.id, new object[]
                    {
                    value.authorDetails.displayName,
                    value.snippet.textMessageDetails.messageText,
                    value.authorDetails.isChatOwner
                    });
                }

                messages_ids_diff = new List<string>(messages_ids);
                messages_ids_diff.RemoveAll(messages_ids_old.Contains);

                foreach (string username in listBox1.Items)
                {
                    white_list.Add(username);
                }

                foreach (var value in messages_ids_diff)
                {
                    if (show_owners_message || !Convert.ToBoolean(messages_dictionary[value][2]))
                    {
                        var message_sender = messages_dictionary[value][0];
                        var message_text = messages_dictionary[value][1];

                        if ((white_list.Contains(message_sender + "")) || !checkBox1.Checked)
                            textBox1.AppendText(message_sender + " : " + message_text + "\r\n");
                    }
                }

                messages_ids_old.Clear();
                messages_ids_old = new List<string>(messages_ids);

                messages_ids_diff.Clear();
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            int sel = listBox1.SelectedIndex;
            listBox1.Items.RemoveAt(sel);
            save_white_list();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            listBox1.Items.Add(textBox4.Text);
            save_white_list();
        }

        private void save_white_list()
        {
            try
            {
                // 保存用のファイルを開く
                using (StreamWriter sw = new StreamWriter(@"white_list.csv", false, Encoding.GetEncoding("shift_jis")))
                {

                    int rowCount = listBox1.Items.Count;

                    foreach (string row in listBox1.Items)
                    {
                        sw.WriteLine(row);
                    }
                }
            }
            catch (Exception ex)
            {
                // ファイルを開くのに失敗したとき
                MessageBox.Show("データが不正です.\n" + ex.ToString(),
                                "エラー",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }
    }
}
