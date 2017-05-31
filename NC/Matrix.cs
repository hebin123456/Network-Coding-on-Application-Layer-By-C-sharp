/*
 * 由SharpDevelop创建。
 * 用户： 何彬
 * 日期: 2017/5/2
 * 时间: 23:51
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.Windows.Forms;

namespace NC
{
	/// <summary>
	/// Description of Matrix.
	/// </summary>
	public class Matrix
	{
		public Matrix()
		{
		}
		
		// 生成对角阵
		public byte[,] getIdentityMatrix(int n)
		{
			byte[,] randmatrix = new byte[n, n];
			int r = 0;
			int c = 0;
			
			for(r = 0; r < n; ++r){
				for(c = 0; c < n; ++c){
					if(r == c)
						randmatrix[r, c] = 1;
					else
						randmatrix[r, c] = 0;
				}
			}
			return randmatrix;
		}
		
		// 得到随机矩阵
		// 为保证得到的随机矩阵是可逆的，因此需要从一个对角阵开始处理
		public byte[,] getRandomMatrix(byte[,] randmatrix){
			int n = randmatrix.GetLength(0);
			Random rand = new Random();
			if(n == 1){
				byte m = (byte)rand.Next(0,255);
				randmatrix[0, 0] = mul(m, randmatrix[0, 0]);
				return randmatrix;
			}
			
			// 矩阵打乱次数
			int count = rand.Next(100, 200);
			for(int i = 0; i < count; i++){
				// 确定主行
				int mainRowNum = (int)(rand.Next(0, 200) % (n - 1));
				
				// 确定随机数和列
				byte temp = (byte)rand.Next(2, 255);
				byte[] tempArray = new byte[n];
				for(int j = 0; j < n; j++)
					tempArray[j] = mul(randmatrix[mainRowNum, j], temp);
				
				for(int j = 0; j < n; j++){
					for(int k = 0; k < n; k++){
						randmatrix[j, k] = add(randmatrix[j, k], tempArray[k]);
					}
				}
			}
			return randmatrix;
		}
		
		// 得到冗余编码矩阵
		// 做两行冗余
		// 一行为sum(αi)
		// 另一行为sum(iαi)
		public byte[,] getRedundantRandomMatrix(byte[,] a){
			int n = a.GetLength(0);
			byte[,] c = new byte[n + 2, n];
			for(int i = 0; i < n; i++){
				for(int j = 0; j < n; j++){
					c[i, j] = a[i, j];
				}
			}
			for(int i = 0; i < n; i++){
				c[n, i] = 0;
				for(int j = 0; j < n; j++){
					c[n, i] = add(c[n, i], c[j, i]);
				}
			}
			
			for(int i = 0; i < n; i++){
				c[n + 1, i] = 0;
				for(int j = 0; j < n; j++){
					c[n + 1, i] = add(c[n + 1, i], mul((byte)(j + 1), c[j, i]));
				}
			}
			
			return c;
		}
		
		// 得到冗余编码矩阵
		// 做m行冗余
		public byte[,] getRedundantRandomMatrix(byte[,] a, int m){
			int n = a.GetLength(0);
			byte[,] c = new byte[n + m, n];
			
			// 前n行保持原样
			for(int i = 0; i < n; i++){
				for(int j = 0; j < n; j++){
					c[i, j] = a[i, j];
				}
			}
			
			// 后面m行做不相关冗余
			for(int i = 0; i < m; i++){
				for(int j = 0; j < n; j++){
					c[n + i, j] = 0;
					for(int k = 0; k < n; k++){
						byte t = 1;
						for(int q = 0; q < i; q++)
							t = mul(t, (byte)(k + 1));
						t = mul(t, c[k, j]);
						c[n + i, j] = add(c[n + i, j], t);
					}
				}
			}
			
			return c;
		}
		
		// 有限域加法
		private byte add(byte a, byte b){
			return (byte)(a ^ b);
		}
		
		// 有限域减法
		private byte sub(byte a, byte b){
			return (byte)(a ^ b);
		}
		
		// 指数表
		private byte[] Logtable = { 0, 0, 25, 1, 50, 2, 26, 198, 75, 199, 27, 104, 51, 238, 223, 3, 100, 4, 224, 14, 52, 141, 129, 239, 76, 113, 8, 200, 248, 105, 28, 193, 125, 194, 29, 181, 249, 185, 39, 106, 77, 228, 166, 114, 154, 201, 9, 120, 101, 47, 138, 5, 33, 15, 225, 36, 18, 240, 130, 69, 53, 147, 218, 142, 150, 143, 219, 189, 54, 208, 206, 148, 19, 92, 210, 241, 64, 70, 131, 56, 102, 221, 253, 48, 191, 6, 139, 98, 179, 37, 226, 152, 34, 136, 145, 16, 126, 110, 72, 195, 163, 182, 30, 66, 58, 107, 40, 84, 250, 133, 61, 186, 43, 121, 10, 21, 155, 159, 94, 202, 78, 212, 172, 229, 243, 115, 167, 87, 175, 88, 168, 80, 244, 234, 214, 116, 79, 174, 233, 213, 231, 230, 173, 232, 44, 215, 117, 122, 235, 22, 11, 245, 89, 203, 95, 176, 156, 169, 81, 160, 127, 12, 246, 111, 23, 196, 73, 236, 216, 67, 31, 45, 164, 118, 123, 183, 204, 187, 62, 90, 251, 96, 177, 134, 59, 82, 161, 108, 170, 85, 41, 157, 151, 178, 135, 144, 97, 190, 220, 252, 188, 149, 207, 205, 55, 63, 91, 209, 83, 57, 132, 60, 65, 162, 109, 71, 20, 42, 158, 93, 86, 242, 211, 171, 68, 17, 146, 217, 35, 32, 46, 137, 180, 124, 184, 38, 119, 153, 227, 165, 103, 74, 237, 222, 197, 49, 254, 24, 13, 99, 140, 128, 192, 247, 112, 7};
		
		// 对数表
		private byte[] Alogtable = { 1, 3, 5, 15, 17, 51, 85, 255, 26, 46, 114, 150, 161, 248, 19, 53, 95, 225, 56, 72, 216, 115, 149, 164, 247, 2, 6, 10, 30, 34, 102, 170, 229, 52, 92, 228, 55, 89, 235, 38, 106, 190, 217, 112, 144, 171, 230, 49, 83, 245, 4, 12, 20, 60, 68, 204, 79, 209, 104, 184, 211, 110, 178, 205, 76, 212, 103, 169, 224, 59, 77, 215, 98, 166, 241, 8, 24, 40, 120, 136, 131, 158, 185, 208, 107, 189, 220, 127, 129, 152, 179, 206, 73, 219, 118, 154, 181, 196, 87, 249, 16, 48, 80, 240, 11, 29, 39, 105, 187, 214, 97, 163, 254, 25, 43, 125, 135, 146, 173, 236, 47, 113, 147, 174, 233, 32, 96, 160, 251, 22, 58, 78, 210, 109, 183, 194, 93, 231, 50, 86, 250, 21, 63, 65, 195, 94, 226, 61, 71, 201, 64, 192, 91, 237, 44, 116, 156, 191, 218, 117, 159, 186, 213, 100, 172, 239, 42, 126, 130, 157, 188, 223, 122, 142, 137, 128, 155, 182, 193, 88, 232, 35, 101, 175, 234, 37, 111, 177, 200, 67, 197, 84, 252, 31, 33, 99, 165, 244, 7, 9, 27, 45, 119, 153, 176, 203, 70, 202, 69, 207, 74, 222, 121, 139, 134, 145, 168, 227, 62, 66, 198, 81, 243, 14, 18, 54, 90, 238, 41, 123, 141, 140, 143, 138, 133, 148, 167, 242, 13, 23, 57, 75, 221, 124, 132, 151, 162, 253, 28, 36, 108, 180, 199, 82, 246, 1};
		
		// 有限域乘法
		private byte mul(byte a, byte b){
			/* multiply two elements of GF(2^m)*/
			if (a != 0 && b != 0) return Alogtable[(Logtable[a] + Logtable[b])%255];
			else return 0;
		}
		
		// 有限域除法
		private byte div(byte a, byte b){
			int j;
			if(a == 0) return 0;
			if((j = Logtable[a] - Logtable[b]) < 0) j += 255;
			return (Alogtable[j]);
		}
		
		// 有限域逆元
		private byte inv(byte a){
			/* 0 is self inverting */
			if(a == 0) return 0;
			else return Alogtable[(255 - Logtable[a])];
		}
		
		// 有限域矩阵乘法
		public byte[,] matrixMul(byte[,] a, byte[,] b){
			int m = a.GetLength(0);
			int n = b.GetLength(1);
			int t = a.GetLength(1);
			// c是m*n的矩阵
			byte[,] c = new byte[m, n];
			for(int i = 0; i < m; i++){  
                for(int j = 0; j < n; j++){  
					byte sum = mul(a[i, 0], b[0, j]);
                    for(int k = 1; k < t; k++){  
						sum = add(sum, mul(a[i, k], b[k, j]));
                    }  
                    c[i, j] = sum;  
                }
            }
			return c;
		}
		
		// 重排函数
		private byte[,] reorderOutput(byte[,] output){
			int dimension = output.GetLength(0);
			byte[,] temp = new byte[dimension, dimension];
		
			for(int i = 1; i < dimension; i++)
				for(int j = 0; j < dimension; j++)
					temp[i - 1, j] = output[i, j];
			for(int j = 0; j < dimension; j++)
					temp[dimension - 1, j] = output[0, j];
		
			for (int i = 0; i < dimension; i++)
				for(int j = 0; j < dimension; j++)
					output[i, j] = temp[i, j];
			return output;
		}
		
		// 矩阵交换
		private void swap(ref byte[,] input, ref byte[,] output, int first_row, int second_row){
			int dimension = input.GetLength(0);
			byte[] temp_row1 = new byte[dimension];
			byte[] temp_row2 = new byte[dimension];
			int i;
			for(i = 0; i < dimension; i++){
				temp_row1[i] = input[first_row, i];
				temp_row2[i] = output[first_row, i];
			}
			for (i = 0; i < dimension; i++)
			{
				input[first_row, i] = input[second_row, i];
				output[first_row, i] = output[second_row, i];
				input[second_row, i] = temp_row1[i];
				output[second_row, i] = temp_row2[i];
			}
		}
		
		// 有限域求逆矩阵
		public byte[,] inverseMatrix(byte[,] input){
			int dimension = input.GetLength(0);
			byte[,] output = new byte[dimension, dimension];
			
			int i, j, k;
			//将输出矩阵初始化为单位矩阵
			for(i = 0; i < dimension; i++){
				for(j = 0; j < dimension; j++)
					output[i, j] = 0;
				output[i, i] = 1;
			}
			for(i = 0; i < dimension; ++i){  //依次处理每一列
				for (j = 0; j < dimension; j++){  //如果当前行当前列值为0，做行变换
					if (input[j, i] != 0){
						swap(ref input, ref output, 0, j);
						break;
					}
				}
				for(j = 0; j < dimension; j++){  //依次处理每一行
					if(j == 0){  //如果是第一行，将input[j][i]设置为1，其他元素均除以input[i][i]
						for(k = dimension - 1; k >= 0; k--)
							output[j, k] = div(output[j, k], input[j, i]);
						for (k = dimension - 1; k >= i; k--)
							input[j, k] = div(input[j, k], input[j, i]);
					}
					else{  //如果不是第一行，将每一行的input[j][i]设置为0，该行其他元素都要倍数的减去第一行对应位置上的值
						for (k = dimension - 1; k >= 0; k--)
							output[j, k] = sub(output[j, k], mul(div(input[j, i], input[0, i]), output[0,k]));
						for (k = dimension - 1; k >= i; k--)
							input[j, k] = sub(input[j, k], mul(div(input[j, i], input[0, i]), input[0, k]));
					}
				}
				swap(ref input, ref output, 0, (i + 1) % dimension);  //每次都将下一次需要处理的行和当前的第一行交换
			}
			return reorderOutput(output); //因为之前的交换操作，行顺序乱了，需要重新排列一下，即把第一行的数据放到最后一行后面
		}
		
		// 有限域矩阵行列式的值计算
		/*private byte matrixValue(byte[,] a){
			int n = a.GetLength(0);
			byte result = 0;
			if(n == 1) return a[0, 0];
			byte[,] temp = new byte[n - 1, n - 1];
			for(int i=0;i<n;i++)
			{
			for(int j=0;j<n-1;j++)
			{
			for(int k=0;k<n-1;k++)
			{
			int flag;
			if(j<i)  flag=0;
			else flag=1;
			temp[j][k]=array[j+flag][k+1];
			}
			}
			byte flag2 = -1;
			if(i%2==0) flag2=1;
			result = add(result, mul(mul(flag2, a[i, 0]), matrixValue(temp));
			}
			return result;
		}*/
		
		/*private byte XTIME(byte x) {
			return (byte)((x << 1) ^ ((x & 0x80) != 0 ? 0x1b : 0x00));
		}
		
		// 有限域乘法		
		private byte multiply(byte a, byte b) {  
			byte[] temp = new byte[8];
			int i = 0;
			for(i = 0; i < 8; i++)
				temp[i] = a;
			byte tempmultiply = 0x00;  
			
			for(i = 1; i < 8; i++){  
				temp[i] = XTIME(temp[i - 1]);  
			}  
			tempmultiply = (byte)((b & 0x01) * a);
			for(i = 1; i <= 7; i++){  
				tempmultiply ^= (byte)(((b >> i) & 0x01) * temp[i]);
			}  
			return tempmultiply;  
		} */
	}
}
