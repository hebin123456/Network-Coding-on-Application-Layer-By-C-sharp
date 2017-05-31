/*
 * 由SharpDevelop创建。
 * 用户： 以夕阳落款
 * 日期: 2017/3/22
 * 时间: 20:18
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace NC
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		
		// 域名
		private string domain = string.Empty;
		
		private int Length;
		
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		// 获取文件的md5值
		private static string GetMD5HashFromFile(string fileName){
			try{
				FileStream file = new FileStream(fileName, FileMode.Open);
				System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
				byte[] retVal = md5.ComputeHash(file);
				file.Close();
				
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < retVal.Length; i++){
					sb.Append(retVal[i].ToString("x2"));
				}
				return sb.ToString();
			}
			catch (Exception ex){
				throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
			}
		}
		
		Matrix mx;
		byte[,] by, by2;
		// 块大小16MB
		long blocklength = 16777216;
		// 小块大小1MB
		int storelength = 1048576;
		// 服务器数量
		int servernum = 5;
		
		public void Button8Click(object sender, EventArgs e)
		{
			OpenFileDialog open = new OpenFileDialog();
			if(open.ShowDialog() == DialogResult.OK){
				string fullname = open.FileName;
				string md5 = GetMD5HashFromFile(fullname);
				
				// 如果存在文件就不上传了
				MySqlHelper mysqlHelper = new MySqlHelper();
				if(mysqlHelper.ExsistsFile(md5)){
					MessageBox.Show("文件已存在！");
					return;
				}
				
				string name = Path.GetFileName(fullname);
				FileInfo  f = new FileInfo(fullname);
				long length = f.Length;
				string command = string.Format("insert into file(Filename, Length, MD5) values('{0}', {1}, '{2}')", name, length, md5);
				
				mysqlHelper.Excute(command);
				
				int fileid = mysqlHelper.GetMaxNum("file");
				int blocknum;
				blocknum = (int)(length / blocklength);
				if(length % blocklength != 0) blocknum++;
				
				FileStream fs = new FileStream(fullname, FileMode.Open, FileAccess.Read);
				byte[] buffer;
				
				// 原始矩阵
				byte[,] original;
				
				// 编码后的矩阵
				byte[,] dest;
				
				// 矩阵的大小
				int n;
				// 冗余数量
				int redundant;
				mx = new Matrix();
				for (int i = 0; i < blocknum - 1; i++){
					command = string.Format("insert into block(fileid, Length, sequence) values('{0}', {1}, {2})", fileid, blocklength, i);
					mysqlHelper.Excute(command);
					
					buffer = new byte[blocklength];  
               		fs.Read(buffer, 0, buffer.Length);
               		
               		n = (int)(blocklength / storelength);
					redundant = getRedundant(n, servernum);
					
               		// 得到随机矩阵
               		by = mx.getRandomMatrix(mx.getIdentityMatrix(n));
			
					// 得到足够的冗余的编码矩阵
					by = mx.getRedundantRandomMatrix(by, redundant);
					
					// 原始数据排列
					original = new byte[n, storelength + 1];
					for(int j = 0; j < n; j++){
						for(int k = 0; k < storelength; k++){
							original[j, k] = buffer[j * storelength + k];
						}
						original[j, storelength] = (byte)j;
					}
					
					dest = mx.matrixMul(by, original);
					
					for(int j = 0; j < n + redundant; j++){
						byte[] temp = new byte[n + storelength + 1];
						for(int k = 0; k < n; k++)
							temp[k] = by[j, k];
						// 保存编码后块
						for(int k = 0; k < storelength + 1; k++)
							temp[k + n] = dest[j, k];
						FileStream fw;
						string newname = Guid.NewGuid().ToString();
						int blockid = mysqlHelper.GetMaxNum("block");
						command = string.Format("insert into store(blockid, serverid, name) values({0}, {1}, '{2}')", blockid, (j % 5), newname);
						mysqlHelper.Excute(command);
						fw = new FileStream("D:/upload/" + (j % servernum) + "/" + newname, FileMode.Create);
						fw.Write(temp, 0, temp.Length);
						//所有流类型都要关闭流，否则会出现内存泄露问题
						fw.Close();
					}
				}
				
				long last = length - (blocknum - 1) * blocklength;
				command = string.Format("insert into block(fileid, Length, sequence) values('{0}', {1}, {2})", fileid, last, blocknum - 1);
				mysqlHelper.Excute(command);
				
				buffer = new byte[last];
               	fs.Read(buffer, 0, buffer.Length);
               	
               	n = (int)(last / storelength);
               	
               	if(last % storelength != 0) n++;
				redundant = getRedundant(n, servernum);
           		// 得到随机矩阵
           		by = mx.getIdentityMatrix(n);
				by = mx.getRandomMatrix(by);
				
				// 得到足够的冗余的编码矩阵
				by = mx.getRedundantRandomMatrix(by, redundant);
				
				// 原始数据排列
				original = new byte[n, storelength + 1];
				for(int j = 0; j < n; j++){
					for(int k = 0; k < storelength; k++){
						if(j * storelength + k < buffer.Length) original[j, k] = buffer[j * storelength + k];
						else original[j,k] = 0;
					}
					original[j, storelength] = (byte)j;
				}
				
				dest = mx.matrixMul(by, original);
				
				for(int j = 0; j < n + redundant; j++){
					byte[] temp = new byte[n + storelength + 1];
					for(int k = 0; k < n; k++)
						temp[k] = by[j, k];
					// 保存编码后块
					for(int k = 0; k < storelength + 1; k++)
						temp[k + n] = dest[j, k];
					FileStream fw;
					string newname = Guid.NewGuid().ToString();
					int blockid = mysqlHelper.GetMaxNum("block");
					command = string.Format("insert into store(blockid, serverid, name) values({0}, {1}, '{2}')", blockid, (j % 5), newname);
					mysqlHelper.Excute(command);
					fw = new FileStream("D:/upload/" + (j % servernum) + "/" + newname, FileMode.Create);
					fw.Write(temp, 0, temp.Length);
					//所有流类型都要关闭流，否则会出现内存泄露问题
					fw.Close();
				}
				
				fs.Close();
			}
		}
		
		// 得到冗余数量
		// m:块数 n:存储器数
		public int getRedundant(int m, int n){
			int k = 1;
			for(;;k++){
				if((int)((m + k) / n) <= k - 2) break;
			}
			return k;
		}
		
		public void Button7Click(object sender, EventArgs e)
		{
			listView1.Items.Clear();
			MySqlHelper mysqlHelper = new MySqlHelper();
			MySqlDataReader mysqlread = mysqlHelper.ExcuteData("select * from file");
			int n = 0;
			while(mysqlread.Read()){
				ListViewItem lt = new ListViewItem((int)mysqlread[0] + "");
				lt.SubItems.Add((string)mysqlread[1]);
				lt.SubItems.Add((int)mysqlread[2] + "");
				lt.SubItems.Add((string)mysqlread[3]);
				listView1.Items.Add(lt);
				n++;
			}
			if(n == 0) MessageBox.Show("服务器上没有文件！");
		}
		
		private void ListView1MouseDoubleClick(object sender, MouseEventArgs e)
		{
			ListViewHitTestInfo info = listView1.HitTest(e.X, e.Y);
			if (info.Item != null){
				var item = info.Item as ListViewItem;
				MySqlHelper mysqlHelper = new MySqlHelper();
				int filenum = mysqlHelper.getFileNum(item.SubItems[3].Text);
				if(filenum == 0){
					MessageBox.Show("文件不存在！");
				}
				else{
					FolderBrowserDialog dialog = new FolderBrowserDialog();
		            dialog.Description = "请选择保存路径";
		            if (dialog.ShowDialog() == DialogResult.OK){
		                string foldPath = dialog.SelectedPath;
		                Download(filenum, foldPath, item.SubItems[1].Text);
		            }
				}
			}
		}
		
		private void Download(int filenum, string path, string name){
			MySqlHelper mysqlHelper = new MySqlHelper();
			List<int> block = new List<int>();
			MySqlDataReader mysqlread = mysqlHelper.ExcuteData("select id, Length from block where fileid = " + filenum);
			
			int n = 0;
			// 恢复每个块
			while(mysqlread.Read()){
				//MessageBox.Show((int)mysqlread[0] + "," + (int)mysqlread[1]);
				int blockid = (int)mysqlread[0];
				int length = (int)mysqlread[1];
				blockSave(blockid, length, n);
				n++;
			}
			
			// 拼接大文件
			byte []buffer = new byte[1024*100];
			FileStream outStream = new FileStream(path + @"\" + name, FileMode.Create);
            int readedLen = 0;
            FileStream srcStream = null;
            for (int i = 0; i < n; i++){
                srcStream = new FileStream(@"temp\" + i, FileMode.Open);
                while ((readedLen = srcStream.Read(buffer, 0, buffer.Length)) > 0){
                    outStream.Write(buffer, 0, readedLen);
                }
                srcStream.Close();
            }
            outStream.Close();
		}
		
		// 所有服务器正常
		private void blockSave(int blockid, int length, int n){
			string path = @"D:\upload\";
			MySqlHelper mysqlHelper = new MySqlHelper();
			FileStream fs;
			
			int storenum = (int)(length / storelength);
            if(length % storelength != 0) storenum++;
            
			mx = new Matrix();
			byte[,] by = new byte[storenum, storenum];
			byte[,] by2 = new byte[storenum, storelength + 1];
			
			MySqlDataReader mysqlread = mysqlHelper.ExcuteData("select serverid, name from store where blockid = " + blockid + " limit 0, " + storenum);
			int i = 0;
			while(mysqlread.Read()){
				fs = new FileStream(path + (int)mysqlread[0] + @"\" + (string)mysqlread[1],FileMode.Open,FileAccess.Read);
				byte[] fbytes = new byte[(int)fs.Length];
				fs.Read(fbytes, 0, fbytes.Length);
				for(int j = 0; j < storenum; j++)
					by[i, j] = fbytes[j];
				for(int j = 0; j < storelength + 1; j++)
					by2[i, j] = fbytes[j + storenum];
				fs.Close();
				i++;
			}
			
			// 编码矩阵求逆
			by = mx.inverseMatrix(by);
			
			// 解码
			byte[,] original = mx.matrixMul(by, by2);
			
			List<byte> byteSource = new List<byte>();
			
			Dictionary<int, byte[]> dic = new Dictionary<int, byte[]>();
			for(i = 0; i < storenum; i++){
				byte[] temp = new byte[storelength];
				for(int j = 0; j < storelength; j++){
					temp[j] = original[i, j];
				}
				dic.Add(original[i, storelength], temp);
				byteSource.AddRange(temp);
			}
			
			
			for(i = 0; i < dic.Count; i++){
				byteSource.AddRange(dic[i]);
			}
			
			byte[] data = byteSource.ToArray();
			
			
			fs = new FileStream(@"temp\" + n, FileMode.Create);
			//将byte数组写入文件中
			fs.Write(data, 0, length);
			//所有流类型都要关闭流，否则会出现内存泄露问题
			fs.Close();
		}
		
		// 有服务器宕机
		private void blockSave(int blockid, int length, int n, int breakid){
			
		}
		
		public void Button10Click(object sender, EventArgs e)
		{
			try
		    {
		         DirectoryInfo dir = new DirectoryInfo(@"D:\upload");
		         FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //返回目录中所有文件和子目录
		         foreach (FileSystemInfo i in fileinfo)
		         {
		             if (i is DirectoryInfo)            //判断是否文件夹
		             {
		                  DirectoryInfo subdir = new DirectoryInfo(i.FullName);
		                  subdir.Delete(true);          //删除子目录和文件
		             } 
		             else
		             {
		                  File.Delete(i.FullName);      //删除指定文件
		             }
		         }                
		    }
		    catch(Exception ex)
		    {
		         throw;
		    }
		    try{
			    Directory.CreateDirectory(@"D:\upload\0");
			    Directory.CreateDirectory(@"D:\upload\1");
			    Directory.CreateDirectory(@"D:\upload\2");
			    Directory.CreateDirectory(@"D:\upload\3");
			    Directory.CreateDirectory(@"D:\upload\4");
		    }
		    catch(Exception ex){
		    	
		    }
		}
		void Button11Click(object sender, EventArgs e)
		{
			Matrix mx = new Matrix();
			int n = 3;
			
			byte[,]	by = mx.getIdentityMatrix(n);
			int redundant = getRedundant(n, 5);
			by = mx.getRandomMatrix(by);
			
			// 得到足够的冗余的编码矩阵
			by = mx.getRedundantRandomMatrix(by, redundant);
		}
	}
}