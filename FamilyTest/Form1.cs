using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.IO;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FamilyTest
{
    public partial class Form1 : Form
    {
        //成员变量

        private string app_name="Family Helper";
        private string app_version = "3.1";
        private string app_author = "Majesty";
        private string app_rtime = "";

        private string app_title = null;

        //第几次刷新
        private int num;
        //刷新总时间
        private long alltime_ms;


        private String cookie;


	    //用来downloadstring
	    WebClient wb;
	    //用来计时
	    Stopwatch watch;

        //切分楼层用
        string[] string_sep = { "评论<em>", "</em></span>" };
        string[] string_sep2 = { "全部评论 <em>", "</em>条</p>" };

        //构造方法
        public Form1()
        {
            InitializeComponent();

            //初始化一些东西
            timer1.Enabled=false;
            //this.appname = "Family Helper v3.0 (for new Family ) - by Majesty ; release time: " + 
            app_rtime = System.IO.File.GetLastWriteTime(this.GetType().Assembly.Location) + "  ";
            app_title = this.app_name + " v" + app_version + " - by " + app_author + " ; release date: " + app_rtime;
            this.Text = app_title;

			num=0;
            wb = new System.Net.WebClient();
            wb.Encoding = System.Text.Encoding.UTF8;
			watch=new Stopwatch();
        }

        //***********************************************************************
        /**
         * 以下是抢楼主程序
        */

        //开始刷新
        private void button_start_Click(object sender, EventArgs e)
        {
            //textbox内容校验
            if (textBox_articleid.Text.Trim() == "" || textBox_cookie.Text.Trim() == "")
            {
                MessageBox.Show("文章id，cookie不能为空！","警告",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                return;
            }

            //设置各种控件只读
            textBox_articleid.Enabled = false;
            textBox_cookie.Enabled = false;
            textBox_timemin.Enabled = false;
            textBox_timemax.Enabled = false;
            textBox_extra.Enabled = false;
            textBox_message.Enabled = false;

            textBox_tailfloor.Enabled = false;
            textBox_timesfloor.Enabled = false;
            textBox_specialfloor.Enabled = false;

            checkBox_ifcommit.Enabled = false;
            checkBox_ifcontinue.Enabled = false;
            checkBox_tailfloor.Enabled = false;
            checkBox_timesfloor.Enabled = false;
            checkBox_specialfloor.Enabled = false;
            checkBox_forcefloor.Enabled = false;

            button_start.Enabled = false;

            //拿到时钟周期

	        int valmin=Convert.ToInt32(textBox_timemin.Text,10);
	        int valmax=Convert.ToInt32(textBox_timemax.Text,10);
	        //最低不低于1ms
            if (valmin < 1)
            {
                valmin = 1;
                textBox_timemin.Text = "1";
	        }
            if (valmax < valmin)
            {
                valmax = valmin;
                textBox_timemax.Text = textBox_timemin.Text;
	        }

	        //初始time_val设定；
            timer1.Interval = valmin;
	        timer1.Enabled=true;
            this.alltime_ms = 0L;
            this.num = 0;

            //设置全局cookie
            this.cookie = textBox_cookie.Text;

        }
        //停止刷新
        private void button_stop_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            textBox_articleid.Enabled = true;
            textBox_cookie.Enabled = true;
            textBox_timemin.Enabled = true;
            textBox_timemax.Enabled = true;
            textBox_extra.Enabled = true;
            textBox_message.Enabled = true;

            textBox_tailfloor.Enabled = true;
            textBox_timesfloor.Enabled = true;
            textBox_specialfloor.Enabled = true;

            checkBox_ifcommit.Enabled = true;
            checkBox_ifcontinue.Enabled = true;
            checkBox_tailfloor.Enabled = true;
            checkBox_timesfloor.Enabled = true;
            checkBox_specialfloor.Enabled = true;
            checkBox_forcefloor.Enabled = true;

            button_start.Enabled = true;
        }
        //通过chrome打开
        private void button_open2chrome_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process np=new System.Diagnostics.Process();

	        np.StartInfo.FileName="chrome";
            np.StartInfo.Arguments = "http://chanpin.family.baidu.com/article/" + textBox_articleid.Text;
            np.Start();
        }

        //时钟事件
        private void timer1_Tick(object sender, EventArgs e)
        {
            //wb的实例化在init里进行

	        try{
		        //1,读出楼数
		        watch.Start();
                label_floors.Text = getFloorNew(textBox_articleid.Text);
                this.Text = app_name + " - 第" + (++this.num) + "次刷新";
                watch.Stop();

		        //2,判断是否符合秒杀条件

		        int fl=Convert.ToInt32(label_floors.Text);
		        if(this.isFloorOK(fl)==true){
                    textBox_hasgot.Text=textBox_hasgot.Text+(fl+1)+",";
			        
			        /// FlashWindow(this->Handle,true);
			        //选了自动提交
			        if(checkBox_ifcommit.Checked==true){

				        ///*******这里模拟一次post*******
				        //用来post
				        HttpWebRequest request;
				        //初始化httpwebrequest对象
                        String post_url = "http://chanpin.family.baidu.com/addComment";
				        request=(HttpWebRequest)WebRequest.Create(post_url);
				        request.Method="POST";
                        request.ServicePoint.Expect100Continue = false;
				        request.ContentType="application/x-www-form-urlencoded; charset=UTF-8";
				        request.UserAgent="Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.65 Safari/537.36";
				        request.CookieContainer=new CookieContainer();
				        //填写cookie
                        request.CookieContainer.Add(new Cookie("express.sid", textBox_cookie.Text, "/", "chanpin.family.baidu.com"));
				        //填写post数据

				        String data_str="articleId="+textBox_articleid.Text+
                            "&replyToComment=&content=" + textBox_message.Text;
				        
                        if(textBox_extra.Text!=""){
                            data_str += "&imageSrc=" + textBox_extra.Text;
				        }

				        byte[] data = (Encoding.GetEncoding("utf-8")).GetBytes(data_str);
				        request.ContentLength=data.Length;

				        request.GetRequestStream().Write(data,0,data.Length);
				
				        HttpWebResponse res=(HttpWebResponse)request.GetResponse();


				        Stream st=res.GetResponseStream();
				        Encoding en=Encoding.GetEncoding("utf-8");
				        StreamReader st_r=new StreamReader(st,en);


				        char[] read = new char[512];
				        // Reads 256 characters at a time.
				        int count = st_r.Read( read, 0, 512 );
				        StringBuilder sb=new StringBuilder();
				        while ( count > 0 ){
					        // Dumps the 256 characters on a String* and displays the String* to the console.
					        String str = new String( read,0,count );
					        sb.Append(str);
					        count = st_r.Read( read, 0, 512);
				        }
		
				        textBox_recieved.Text=sb.ToString();
                        
			        }

			        //如果持续没有被选中，则这里停止，为优化，放在后面
			        if(checkBox_ifcontinue.Checked==false)
				        this.button_stop_Click(sender,e);

		        }

	        }
	        catch (Exception err){
		        this.Text="Error";
		        //MessageBox.Show(err.Message);
                textBox_recieved.Text = err.Message;
	        }

	        //重设timeer_val
	        int val=Convert.ToInt32(textBox_timemin.Text,10);
	        int valmax=Convert.ToInt32(textBox_timemax.Text,10);
	        Random ran=new Random(timer1.Interval);
	        timer1.Interval=ran.Next(val,valmax);
	
	        //停止计时器并且显示
            //加到总时间里
            alltime_ms += watch.ElapsedMilliseconds;
	        label_speed.Text=watch.ElapsedMilliseconds.ToString()+"ms     平均时间:"+((double)alltime_ms/(double)this.num);
            
	        watch.Reset();
        }

        //获取当前楼层，通过正文页面（废弃） - OK
        private String getFloorOld(String id){

            String ret = wb.DownloadString("http://chanpin.family.baidu.com/article/" + id);
            String[] arr = ret.Split(string_sep,5,StringSplitOptions.None);
	        //2,返回数据不合法，不停止时钟并返回；下一轮时钟
            if (arr.Length != 5) {
                return "-2";
            }
	        return arr[3];
        }
        //获取当前楼层（new)，通过评论接口 - OK
        private String getFloorNew(String id)
        {
            HttpWebRequest request;
            //初始化httpwebrequest对象
            String post_url = "http://chanpin.family.baidu.com/commentList";
            request = (HttpWebRequest)WebRequest.Create(post_url);
            request.Method = "POST";

            request.ServicePoint.Expect100Continue = false;

            //请求头部
            request.Accept = "*/*";
            request.Timeout = 300;  //请求超时
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.Referer = "http://chanpin.family.baidu.com/article/" + id;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.93 Safari/537.36";


            request.Headers.Add("Accept-Encoding", "gzip, deflate");
            request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8");
            request.Headers.Add("Origin", "http://chanpin.family.baidu.com");
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");

            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(new Cookie("express.sid", this.cookie, "/", ".family.baidu.com"));


            byte[] data = (Encoding.GetEncoding("utf-8")).GetBytes("pageNo=9999&articleId=" + id);
            request.ContentLength = data.Length;
            try
            {
                request.GetRequestStream().Write(data, 0, data.Length);

                HttpWebResponse res = (HttpWebResponse)request.GetResponse();

                Stream st = res.GetResponseStream();
                Encoding en = Encoding.GetEncoding("utf-8");
                StreamReader st_r = new StreamReader(st, en);
                string res_str = st_r.ReadToEnd();
                st.Close();
                st_r.Close();
                string ret = res_str.Split(string_sep2, StringSplitOptions.None)[1];
                return ret;
            }
            catch
            {
                Console.Out.Write("1");
                return "-2";
            }
        }

        //自动修正cookie
        private void textBox_cookie_Leave(object sender, EventArgs e)
        {
            string str = textBox_cookie.Text.Trim();
            textBox_cookie.Text = str;
        }
        //cookie验证
        private void button_cookievalid_Click(object sender, EventArgs e)
        {
            string cookie = textBox_cookie.Text.Trim();
            if (cookie.Equals(""))
            {
                return;
            }


            HttpWebRequest request;
            //初始化httpwebrequest对象
            request = (HttpWebRequest)WebRequest.Create("http://chanpin.family.baidu.com/getProductUserInfo");
            request.Method = "POST";

            //request.ServicePoint.Expect100Continue = false;

            //请求头部
            request.Accept = "*/*";

            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.93 Safari/537.36";
            request.Headers.Add("Accept-Encoding", "gzip, deflate");
            request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8");
            request.Headers.Add("Origin", "http://chanpin.family.baidu.com");
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");

            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(new Cookie("express.sid", cookie, "/", ".family.baidu.com"));

            HttpWebResponse res = (HttpWebResponse)request.GetResponse();

            Stream st = res.GetResponseStream();
            Encoding en = Encoding.GetEncoding("utf-8");
            StreamReader st_r = new StreamReader(st, en);
            string res_str = st_r.ReadToEnd();
            st.Close();
            st_r.Close();

            ;
            if (res_str.Equals("{\"isLogin\":false}"))
            {
                MessageBox.Show("验证结果：无效的Cookie！");
            }
            else
            {
                MessageBox.Show("验证结果：有效~");
            }
        }

        //判断是否当前楼层+1为命中楼层（核心）- OK
        private bool isFloorOK(int n){
	        int aim=n+1;
	        String str_aim=Convert.ToString(aim);
	        //尾数楼
	        if(checkBox_tailfloor.Checked==true){
		        if(str_aim.EndsWith(textBox_tailfloor.Text)){
			        return true;
		        }
	        }
	        //倍数楼
	        if(checkBox_timesfloor.Checked==true){
                int times = Convert.ToInt32(textBox_timesfloor.Text, 10);
		        if(times!=0 && aim%times==0){
			        return true;
		        }
	        }
            
	        //特定楼
	        if(checkBox_specialfloor.Checked==true){
                String str = textBox_specialfloor.Text.Trim();
		        if(str.StartsWith(str_aim+",") || str.EndsWith(","+str_aim) || str.Contains(","+str_aim+",") || str==str_aim){
			        return true;
		        }

	        }
	        //必中
	        if(checkBox_forcefloor.Checked==true)
		        return true;
	        return false;
        }



        //***********************************************************************
        /**
         * 以下是最新贴刷新
        */
        //刷新最热贴
        private void button1_Click(object sender, EventArgs e)
        {

            //是hot还是new
            string type = radioButton_hot.Checked?"hot":"new";
            
            string url = "http://chanpin.family.baidu.com/getArticleList";
            string postString = "pageNo=1&pageSize=40&category=&type=" + type;
            string srcString = httpPost(url, postString);
            if (srcString == null || srcString.Length == 0) {
                MessageBox.Show("获取失败~");
                return;
            }

            MessageBox.Show(srcString);

            listView1.Items.Clear();
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(srcString);
            
            HtmlNodeCollection nodes= doc.DocumentNode.SelectNodes("//li");
            foreach (HtmlNode anode in nodes)
            {
                string id = anode.GetAttributeValue("data-id", "null");
                string title = anode.SelectSingleNode("div/a[@class='title-txt']").InnerHtml;
                string author = anode.SelectSingleNode("div/a[@class='linfo-author']").InnerHtml.Replace("来自 ", "");
                string zone = anode.SelectSingleNode("div/a[@class='linfo-location']").InnerText;
                string time = anode.SelectSingleNode("div/span[@class='title-time']").InnerText;

                string visit = anode.SelectSingleNode("div/span[@class='linfo-visit']").InnerText.Replace("浏览 ","");
                string reply = anode.SelectSingleNode("div/span[@class='icon-reply']").InnerText;
                string support = anode.SelectSingleNode("div/span[@class='icon-support']").InnerText;

                ListViewItem lvi = new ListViewItem(id);
                lvi.SubItems.Add(title);
                lvi.SubItems.Add(reply);
                lvi.SubItems.Add(support);
                lvi.SubItems.Add(visit);
                lvi.SubItems.Add(author);
                lvi.SubItems.Add(time);
                lvi.SubItems.Add(zone);
                listView1.Items.Add(lvi);
                if (time.Contains("小时前")) {
                    string horus = time.Replace("小时前", "");
                    int horus_int = Convert.ToInt32(horus);
                    int reply_int=Convert.ToInt32(reply);
                    if (horus_int < 10 && reply_int > 20) {
                        lvi.BackColor = Color.Red;
                    } 
                }
                //MessageBox.Show(author + " " + title);
            }

            return;
        }

        //双击打开
        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if(listView1.SelectedItems.Count<=0){
		        MessageBox.Show("请选中！");
		        return;
	        }
            if (button_start.Enabled == false) {
                MessageBox.Show("运行中不允许设置id！");
                return;
            }

	        String id=(listView1.SelectedItems[0]).SubItems[0].Text;
	        textBox_articleid.Text=id;
	        tabControl1.SelectedIndex=0;
        }
     
        //listview1右键菜单：发送id
        private void 发送IDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1_DoubleClick(sender, e);
        }
        //listview1右键菜单：打开chrome
        private void 打开chromeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(listView1.SelectedItems.Count<=0){
		        MessageBox.Show("请选中！");
		        return;
	        }
	        String id=(listView1.SelectedItems[0]).SubItems[0].Text;
	        System.Diagnostics.Process np=new System.Diagnostics.Process();

	        np.StartInfo.FileName="chrome";
            np.StartInfo.Arguments = "http://chanpin.family.baidu.com/article/" + id;
	        np.Start();
        }


        //***********************************************************************
        /**
         * 以下是百分比抢楼助手
        */

        //更新当前楼数
        private void button_renewfloors_Click(object sender, EventArgs e)
        {
            String id = textBox_articleid2.Text.Trim();
            this.cookie = textBox_cookie.Text.Trim();
            if (id == "") 
            {
                MessageBox.Show("ID is error!");
                return;
            }
            String floor = getFloorNew(id);
            textBox_floors.Text = floor;
        }

        //转化为整数
        private int getInt(double num)
        {
	        if(checkBox_if45.Checked==false)
            {
		        return (int)num;
	        }
	        else
            {
		        return (int)(Math.Round(num));
	        }
        }

        //计算将要获奖的总楼层
        private void button_calc_Click(object sender, EventArgs e)
        {
            String alreadyget=textBox_alreadyget.Text;
	        String prizepct=textBox_persents.Text;
	        String id=textBox_articleid2.Text;
	        if(alreadyget=="" || prizepct=="" || id==""){
		        MessageBox.Show("已抢到楼层、百分比、id，均不能为空！");
		        return;
	        }

	        //1清空text
	        textBox_willget.Clear();
	        string[] arr_alreadyget = alreadyget.Split(',');
	        string[] arr_prizepct = prizepct.Split(',');
	        int len_alreadyget=arr_alreadyget.Length;
	        int len_prizepct=arr_prizepct.Length;

            //拿到当前楼层
            //button_renewfloors_Click(sender, e);
            int floornow = Convert.ToInt32(textBox_floors.Text);
	        //int floornow=Convert.ToInt32(getFloorOld(id),10);
            int prize_floor_ct = 0;//总共中奖次数
            //从当前楼层往后找300层
	        for(int i=floornow;i<floornow+300;i++){
                //保存中了几次
		        int bingo=0;
                //保存哪些比例中了
                StringBuilder sb = new StringBuilder(getSpace(6- (i+"").Length));
		        for(int j=0;j<len_prizepct;j++){
			        double pct=Convert.ToDouble(arr_prizepct[j]);
			        int real_bingo=getInt(pct*i);
			        String real_flo=""+real_bingo;

			        if(Array.IndexOf(arr_alreadyget,real_flo)>=0 ){
				        bingo++;
                        sb.Append(arr_prizepct[j]+getSpace(6-arr_prizepct[j].Length));
			        }
		        }

                if (bingo > 0)
                {
                    prize_floor_ct++;
                    textBox_willget.AppendText("" + i + sb + "\r\n");
                }
                else {
                    textBox_willget.AppendText("" + i + "\r\n");
                }
	        }

            //滚动到最上面
            textBox_willget.Select(0, 0);
            textBox_willget.ScrollToCaret();

            label_per_rate.Text = prize_floor_ct + "/300";
        }

        //打开计算器
        private void button_opencalc_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process np = new System.Diagnostics.Process();

            np.StartInfo.FileName = "calc";
            np.Start();
        }

        //一串空格
        private static string getSpace(int len) 
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < len; i++)
            {
                sb.Append(" ");
            }
            return sb.ToString();
        }

        //自动找到id
        private void textBox_articleid_Leave(object sender, EventArgs e)
        {
            string str = textBox_articleid.Text.Trim();
            if(str.Contains("="))
            {
                textBox_articleid.Text=str.Split('=')[1];
            }
        }

        //执行post
        private string httpPost(string post_url, string data_in)
        {
            HttpWebRequest request;
            //初始化httpwebrequest对象
            request = (HttpWebRequest)WebRequest.Create(post_url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = Encoding.UTF8.GetByteCount(data_in);

            request.ServicePoint.Expect100Continue = false;

            
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(new Cookie("JSESSIONID", textBox_cookie.Text, "/", "life.family.baidu.com"));

            byte[] data = (Encoding.GetEncoding("utf-8")).GetBytes(data_in);

            request.GetRequestStream().Write(data, 0, data.Length);

            HttpWebResponse res = (HttpWebResponse)request.GetResponse();

            Stream st = res.GetResponseStream();
            Encoding en = Encoding.GetEncoding("utf-8");
            StreamReader st_r = new StreamReader(st, en);
            string res_str = st_r.ReadToEnd();
            st.Close();
            st_r.Close();
            //MessageBox.Show(res_str);
            return res_str;

        }




        //***********************************************************************
        /**
         * 以下是交易
        */

        private void button_getsell_Click(object sender, EventArgs e)
        {


            string url = "http://dulife.baidu.com/pc/userCenter/secondHand/myPublicList.do?pageSize=999&pageNo=1";
            HttpWebRequest request;
            //初始化httpwebrequest对象
            request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";

            request.ServicePoint.Expect100Continue = false;


            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(new Cookie("DLSESSIONID", textBox_sell_cookie.Text.Trim(), "/", ".baidu.com"));


            HttpWebResponse res = (HttpWebResponse)request.GetResponse();

            Stream st = res.GetResponseStream();
            Encoding en = Encoding.GetEncoding("utf-8");
            StreamReader st_r = new StreamReader(st, en);
            string res_str = st_r.ReadToEnd();
            st.Close();
            st_r.Close();

            if (res_str == null || res_str.Length == 0 || res_str.StartsWith("<"))
            {
                MessageBox.Show("获取失败~");
                return;
            }
            //MessageBox.Show(res_str);

            listView_sell.Items.Clear();
            JObject res_j = (JObject)JsonConvert.DeserializeObject(res_str);
            if (res_j == null || ((string)res_j["success"]).Equals("false"))
            {
                MessageBox.Show("获取失败,请检查cookie.");
                return;
            }

            //MessageBox.Show((string)res_j["data"]["total"]);
            int selling = 0;

            JArray res_ja = (JArray)res_j["data"]["list"];
            for (int i = 0; i < res_ja.Count; i++) {
                string id = (string)res_ja[i]["goods_id"];
                string title = (string)res_ja[i]["sales_title"];
                string price = (string)res_ja[i]["sales_price"];
                string like_ct = (string)res_ja[i]["sales_like_num"];
                string comm_ct = (string)res_ja[i]["sales_msg_num"];
                string view_ct = (string)res_ja[i]["sales_view_num"];
                string all_ct = like_ct + "/" + comm_ct + "/" + view_ct;
                long timeStamp = (long)res_ja[i]["sales_create_time"];

                System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                dtDateTime = dtDateTime.AddSeconds(timeStamp).ToLocalTime();
                string time_str = dtDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                string status = (string)res_ja[i]["finish_status"];
                if (status.Equals("0")) {
                    selling++;
                }

                ListViewItem lvi = new ListViewItem(id);
                lvi.SubItems.Add(title);
                lvi.SubItems.Add(price);
                lvi.SubItems.Add(all_ct);
                lvi.SubItems.Add(time_str);
                lvi.SubItems.Add(status);
                listView_sell.Items.Add(lvi);
            }
            //MessageBox.Show(res_ja.Count + "个商品；\n在售中：" + selling + "件");
            return;
        }
        //双击list item
        private void listView_sell_DoubleClick(object sender, EventArgs e)
        {
            string id = listView_sell.SelectedItems[0].SubItems[0].Text;
            System.Diagnostics.Process np = new System.Diagnostics.Process();

            np.StartInfo.FileName = "chrome";
            np.StartInfo.Arguments = "http://ershou.dulife.baidu.com/Goods/detail/goods_id/" + id;
            np.Start();
        }
        //从浏览器打开二手市场
        private void button_gosell_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process np = new System.Diagnostics.Process();

            np.StartInfo.FileName = "chrome";
            np.StartInfo.Arguments = "http://ershou.dulife.baidu.com/";
            np.Start();
        }

 
        
        




    }

    
}
