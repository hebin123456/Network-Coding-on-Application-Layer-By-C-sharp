/*
 * 由SharpDevelop创建。
 * 用户： 何彬
 * 日期: 2017/5/24
 * 时间: 5:43
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.Collections;
using System.Configuration;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;

namespace NC
{
	/// <summary>
	/// Description of MySqlHelper.
	/// </summary>
	public class MySqlHelper
	{
		//数据库连接字符串
	    public static string Conn = "Database='newnc';Data Source='localhost';User Id='root';Password='root';charset='utf8';pooling=true";
		MySqlConnection myCon;
	    
	    public MySqlHelper(){
	    	myCon = new MySqlConnection(Conn);
	    }
	    
		public int GetMaxNum(string tablename)
	    {
			myCon.Open();
	        MySqlCommand mysqlcom = new MySqlCommand("select * from " + tablename + " order by id desc limit 0, 1", myCon);
	        MySqlDataReader mysqlread = mysqlcom.ExecuteReader(CommandBehavior.CloseConnection);
	        int result = 0;
	        if(mysqlread.Read()) result = (int)mysqlread[0];
	        myCon.Close();
	        return result;
	    }
		
		public void Excute(string command)
	    {
			myCon.Open();
	        MySqlCommand mysqlcom = new MySqlCommand(command, myCon);
	        mysqlcom.ExecuteNonQuery();
	        myCon.Close();
	    }
		
		public bool ExsistsFile(string md5){
			myCon.Open();
	        MySqlCommand mysqlcom = new MySqlCommand("select * from file where MD5 = '" + md5 + "'", myCon);
	        MySqlDataReader mysqlread = mysqlcom.ExecuteReader(CommandBehavior.CloseConnection);
	        bool result = false;
	        if(mysqlread.Read()) result = true;
	        myCon.Close();
	        return result;
		}
		
		public int getFileNum(string md5){
			myCon.Open();
	        MySqlCommand mysqlcom = new MySqlCommand("select id from file where MD5 = '" + md5 + "'", myCon);
	        MySqlDataReader mysqlread = mysqlcom.ExecuteReader(CommandBehavior.CloseConnection);
	        int result = 0;
	        if(mysqlread.Read()) result = (int)mysqlread[0];
	        myCon.Close();
	        return result;
		}
		
		public MySqlDataReader ExcuteData(string command)
	    {
			myCon.Open();
	        MySqlCommand mysqlcom = new MySqlCommand(command, myCon);
	        MySqlDataReader mysqlread = mysqlcom.ExecuteReader(CommandBehavior.CloseConnection);
	        //myCon.Close();
	        return mysqlread;
	    }
	}
}
